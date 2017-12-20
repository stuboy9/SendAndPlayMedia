using AreaParty.clientPC;
using AreaParty.fileServece;
using log4net;
using NATUPNPLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace client
{
    public class ClientPCNew2
    {
        //private static String serverIp = "192.168.199.184";
        private static string serverIp = "119.23.12.116";
        //private static string serverIp = "127.0.0.1";
        private const int serverPort = 13456; //用于和pc主进程通信
        private const int serverTcpPort = 13333; //用于长连接服务器
        private const int serverHoleTcpPort = 3334; //用于短连接服务器打洞，通知服务器打洞是否成功
        private const string serverHoleUdpPort = "3335"; //用于短连接服务器通知udp端口和ip
        private static List<ShareFile> userBeanList = new List<ShareFile>();
        //private static bool canHole = true;
        private static bool isKeepAlive = true;
        private static string userId = "lh";
        private static string psw = "bb";
        private static TcpClient mainPcTcpClient = null;//用于和PC主进程通信
        private static TcpClient mainServerTcpClient = null;//用户和Server进行通信
        private static bool connect = true;
        private static Dictionary<string, string> fileSizeList = new Dictionary<string, string>();
        private static bool receiveMainPcFlag = true;
        private static ILog clientPClogger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Thread keepAliveThread;
        private static Thread receiveServerMsgThread;
        private static Thread refreshFileListThread;
        private static List<ClientPCState> clientPCStateList = new List<ClientPCState>();
        private static UPnPNAT upnpnat = new UPnPNAT();
        private static IStaticPortMappingCollection UPnPMappings;
        /// <summary>
        /// 日志初始化
        /// </summary>
        private static void InitLog4Net()
        {
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/clientPC/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logCfg);
        }
        /// <summary>
        /// 打洞程序入口
        /// </summary>
        public void StartMain()
        {
            try
            {
                InitLog4Net();
                
                IPAddress localIP = IPAddress.Parse("127.0.0.1");
                TcpListener tcpListener = new TcpListener(localIP,serverPort);
                tcpListener.Start();
                while (receiveMainPcFlag)
                {
                    Console.WriteLine("等待pc主界面连接");
                    mainPcTcpClient = tcpListener.AcceptTcpClient();//等待客户端连接
                    ListenPcMainThread listenPcMainThread = new ListenPcMainThread(mainPcTcpClient);
                    Thread listenPcMain = new Thread(listenPcMainThread.Run);
                    listenPcMain.Start();
                }
            }
            catch (Exception e)
            {
                clientPClogger.Info(e.ToString());
                
            }
        }
        public static void StartService(TcpClient tcpClient)
        {
            mainServerTcpClient = tcpClient;
            isKeepAlive = true;
            //心跳包发送线程
            KeepAlive keepAlive = new KeepAlive(mainServerTcpClient);
            keepAliveThread = new Thread(keepAlive.Run);
            keepAliveThread.IsBackground = true;
            keepAliveThread.Start();
            //接收服务器消息线程
            ReceiveServerMsg receiveServerMsg = new ReceiveServerMsg(mainServerTcpClient);
            receiveServerMsgThread = new Thread(receiveServerMsg.Run);
            receiveServerMsgThread.IsBackground = true;
            receiveServerMsgThread.Start();
            //刷新文件信息列表
            RefreshFileList refreshFileList = new RefreshFileList();
            refreshFileListThread = new Thread(refreshFileList.Run);
            refreshFileListThread.IsBackground = true;
            refreshFileListThread.Start();
        }

        public static void StopService()
        {
            isKeepAlive = false;
            ClearUpUPnPAll();
            try
            {
                if (mainServerTcpClient != null)
                {
                    mainServerTcpClient.Close();
                }
                if (keepAliveThread != null)
                {
                    keepAliveThread.Abort();
                    keepAliveThread.Join();
                }
                if (refreshFileListThread != null)
                {
                    refreshFileListThread.Abort();
                    refreshFileListThread.Join();
                }
                if (receiveServerMsgThread != null)
                {
                    receiveServerMsgThread.Abort();
                    receiveServerMsgThread.Join();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (keepAliveThread != null)
                {
                    keepAliveThread.Abort();
                }
                if (receiveServerMsgThread != null)
                {
                    receiveServerMsgThread.Abort();
                }
                if (refreshFileListThread != null)
                {
                    refreshFileListThread.Abort();
                }
                if (mainServerTcpClient != null)
                {
                    mainServerTcpClient.Close();
                }
            }
        }
        /// <summary>
        /// 初始化连接服务器并登录
        /// </summary>
        /// <param name="tcpClient"></param>
        private static void Init(TcpClient tcpClient)
        {
            BinaryWriter bw;
            try
            {
                IPAddress remoteIP = IPAddress.Parse(serverIp);
                mainServerTcpClient = new TcpClient();
                mainServerTcpClient.Connect(new IPEndPoint(remoteIP, serverTcpPort));
                clientPClogger.Info("连接服务器成功");
                Login(mainServerTcpClient, userId, psw);
                LoginRsp result = LoginResult(mainServerTcpClient);
                NetworkStream clientStream = tcpClient.GetStream();
                bw = new BinaryWriter(clientStream);
                if (LoginRsp.ResultCode.FAIL.Equals(result.resultCode))
                {
                    clientPClogger.Info("用户名或密码错误");
                    bw.Write("{\"status\":\"404\",\"message\":\"login fail\",\"data\":{\"failStyle\":\"userName or password error\"}}");
                    connect = false;
                    tcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.LOGGEDIN.Equals(result.resultCode))
                {
                    clientPClogger.Info("该账号已登录");
                    bw.Write("{\"status\":\"404\",\"message\":\"login fail\",\"data\":{\"failStyle\":\"the account is online\"}}");
                    connect = false;
                    mainServerTcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.NOTMAINPHONE.Equals(result.resultCode))
                {
                    clientPClogger.Info("该设备不是主设备");
                    bw.Write("{\"status\":\"404\",\"message\":\"login fail\",\"data\":{\"failStyle\":\"the device is not main pc\"}}");
                    AccreditRsp accreditResult = AccreditResult(mainServerTcpClient);
                    if (AccreditRsp.ResultCode.CANLOGIN.Equals(accreditResult.resultCode))
                    {
                        Login(mainServerTcpClient, userId, psw);
                         bw.Write("{\"status\":\"200\",\"message\":\"login success\",\"data\":{\"successStyle\":\"main device agree login\"}}");
                        connect = false;
                    }
                    else if (AccreditRsp.ResultCode.FAIL.Equals(accreditResult.resultCode))
                    {
                        bw.Write("{\"status\":\"404\",\"message\":\"login fail\",\"data\":{\"failStyle\":\"main device refuse login\"}}");
                        connect = false;
                        isKeepAlive = false;
                        tcpClient.Close();
                        return;
                    }
                }
                else if (LoginRsp.ResultCode.MAINPHONEOUTLINE.Equals(result.resultCode))
                {
                    clientPClogger.Info("主设备不在线");
                    bw.Write("{\"status\":\"404\",\"message\":\"login fail\",\"data\":{\"failStyle\":\"main device outline\"}}");
                    connect = false;
                    isKeepAlive = false;
                    mainServerTcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.SUCCESS.Equals(result.resultCode))
                {
                    clientPClogger.Info("登录成功");
                    bw.Write("{\"status\":\"200\",\"message\":\"login success\",\"data\":{\"successStyle\":\"main device agree login\"}");
                    connect = false;
                }
                isKeepAlive = true;
                KeepAlive keepAlive = new KeepAlive(mainServerTcpClient);
                new Thread(keepAlive.Run).Start();
                ReceiveServerMsg receiveServerMsg = new ReceiveServerMsg(mainServerTcpClient);
                new Thread(receiveServerMsg.Run).Start();
                RefreshFileList refreshFileList = new RefreshFileList();
                new Thread(refreshFileList.Run).Start();
            }
            catch (Exception e)
            {
                clientPClogger.Error(e.ToString());
            }
        }
        /// <summary>
        /// 用户和Server进行保活操作，每隔20S发送一个心跳包
        /// </summary>
        class KeepAlive
        {
            internal readonly string socketContent = "keep alive";
            internal TcpClient tcpClient;
            internal StreamWriter sw;
            internal KeepAlive(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
            }
            public virtual void Run()
            {
                clientPClogger.Info("进入保活程序");
                NetworkStream clientStream = tcpClient.GetStream();
                while (isKeepAlive)
                {
                    try
                    {
                        Thread.Sleep(20 * 1000); //20s发送一次心跳
                        //bw = new BinaryWriter(clientStream);
                        sw = new StreamWriter(clientStream);
                        sw.WriteLine(socketContent);
                        sw.Flush();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                //isKeepAlive = true;
                clientPClogger.Info("退出保活程序");
            }
        }

        public class ReceiveServerMsg
        {
            TcpClient tcpClient;
            StreamReader sr;
            StreamWriter sw;
            NetworkStream clientStream;
            //HolePunchRsp HolePunchRsp = new HolePunchRsp();
            
            public ReceiveServerMsg(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
            }
            public virtual void Run()
            {
                clientPClogger.Info("进入接收服务器消息程序");
                clientStream = tcpClient.GetStream();//利用TcpClient对象GetStream方法得到网络流
                //HolePunchReq holeReq = Serializer.Deserialize<HolePunchReq>(clientStream);
                sw = new StreamWriter(clientStream);
                sr = new StreamReader(clientStream);
                while (isKeepAlive)
                {
                    try
                    {
                        string msg = null;
                        try
                        {
                            msg = sr.ReadLine();
                            clientPClogger.Info("接收服务器消息msg-->" + msg);
                        }
                        catch (Exception e)
                        {
                            clientPClogger.Info(e.ToString());
                        }
                        if (msg != null)
                        {
                            
                            if (msg.Contains("getProgress"))
                            {
                                //    (new Thread(() =>
                                //{
                                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\Download\\FriendDownload";
                                // 路径
                                //    File f = new File(path);
                                //    if (!f.exists())
                                //    {
                                //        f.mkdirs();
                                //    }
                                //    File[] fa = f.listFiles();
                                //    List<ProgressObj> progressObjList = new ArrayList<ProgressObj>();
                                //    for (int i = 0; i < fa.Length; i++)
                                //    {
                                //        File fs = fa[i];
                                //        string style = fs.Name.substring(fs.Name.LastIndexOf(".") + 1);
                                //        ProgressObj progressObj = new ProgressObj();
                                //        if (style.Equals("temp"))
                                //        {
                                //            string str = "";
                                //            string totalSize = "";
                                //            string fileName = "";
                                //            try
                                //            {
                                //                str = FileOp.readTxtFile(new File(System.getProperty("user.home") + "\\AppData\\Local\\AreaParty\\saveFileSize.json"));
                                //                Gson gson = new Gson();
                                //                JsonParser parser = new JsonParser();
                                //                JsonArray jsonArray = parser.parse(str).AsJsonArray;
                                //                foreach (JsonElement file in jsonArray)
                                //                {
                                //                    FileSize fileSaveSize = gson.fromJson(file, typeof(FileSize));
                                //                    Console.WriteLine(fs.Name);
                                //                    Console.WriteLine(fileSaveSize.fileName);
                                //                    if (fileSaveSize.fileName.Substring(0, fs.Name.LastIndexOf(".")).Equals(fs.Name.substring(0, fs.Name.LastIndexOf("."))))
                                //                    {
                                //                        totalSize = fileSaveSize.fileSize;
                                //                        fileName = fileSaveSize.fileName;
                                //                        break;
                                //                    }
                                //                }
                                //            }
                                //            catch (Exception e)
                                //            {
                                //                // TODO Auto-generated catch block
                                //                Console.WriteLine(e.ToString());
                                //                Console.Write(e.StackTrace);
                                //            }
                                //            //String totalSize = fileSizeList.get(fs.getName().substring(0,fs.getName().lastIndexOf(".")));
                                //            string downloadedSize = (fs.length() / 1024).ToString();
                                //            double progress = fs.length() / 1024 * 100 / Convert.ToInt64(totalSize);
                                //            DecimalFormat df = new DecimalFormat("#.00");
                                //            progressObj.FileName = fileName;
                                //            progressObj.FileSize = downloadedSize;
                                //            progressObj.FileTotalSize = totalSize;
                                //            progressObj.Progress = df.format(progress) + "%";
                                //            progressObj.State = 0;
                                //        }
                                //        else
                                //        {
                                //            string totalSize = (fs.length() / 1024).ToString();
                                //            string downloadedSize = (fs.length() / 1024).ToString();
                                //            double progress = 100;
                                //            DecimalFormat df = new DecimalFormat("#.00");
                                //            progressObj.FileName = fs.Name;
                                //            progressObj.FileSize = downloadedSize;
                                //            progressObj.FileTotalSize = totalSize;
                                //            progressObj.Progress = df.format(progress) + "%";
                                //            progressObj.State = 1;
                                //        }
                                //        progressObjList.add(progressObj);
                                //    }
                                //    Gson gson = (new GsonBuilder()).create();
                                //    string s = gson.toJson(progressObjList);
                                //    Console.WriteLine(s);
                                //    pw.println("fileProcess:" + userId + "," + s);
                                //})).Start();
                                continue;
                            }
                            if (msg.Contains("deleteFile"))
                            {
                                string fileName = msg.Split(':')[1];
                                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\Download\\FriendDownload";
                                File.Delete(path + "\\" + fileName);
                                continue;
                            }
                            //if (!canHole)
                            //{
                            //    sw.WriteLine("server is busy");
                            //    sw.Flush();
                            //    continue;
                            //}
                            //else
                            //{
                            //    sw.WriteLine("server get");
                            //    sw.Flush();
                            //}
                            if (msg.Contains("send:"))
                            {
                                string fileDate = msg.Split(':')[1].Split(',')[0];
                                string filePath = "";
                                string senderId = msg.Split(':')[1].Split(',')[1];
                                string receiverId = msg.Split(':')[1].Split(',')[2];
                                string fileSize = msg.Split(':')[1].Split(',')[3];
                                string id = msg.Split(':')[1].Split(',')[4];
                                ClientPCState sendState = new ClientPCState(id);
                                //记录发送端状态信息
                                sendState.TransState = (int)StateEnum.State.Send;
                                for (int i = 0; i < userBeanList.Count; i++)
                                {
                                    if (fileDate.Equals(userBeanList[i].CreatTime))
                                    {
                                        filePath = userBeanList[i].WholePath;
                                        break;
                                    }
                                }
                                if (filePath.Equals(""))
                                {
                                    //sendState.FileState = (int)StateEnum.State.File_NoSuchFile;
                                    Console.WriteLine("noSuchFile");
                                    sw.WriteLine("noSuchFile," + receiverId + "," + id);
                                    sw.Flush();
                                    ////不能传输将保存的删除
                                    clientPCStateList.RemoveAll(s => s.Id == id);
                                    //canHole = true;
                                    continue;
                                }
                                sendState.FileDate = fileDate;
                                sendState.FilePath = filePath;
                                sendState.FileSize = fileSize;
                                sendState.SenderId = senderId;
                                sendState.ReceiverId = receiverId;
                                sendState.PunchState = (int)StateEnum.State.Punch_Stop;
                                clientPCStateList.Add(sendState);
                            }
                            //string receiveReady = sr.ReadLine();
                            //clientPClogger.Info("接收服务器消息receiveReady-->" + receiveReady);
                            else if (msg.Contains("readyReceive"))
                            {
                                string id = msg.Split(',')[1];
                                ClientPCState sendState = clientPCStateList.Find(s => s.Id == id);
                                int i = clientPCStateList.IndexOf(sendState);
                                if (i >= 0)
                                {
                                    clientPCStateList[i].PunchState = (int)StateEnum.State.Punch_ReadyStart;
                                    //sw.WriteLine("findFile," + senderId + "," + receiverId);
                                    sw.WriteLine("findFile," + sendState.SenderId + "," + sendState.ReceiverId + "," + id);
                                    sw.Flush();
                                }
                                else
                                {
                                    clientPClogger.Info(sendState + ":" + i + ":" + id);
                                }
                            }
                            else if (msg.Contains("fileExist"))
                            {
                                string id = msg.Split(',')[1];
                                clientPCStateList.RemoveAll(s => s.Id == id);
                                clientPClogger.Info("对方文件存在，请重试");
                                //canHole = true;
                                continue;
                            }
                            else if (msg.Contains("sendStart"))
                            {
                                string id = msg.Split(',')[1];
                                ClientPCState sendState = clientPCStateList.Find(s => s.Id == id);
                                if(sendState.PunchState == (int)StateEnum.State.Punch_ReadyStart)
                                {
                                    //SendServer sendServer = new SendServer(filePath, senderId, receiverId, fileSize);
                                    SendServer sendServer = new SendServer(sendState.FilePath, sendState.SenderId, sendState.ReceiverId, sendState.FileSize, sendState.Id);
                                    Thread sendServerThread = new Thread(sendServer.Run);
                                    sendServerThread.IsBackground = true;
                                    sendServerThread.Start();
                                }
                            }
                            else if (msg.Contains("receive:"))
                            {
                                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\Download\\FriendDownload";
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                string senderId = msg.Split(':')[1].Split(',')[1];
                                string receiverId = msg.Split(':')[1].Split(',')[2];
                                string fileTempName = msg.Split(':')[1].Split(',')[0].Substring(0, msg.Split(':')[1].Split(',')[0].LastIndexOf("."));
                                string fileSize = msg.Split(':')[1].Split(',')[3];
                                string id = msg.Split(':')[1].Split(',')[4];
                                ClientPCState receiveState = new ClientPCState(id);
                                receiveState.TransState = (int)StateEnum.State.Receive;
                                //clientPCState.FilePath = filePath;
                                receiveState.FileSize = fileSize;
                                receiveState.SenderId = senderId;
                                receiveState.ReceiverId = receiverId;
                                string sizeSave = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\saveFileSize.json";
                                if (!File.Exists(sizeSave))
                                {
                                    try
                                    {
                                        FileOp.CreateFile(sizeSave);
                                        List<FileSizeInfo> fsList = new List<FileSizeInfo>();
                                        FileSizeInfo fs = new FileSizeInfo
                                        {
                                            FileName = msg.Split(':')[1].Split(',')[0],
                                            FileSize = fileSize
                                        };
                                        fsList.Add(fs);
                                        FileOp.WriteTxtFile(JsonConvert.SerializeObject(fsList), sizeSave);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.ToString());
                                    }
                                }
                                else
                                {
                                    FileInfo fileInfo = new FileInfo(sizeSave);
                                    long len = fileInfo.Length;
                                    if (len != 0)
                                    {
                                        bool isExist = false;
                                        FileSizeInfo fs = new FileSizeInfo
                                        {
                                            FileName = msg.Split(':')[1].Split(',')[0],
                                            FileSize = fileSize
                                        };
                                        string s = "";
                                        try
                                        {
                                            s = FileOp.ReadTxtFile(sizeSave);
                                            List<FileSizeInfo> fsList = new List<FileSizeInfo>();
                                            FileSizeInfo[] fileSaveSize = JsonConvert.DeserializeObject<FileSizeInfo[]>(s);
                                            foreach (var item in fileSaveSize)
                                            {
                                                if (item.FileName.Equals(msg.Split(':')[1].Split(',')[0]))
                                                {
                                                    isExist = true;
                                                }
                                                fsList.Add(item);
                                            }
                                            if (!isExist)
                                            {
                                                fsList.Add(fs);
                                            }
                                            FileOp.WriteTxtFile(JsonConvert.SerializeObject(fsList), sizeSave);
                                        }
                                        catch (Exception e1)
                                        {
                                            clientPClogger.Info(e1.ToString());
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            List<FileSizeInfo> fsList = new List<FileSizeInfo>();
                                            FileSizeInfo fs = new FileSizeInfo
                                            {
                                                FileName = msg.Split(':')[1].Split(',')[0],
                                                FileSize = fileSize
                                            };
                                            fsList.Add(fs);
                                            FileOp.WriteTxtFile(JsonConvert.SerializeObject(fsList), sizeSave);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.ToString());
                                        }
                                    }
                                }
                                string fileType = msg.Split(':')[1].Split(',')[0].Substring(msg.Split(':')[1].Split(',')[0].LastIndexOf('.') + 1);
                                string fileTempPath = path + "\\" + fileTempName;
                                clientPClogger.Info("receive: path " + path + " senderId" + senderId + " receiverId" + receiverId + " fileTempName" + fileTempName + " fileType" + fileType + " fileTempPath" + fileTempPath);
                                string filePath = path + "\\" + msg.Split(':')[1].Split(',')[0];
                                receiveState.FilePath = filePath;
                                receiveState.FileTempPath = fileTempPath;
                                receiveState.FileType = fileType;
                                //clientPCStateList.Add(clientPCState);
                                if (File.Exists(filePath))
                                {
                                    clientPClogger.Info("文件存在");
                                    sw.WriteLine("fileExist," + senderId+","+ id);
                                    sw.Flush();
                                    clientPCStateList.RemoveAll(s => s.Id == id);
                                    //canHole = true;
                                    continue;
                                }
                                string fileProgress = path + "\\" + fileTempName + ".temp";
                                if (File.Exists(fileProgress))
                                {
                                    FileInfo fileInfo = new FileInfo(fileProgress);
                                    receiveState.FileOffset = fileInfo.Length;
                                    clientPClogger.Info("存在临时文件");
                                    receiveState.FileState = (int)StateEnum.State.File_TempExist;
                                }
                                receiveState.PunchState = (int)StateEnum.State.Punch_ReadyStart;
                                clientPCStateList.Add(receiveState);
                                clientPClogger.Info("接收方已做好接收准备");
                                sw.WriteLine("readyReceive," + senderId + ","+id);
                                sw.Flush();
                            }
                            //接受方后续程序
                            else if (msg.Contains("noSuchFile"))
                            {
                                string id = msg.Split(',')[1];
                                clientPCStateList.RemoveAll(s => s.Id == id);
                                clientPClogger.Info("对方没找到文件，请重试");
                                //canHole = true;
                                continue;
                            }
                            else if (msg.Contains("receiveStart"))
                            {
                                string id = msg.Split(',')[1];
                                ClientPCState receiveState = clientPCStateList.Find(s => s.Id == id);
                                //int i = clientPCStateList.IndexOf(receiveState);
                                if(receiveState.PunchState == (int)StateEnum.State.Punch_ReadyStart)
                                {
                                    //clientPClogger.Info("开始接收文件" + fileTempPath + ":" + senderId + ":" + receiverId + ":" + offset + ":" + fileType);
                                    //ReceiveServer receiveServer = new ReceiveServer(fileTempPath, senderId, receiverId, offset, fileType, fileSize);
                                    clientPClogger.Info("开始接收文件" + receiveState.FileTempPath + ":" + receiveState.SenderId + ":" + receiveState.ReceiverId + ":" + receiveState.FileOffset + ":" + receiveState.FileType);
                                    ReceiveServer receiveServer = new ReceiveServer(receiveState.FileTempPath, receiveState.SenderId, receiveState.ReceiverId, receiveState.FileOffset, receiveState.FileType, receiveState.FileSize, receiveState.Id);
                                    Thread receiveServerThread = new Thread(receiveServer.Run);
                                    receiveServerThread.IsBackground = true;
                                    receiveServerThread.Start();
                                }
                                //(new Thread(new receiveServer(fileTempPath, senderId, receiverId, offset, fileType, fileSize))).Start();
                            }
                            else
                            {

                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        clientPClogger.Info(e.ToString());
                        //canHole = true;
                        if (e.Message.Equals("Connection reset"))
                        {
                            clientPClogger.Info("服务器崩溃");
                            try
                            {
                                isKeepAlive = false;
                                tcpClient.Close();
                            }
                            catch (IOException e1)
                            {
                                Console.WriteLine(e1.ToString());
                            }
                            return;
                        }
                        else if (e.Message.Equals("Socket closed"))
                        {
                            //canHole = true;
                            clientPClogger.Info("连接断开");
                        }
                        else
                        {
                            //canHole = true;
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
                clientPClogger.Info("退出接收服务器消息程序");
            }
        }
        private static AccreditRsp AccreditResult(TcpClient tcpClient)
        {
            AccreditRsp result = null;
            BinaryReader br;
            try
            {
                byte[] byteArray = new byte[10000];
                NetworkStream clientStream = tcpClient.GetStream();
                br = new BinaryReader(clientStream);
                int len = br.Read(byteArray, 0, byteArray.Length);
                byte[] data = new byte[len];

                Array.Copy(byteArray, data, len);
                MemoryStream ms = new MemoryStream(data);
                result = Serializer.Deserialize<AccreditRsp>(ms);
            }
            catch (IOException e)
            {
                clientPClogger.Error(e.ToString());
            }
            finally
            {
                
            }
            return result;
        }
        private static void Login(TcpClient tcpClient, String userId, String psw)
        {
            BinaryWriter bw;
            LoginReq loginReq = new LoginReq
            {
                userId = userId,
                userPassword = psw,
                loginType = LoginReq.LoginType.PC,
                userMac = GetMacAddress().Split(',')[0],
                mobileInfo = GetMacAddress().Split(',')[1]
            };
            MemoryStream ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, loginReq);
            byte[] data = ms.ToArray();
            NetworkStream clientStream = tcpClient.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write(data, 0, data.Length);
            bw.Flush();
            clientPClogger.Info("发送登录请求");
        }
        /// <summary>
        /// PC端打洞程序主线程
        /// </summary>
        class ListenPcMainThread
        {
            internal TcpClient tcpClient;
            internal ListenPcMainThread(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
            }
            public void Run()
            {
                string jsonText = null;
                BinaryReader br;
                try
                {
                    Console.WriteLine(tcpClient);
                    clientPClogger.Info("开启新线程");
                    connect = true;
                    JObject json;
                    byte[] buffer = new byte[1024];
                    try
                    {
                        NetworkStream clientStream = tcpClient.GetStream();//利用TcpClient对象GetStream方法得到网络流
                        br = new BinaryReader(clientStream);
                        jsonText = br.ReadString();//读取
                        Console.WriteLine("receiveNumber" + jsonText);
                        json = (JObject)JsonConvert.DeserializeObject(jsonText);//将json数据转为为String型的数据

                        string name = json["name"].ToString();
                        string command = json["command"].ToString();
                        string paramName = json["param"]["name"].ToString();
                        string paramPassword = json["param"]["password"].ToString();
                        clientPClogger.Info(name + " " + command + " " + paramName + " " + paramPassword);
                        Console.WriteLine(name + " " + command + " " + paramName + " " + paramPassword);
                        if (command.Equals("login"))
                        {
                            userId = paramName;
                            psw = paramPassword;
                            Init(tcpClient);
                        }
                        else if (command.Equals("close"))
                        {
                            mainPcTcpClient.Close();
                            receiveMainPcFlag = false;
                            isKeepAlive = false;
                            connect = false;
                            if (tcpClient != null)
                            {
                                tcpClient.Close();
                            }
                            Console.WriteLine("close");
                            return;
                        }
                        else if (command.Equals("logout"))
                        {
                            isKeepAlive = false;
                            tcpClient.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
                catch (Exception e)
                {
                    clientPClogger.Error(e.ToString());
                    tcpClient.Close();
                }
            }
        }
        /// <summary>
        /// 获取登录状态结果
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns>LoginRsp</returns>
        private static LoginRsp LoginResult(TcpClient tcpClient)
        {
            LoginRsp result = null;
            BinaryReader br;
            try
            {
                byte[] byteArray = new byte[2048];
                NetworkStream clientStream = tcpClient.GetStream();
                br = new BinaryReader(clientStream);
                int len = br.Read(byteArray, 0, byteArray.Length);
                byte[] data = new byte[len];
                Array.Copy(byteArray, data, len);
                MemoryStream ms1 = new MemoryStream(data);
                result = Serializer.Deserialize<LoginRsp>(ms1);
            }
            catch (IOException e)
            {
                clientPClogger.Error(e.ToString());
            }
            finally
            {
                
            }
            return result;
        }
        /// <summary>
        /// 获取PCMac地址
        /// </summary>
        /// <returns>string mac</returns>
        public static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] bytes = nic.GetPhysicalAddress().GetAddressBytes();
                    foreach (sbyte b in bytes)
                    {
                        //与11110000作按位与运算以便读取当前字节高4位  
                        sb.Append(((b & 240) >> 4).ToString("x"));
                        //与00001111作按位与运算以便读取当前字节低4位  
                        sb.Append((b & 15).ToString("x"));
                        sb.Append("-");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    return sb.ToString().ToUpper() + "," + Dns.GetHostName();
                }
            }
            return null;
        }
        /// <summary>
        /// 读取最新的分享文件列表
        /// </summary>
        public class RefreshFileList
        {
            public void Run()
            {
                clientPClogger.Info("进入刷新文件列表程序");
                userBeanList = new List<ShareFile>();
                while (isKeepAlive)
                {
                    userBeanList.Clear();
                    StreamReader reader = null;
                    string laststr = "";
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\sharedFilePathInfor.audio";
                        if (!File.Exists(path))
                        {
                            File.Create(path);
                        }
                        FileStream fileInputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                        reader = new StreamReader(fileInputStream);
                        string tempString = null;
                        while ((tempString = reader.ReadLine()) != null)
                        {
                            laststr += tempString;
                        }
                        reader.Close();
                        ShareFile[] descJsonStu = JsonConvert.DeserializeObject<ShareFile[]>(laststr);//反序列化
                        if(descJsonStu != null)
                        {
                            foreach (var item in descJsonStu)
                            {
                                userBeanList.Add(item);
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            try
                            {
                                reader.Close();
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                    try
                    {
                        Thread.Sleep(60000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                clientPClogger.Info("退出刷新文件列表程序");
            }

        }
        /// <summary>
        /// 分享文件
        /// </summary>
        public class ShareFile
        {
            private readonly ClientPCNew2 outerInstance;

            public ShareFile(ClientPCNew2 outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            string creatTime;
            string wholePath;

            public string CreatTime { get => creatTime; set => creatTime = value; }
            public string WholePath { get => wholePath; set => wholePath = value; }
        }

        /// <summary>
        /// 已经传输的文件大小信息
        /// </summary>
        public class FileSizeInfo
        {
            string fileSize;
            string fileName;

            public string FileSize { get => fileSize; set => fileSize = value; }
            public string FileName { get => fileName; set => fileName = value; }

            public override string ToString()
            {
                return "fileSize "+ fileSize+ "fileName "+ fileName;
            }
        }
        /// <summary>
        /// 发送打洞信息
        /// </summary>
        /// <param name="receiveHost"></param>
        /// <param name="udpClient"></param>
        /// <param name="message"></param>
        public static void SendHoleMsg(IPEndPoint receiveHost, UdpClient udpClient, string message)
        {
            try
            {
                byte[] sendData = Encoding.UTF8.GetBytes(message);
                udpClient.Send(sendData, sendData.Length, receiveHost);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// 发送方打洞线程
        /// </summary>
        private class SendServer
        {
            internal string filePath;//文件路径
            internal string senderId;//发送方ID
            internal string receiverId;//接收方ID
            UdpClient udpClient;
            internal bool flagSend = true;
            internal bool getReceiveMsg = false;
            internal bool getSuccess = false;
            internal int sendCount = 0;
            internal string fileSize;
            internal string id;
            //internal string externalIp;
            //internal string externalPort;
            internal SendServer(string filePath, string senderId, string receiverId, string fileSize,string id)
            {
                this.filePath = filePath;
                this.senderId = senderId;
                this.receiverId = receiverId;
                this.fileSize = fileSize;
                this.id = id;
                clientPClogger.Info("sendServer:" + "filePath-" + filePath + " senderId-" + senderId + " receiverId-" + receiverId + " id-" + id);
            }
            public void Run()
            {
                try
                {
                    //UDP连接服务器
                    udpClient = new UdpClient();
                    clientPClogger.Info("sendHoleMsg" + serverIp + ":" + serverHoleUdpPort);
                    IPAddress HostIP = IPAddress.Parse(serverIp);
                    IPEndPoint serverEP = new IPEndPoint(HostIP, int.Parse(serverHoleUdpPort));
                    SendHoleMsg(serverEP, udpClient, id + "," + senderId);
                    //upnp
                    AttemptUPnP(((IPEndPoint)(udpClient.Client.LocalEndPoint)).Port, ((IPEndPoint)(udpClient.Client.LocalEndPoint)).Port);
                    bool udpConnect = false;
                    while (!udpConnect)
                    {
                        try
                        {
                            udpClient.Client.ReceiveTimeout = 2000;
                            byte[] bufSend = new byte[1024];
                            byte[] receiveBuffer = udpClient.Receive(ref serverEP);
                            string receiveMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
                            clientPClogger.Info("receiveMessage" + receiveMessage);
                            if (receiveMessage.Equals("get"))
                            {
                                //string[] param = receiveMessage.Split(',');
                                //externalIp = param[1];
                                //externalPort = param[]
                                udpConnect = true;
                            }
                        }
                        catch (Exception e)
                        {
                            clientPClogger.Error(e.ToString());
                            SendHoleMsg(serverEP, udpClient, id + "," + senderId);
                            continue;
                        }
                    }
                    //TCP短连接连接服务器
                    TcpClient tcpClient = new TcpClient();
                    IPAddress remoteIP = IPAddress.Parse(serverIp);//远程主机IP
                    tcpClient.Connect(new IPEndPoint(remoteIP, serverHoleTcpPort)); //配置服务器IP与端口
                    NetworkStream clientStream = tcpClient.GetStream();
                    //BinaryWriter写入字符串是以UTF-7编码写入，并在字符串前添加长度前缀，所以此处需要用StreamWriter
                    StreamReader sr = new StreamReader(clientStream);
                    StreamWriter sw = new StreamWriter(clientStream);
                    sw.WriteLine("I am " + id + "," + senderId);
                    sw.Flush();
                    if (sr.ReadLine().Equals("welcome"))
                    {
                        clientPClogger.Info("welcome");
                        sw.WriteLine("senderPeer:" + id + "," + senderId);
                        sw.Flush();

                        string peer = sr.ReadLine();
                        clientPClogger.Info(peer);
                        string Msg = peer.Substring(8);
                        string[] param = Msg.Split(',');
                        string receiverIp = param[0].Substring(5);
                        string receiverPort = param[1].Substring(5);

                        clientPClogger.Info(receiverIp + ":" + receiverPort);
                        Thread.Sleep(2000);
                        IPAddress receiverIP = IPAddress.Parse(receiverIp);
                        //IPEndPoint receiverEP = new IPEndPoint(receiverIP, int.Parse(receiverPort));
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        IPEndPoint receiverEP = new IPEndPoint(receiverIP, int.Parse(receiverPort));
                        //IPEndPoint trueReceiverEP = new IPEndPoint(receiverIP, int.Parse(receiverPort));
                        //向receiverEP发送端口预测端口 + - 5
                        //for (int i = 0; i < 11; i++)
                        //{
                        //    try
                        //    {
                        //        int port = (int.Parse(receiverPort) - 5 + i);
                        //        if (port > 1024 && port < 65536)
                        //        {
                        //            receiverEP = new IPEndPoint(receiverIP, port);
                        //            SendHoleMsg(receiverEP, udpClient, "IAmSender,"+id);
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        clientPClogger.Error(e);
                        //    }
                        //}
                        SendHoleMsg(receiverEP, udpClient, "IAmSender");
                        Mutex mutex = new Mutex();
                        long offset = 0;
                        udpClient.Client.ReceiveTimeout = 500;
                        //UDP用户打洞
                        while (flagSend && sendCount < 10)
                        {
                            try
                            {
                                if (getReceiveMsg && getSuccess)
                                {
                                    //(new Thread(new send2(filePath, udpSocket, receiverIp, receiverPort, offset, fileSize))).Start
                                    SendFile sendFile = new SendFile(filePath, udpClient, receiverEP, fileSize, offset, id);
                                    Thread sendFileThread = new Thread(sendFile.Run);
                                    sendFileThread.IsBackground = true;
                                    sendFileThread.Start();
                                    //canHole = true;
                                    tcpClient.Close();
                                    flagSend = false;
                                    break;
                                }
                                udpClient.Client.ReceiveTimeout = 3000;
                                byte[] receiveBuffer = udpClient.Receive(ref remoteEP);
                                string receiveMessage = Encoding.Default.GetString(receiveBuffer);
                                clientPClogger.Info("receiveMessage " + receiveMessage);
                                if (receiveMessage.Contains("IAmReceiver"))
                                {
                                    //将接收到的用户地址记录下来
                                    receiverEP = remoteEP;
                                    clientPClogger.Info("receiver1:" + receiverEP.Address + ":" + receiverEP.Port);
                                    (new Thread(() =>
                                    {
                                        mutex.WaitOne();
                                        if (tcpClient.Connected)
                                        {
                                            try
                                            {
                                                clientPClogger.Info("发送成功消息");
                                                sw.WriteLine("senderSuccess" + id + "," + senderId);
                                                sw.Flush();
                                                string msg = sr.ReadLine();
                                                clientPClogger.Info("holeMessage:" + msg);
                                                if (("success").Equals(msg))
                                                {
                                                    getSuccess = true;
                                                }
                                                getReceiveMsg = true;
                                            }
                                            catch (Exception e)
                                            {
                                                //canHole = true;
                                                clientPClogger.Info(e.ToString());
                                            }
                                        }
                                        mutex.ReleaseMutex();
                                    })).Start();
                                    offset = long.Parse(receiveMessage.Split(',')[1]);
                                }

                                if (receiveMessage.Equals("lostMsg"))
                                {
                                    receiverEP = remoteEP;
                                    clientPClogger.Info("receiver2:" + receiverEP.Address + ":" + receiverEP.Port);
                                    SendHoleMsg(receiverEP, udpClient, "IAmSender");
                                }

                                //if (getReceiveMsg && getSuccess)
                                //{
                                //    SendFile sendFile = new SendFile(filePath, udpClient, trueReceiverEP, fileSize, offset, id);
                                //    Thread sendFileThread = new Thread(sendFile.Run);
                                //    sendFileThread.IsBackground = true;
                                //    sendFileThread.Start();
                                //    tcpClient.Close();
                                //    flagSend = false;
                                //    break;
                                //}
                                //byte[] receiveBuffer = udpClient.Receive(ref remoteEP);
                                //string receiveMessage = Encoding.UTF8.GetString(receiveBuffer);
                                //clientPClogger.Info("receiveMessage " + receiveMessage);
                                //if (receiveMessage.Contains("IAmReceiver"))
                                //{
                                //    string strId = receiveMessage.Split(',')[1];
                                //    clientPClogger.Info("IAmReceiver---" + strId + "---" + id);
                                //    if (strId.Equals(id))//id验证
                                //    {
                                //        //将接收到的用户地址记录下来
                                //        trueReceiverEP = remoteEP;
                                //        getReceiveMsg = true;
                                //        clientPClogger.Info("receiver1:" + receiverEP.Address + ":" + receiverEP.Port);
                                //        (new Thread(() =>
                                //        {
                                //            mutex.WaitOne();
                                //            if (tcpClient.Connected)
                                //            {
                                //                try
                                //                {
                                //                    clientPClogger.Info("发送成功消息");
                                //                    sw.WriteLine("senderSuccess" + id + "," + senderId);
                                //                    sw.Flush();
                                //                    string msg = sr.ReadLine();
                                //                    clientPClogger.Info("holeMessage:" + msg);
                                //                    if (("success").Equals(msg))
                                //                    {
                                //                        getSuccess = true;
                                //                    }
                                //                }
                                //                catch (Exception e)
                                //                {
                                //                    //canHole = true;
                                //                    clientPClogger.Info(e.ToString());
                                //                }
                                //            }
                                //            mutex.ReleaseMutex();
                                //        })).Start();
                                //        offset = long.Parse(receiveMessage.Split(',')[2]);
                                //    }
                                //}
                                ////else if (receiveMessage.Contains("lostMsg"))
                                ////{
                                ////    string strId = receiveMessage.Split(',')[1];
                                ////    if (strId.Equals(id))//id验证
                                ////    {
                                ////        receiverEP = remoteEP;
                                ////        SendHoleMsg(receiverEP, udpClient, "IAmSender," + id);
                                ////        clientPClogger.Info("receiver2:" + receiverEP.Address + "," + receiverEP.Port);
                                ////    }
                                ////}
                            }
                            catch (Exception e)
                            {
                                clientPClogger.Info(e.ToString());
                                if (!getReceiveMsg)
                                {
                                    SendHoleMsg(receiverEP, udpClient, "lostMsg");
                                }
                                clientPClogger.Info("连接超时--------" + sendCount);
                                sendCount++;
                                if (sendCount == 10)
                                {
                                    string[] path = Regex.Split(filePath, "\\\\");
                                    string fileName = path[path.Length - 1];
                                    sw.WriteLine("holeFail," + senderId + "," + receiverId + "," + fileName);
                                    sw.Flush();
                                    clientPClogger.Info("holeFail");
                                    tcpClient.Close();
                                    //canHole = true;
                                }
                                //                                clientPClogger.Info(e.Message);
                                //                                udpClient.Client.ReceiveTimeout += 200;
                                //                                if (!getReceiveMsg)//如果没有收到对方打洞信息，端口再次预测
                                //                                {
                                //                                    //SendHoleMsg(receiverEP, udpClient, "lostMsg");
                                //                                    //for (int i = 0; i < 11; i++)
                                //                                    //{
                                //                                    //    int port = (int.Parse(receiverPort) - 5 + i);
                                //                                    //    if (port > 1024 && port < 65536)
                                //                                    //    {
                                ////                                            receiverEP = new IPEndPoint(receiverIP, int.Parse(receiverPort));
                                //                                            SendHoleMsg(receiverEP, udpClient, "IAmSender," + id);
                                //                                    //    }
                                //                                    //}
                                //                                }
                                //                                else//如果收到对方打洞信息,直接发送
                                //                                {
                                //                                    SendHoleMsg(trueReceiverEP, udpClient, "IAmSender," + id);
                                //                                }
                                //                                clientPClogger.Info("连接超时--------" + sendCount);
                                //                                sendCount++;
                                //                                if (sendCount == 10)
                                //                                {
                                //                                    string[] path = Regex.Split(filePath, "\\\\");
                                //                                    string fileName = path[path.Length - 1];
                                //                                    sw.WriteLine("holeFail," + senderId + "," + receiverId + "," + fileName + "," + id);
                                //                                    sw.Flush();
                                //                                    clientPClogger.Info("holeFail");
                                //                                    tcpClient.Close();
                                //                                    //canHole = true;
                                //                                }
                                continue;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //canHole = true;
                    clientPClogger.Error(e.ToString());
                }
            }

        }

        private static bool AttemptUPnP(int iPort, int ePort)
        {
            UPnPMappings = upnpnat.StaticPortMappingCollection;
            if (UPnPMappings == null)
            {
                clientPClogger.Info("没有检测到路由器，或者路由器不支持UPnP功能");
                return false;
            }
            else
            {
                try
                {
                    string localIP = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
                    UPnPMappings.Add(ePort, "UDP", iPort, localIP, true, "AreaParty");
                    clientPClogger.Info("UPnp success-->iport=" + iPort + "eport=" + ePort);
                    return true;
                }
                catch
                {
                    clientPClogger.Info("UPnp fail");
                    return false;
                }
            }
        }
        public static void ClearUpUPnP(int eport)
        {
            UPnPMappings = upnpnat.StaticPortMappingCollection;
            if (UPnPMappings != null)
            {
                try
                {
                    UPnPMappings.Remove(eport, "UDP");
                    clientPClogger.Info("remove upnp success " + eport);
                }
                catch (Exception ex)
                {
                    clientPClogger.Error(ex);
                }
            }
        }
        public static void ClearUpUPnPAll()
        {
            UPnPMappings = upnpnat.StaticPortMappingCollection;
            if (UPnPMappings != null)
            {
                List<int> PortMappingsToDelete = new List<int>();

                foreach (IStaticPortMapping map in UPnPMappings)
                {
                    try
                    {
                        if (map.Protocol == "UDP" && map.Description == "AreaParty")
                            PortMappingsToDelete.Add(map.ExternalPort);
                    }
                    catch
                    {

                    }
                }

                foreach (int port in PortMappingsToDelete)
                {
                    try
                    {
                        UPnPMappings.Remove(port, "UDP");
                        clientPClogger.Info("remove upnp success "+port);
                    }
                    catch (Exception ex)
                    {
                        clientPClogger.Error(ex);
                    }
                }
            }
        }
        /// <summary>
        /// 发送文件线程
        /// </summary>
        private class SendFile
        {
            private String filePath;
            private UdpClient senderClient;
            //private String receiverIp;
            //private String receiverPort;
            private IPEndPoint receiverEP;
            private String fileSize;
            private long offset;
            private string id;

            public SendFile(string filePath, UdpClient senderClient, IPEndPoint trueReceiverEP, string fileSize, long offset, string id)
            {
                this.filePath = filePath;
                this.senderClient = senderClient;
                //this.receiverIp = receiverIp;
                //this.receiverPort = receiverPort;
                this.receiverEP = trueReceiverEP;
                this.fileSize = fileSize;
                this.offset = offset;
                this.id = id;
            }

            public void Run()
            {
                Console.WriteLine("开始发送");
                SendState sendState = new SendState();
                SendLittle sendLittle = new SendLittle(senderClient, receiverEP, sendState);
                byte[] buff = new byte[1024 * 1024];
                FileStream sFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                sFile.Seek(offset, SeekOrigin.Begin);
                sendState.ExistFileLength = offset;
                sendState.FileLength = long.Parse(fileSize);
                while (isKeepAlive)
                {
                    int i = sFile.Read(buff, 0, buff.Length); //第一个参数是被传进来的字节数组,用以接受FileStream对象中的数据,第2个参数是字节数组中开始写入数据的位置,它通常是0,表示从数组的开端文件中向数组写数据,最后一个参数规定从文件读多少字符.
                    Console.WriteLine(i);
                    if (i == 0)
                    {
                        String end = "end";
                        ByteChange.StringToByte(end, buff, 0, 3);
                        sendLittle.SendAll(buff, 3);
                        break;
                    }
                    sendLittle.SendAll(buff, i);
                }
                sFile.Close();
                sFile.Dispose();
                clientPCStateList.RemoveAll(s => s.Id == id);
                ClearUpUPnP(((IPEndPoint)senderClient.Client.LocalEndPoint).Port);
            }
        }
        /// <summary>
        /// 接受者打洞线程
        /// </summary>
        private class ReceiveServer
        {
            internal string filePath;
            internal string senderId;
            internal string receiverId;
            internal string fileType;
            internal UdpClient udpClient;
            internal bool flagSend = true;
            internal bool getSendMsg = false;
            internal bool getSuccess = false;
            internal int sendCount = 0;
            internal string fileSize;
            internal long offset;
            internal string id;
            internal ReceiveServer(string filePath, string senderId, string receiverId, long offset, string fileType, string fileSize, string id)
            {
                this.filePath = filePath;
                this.senderId = senderId;
                this.receiverId = receiverId;
                this.offset = offset;
                this.fileType = fileType;
                this.fileSize = fileSize;
                this.id = id;
                clientPClogger.Info("receiveServer:" + "filePath-" + filePath + " senderId-" + senderId + " receiverId-" + receiverId + " offset-" + offset + " fileType-" + fileType + " id-" + id);
            }
            public void Run()
            {
                try
                {
                    udpClient = new UdpClient();
                    IPAddress HostIP = IPAddress.Parse(serverIp);
                    IPEndPoint serverEP = new IPEndPoint(HostIP, int.Parse(serverHoleUdpPort));
                    SendHoleMsg(serverEP, udpClient, id + "," + receiverId);
                    //upnp
                    AttemptUPnP(((IPEndPoint)(udpClient.Client.LocalEndPoint)).Port, ((IPEndPoint)(udpClient.Client.LocalEndPoint)).Port);
                    bool udpConnect = false;
                    while (!udpConnect)
                    {
                        try
                        {
                            udpClient.Client.ReceiveTimeout = 3000;
                            byte[] bufSend = udpClient.Receive(ref serverEP);
                            string receiveMessage = Encoding.UTF8.GetString(bufSend, 0, bufSend.Length);
                            clientPClogger.Info(receiveMessage);
                            if (receiveMessage.Equals("get"))
                            {
                                udpConnect = true;
                            }
                        }
                        catch (Exception)
                        {
                            SendHoleMsg(serverEP, udpClient, id + "," + receiverId);
                            continue;
                        }
                    }
                    TcpClient tcpClient = new TcpClient();
                    IPAddress remoteIP = IPAddress.Parse(serverIp);//远程主机IP
                    tcpClient.Connect(new IPEndPoint(remoteIP, serverHoleTcpPort)); //配置服务器IP与端口

                    NetworkStream clientStream = tcpClient.GetStream();
                    StreamWriter sw = new StreamWriter(clientStream);
                    StreamReader sr = new StreamReader(clientStream);
                    sw.WriteLine("I am " + id + "," + receiverId);
                    sw.Flush();
                    if (sr.ReadLine().Equals("welcome"))
                    {
                        clientPClogger.Info("welcome");
                        sw.WriteLine("receiverPeer:" + id + "," + receiverId);
                        sw.Flush();
                        string peer = sr.ReadLine();
                        clientPClogger.Info(peer);
                        string Msg = peer.Substring(8);
                        string[] param = Msg.Split(',');
                        string senderIp = param[0].Substring(5);
                        string senderPort = param[1].Substring(5);
                        Thread.Sleep(2000);
                        IPAddress senderIP = IPAddress.Parse(senderIp);
                        IPEndPoint senderEP = null;
                        IPEndPoint trueSenderEP = null;
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        //for (int i = 0; i < 11; i++)
                        //{
                        //    int port = (int.Parse(senderPort)) - 5 + i;
                        //    if(port > 1024 && port < 65535)
                        //    {
                                senderEP = new IPEndPoint(senderIP, int.Parse(senderPort));
                                //向senderEP发送
                                SendHoleMsg(senderEP, udpClient, "IAmReceiver," + offset);
                        //    }
                        //}
                        udpClient.Client.ReceiveTimeout = 500;
                        Mutex mutex = new Mutex();
                        while (flagSend && sendCount < 10)
                        {
                            try
                            {
                                if (getSendMsg && getSuccess)
                                {
                                    //(new Thread(new receive2(filePath, udpSocket, senderIp, senderPort, offset, fileType, fileSize))).Start();
                                    ReceiveFile receiveFile = new ReceiveFile(filePath, udpClient, senderEP, fileType, offset, fileSize, id);
                                    Thread receiveFileThread = new Thread(receiveFile.Run);
                                    receiveFileThread.IsBackground = true;
                                    receiveFileThread.Start();
                                    //canHole = true;
                                    tcpClient.Close();
                                    flagSend = false;
                                    break;
                                }
                                udpClient.Client.ReceiveTimeout = 2000;
                                byte[] receiveBuffer = udpClient.Receive(ref remoteEP);
                                string receiveMessage = Encoding.Default.GetString(receiveBuffer);
                                clientPClogger.Info(receiveMessage);
                                if (receiveMessage.Equals("IAmSender"))
                                {
                                    senderEP = remoteEP;
                                    clientPClogger.Info("sender1:" + senderEP);
                                    (new Thread(() =>
                                    {
                                        mutex.WaitOne();
                                        if (tcpClient.Connected)
                                        {
                                            try
                                            {
                                                sw.WriteLine("receiverSuccess" + id + "," + receiverId);
                                                sw.Flush();
                                                string msg = sr.ReadLine();
                                                clientPClogger.Info("holeMessage:" + msg);
                                                if (msg.Equals("success"))
                                                {
                                                    getSuccess = true;
                                                }
                                                getSendMsg = true;
                                            }
                                            catch (Exception e)
                                            {
                                                //canHole = true;
                                                clientPClogger.Info(e.ToString());
                                            }
                                        }
                                        mutex.ReleaseMutex();
                                    })).Start();
                                }
                                if (receiveMessage.Equals("lostMsg"))
                                {
                                    senderEP = remoteEP;
                                    clientPClogger.Info("sender2:" + senderEP);
                                    SendHoleMsg(senderEP, udpClient, "IAmReceiver," + offset);
                                }
                                //if (getSendMsg && getSuccess)
                                //{
                                //    //(new Thread(new receive2(filePath, udpSocket, senderIp, senderPort, offset, fileType, fileSize))).Start();
                                //    ReceiveFile receiveFile = new ReceiveFile(filePath, udpClient, trueSenderEP, fileType, offset, fileSize, id);
                                //    Thread receiveFileThread = new Thread(receiveFile.Run);
                                //    receiveFileThread.IsBackground = true;
                                //    receiveFileThread.Start();
                                //    //canHole = true;
                                //    tcpClient.Close();
                                //    flagSend = false;
                                //    break;
                                //}
                                //byte[] receiveBuffer = udpClient.Receive(ref remoteEP);
                                //string receiveMessage = Encoding.UTF8.GetString(receiveBuffer);
                                //clientPClogger.Info("receiveMessage"+receiveMessage);
                                //if (receiveMessage.Contains("IAmSender"))
                                //{
                                //    string strId = receiveMessage.Split(',')[1];
                                //    clientPClogger.Info("IAmSender---" + strId + "---" + id);
                                //    if (strId.Equals(id))
                                //    {
                                //        trueSenderEP = remoteEP;
                                //        getSendMsg = true;
                                //        clientPClogger.Info("sender1:" + senderEP);
                                //        (new Thread(() =>
                                //        {
                                //            mutex.WaitOne();
                                //            if (tcpClient.Connected)
                                //            {
                                //                try
                                //                {
                                //                    sw.WriteLine("receiverSuccess" + id + "," + receiverId);
                                //                    sw.Flush();
                                //                    string msg = sr.ReadLine();
                                //                    clientPClogger.Info("holeMessage:" + msg);
                                //                    if (msg.Equals("success"))
                                //                    {
                                //                        getSuccess = true;
                                //                    }
                                //                }
                                //                catch (Exception e)
                                //                {
                                //                //canHole = true;
                                //                    clientPClogger.Info(e.ToString());
                                //                }
                                //            }
                                //            mutex.ReleaseMutex();
                                //        })).Start();
                                //    }
                                //}
                                ////if (receiveMessage.Equals("lostMsg"))
                                ////{
                                ////    string strId = receiveMessage.Split(',')[1];
                                ////    if (strId.Equals(id))
                                ////    {
                                ////        senderEP = remoteEP;
                                ////        clientPClogger.Info("sender2:" + senderEP);
                                ////        SendHoleMsg(senderEP, udpClient, "IAmReceiver," + id + "," + offset);
                                ////    }
                                ////}
                            }
                            catch (Exception e)
                            {
                                if (!getSendMsg)
                                {
                                    SendHoleMsg(senderEP, udpClient, "lostMsg");
                                }
                                clientPClogger.Info("连接超时--------" + sendCount);
                                sendCount++;
                                if (sendCount == 10)
                                {
                                    clientPClogger.Info("holeFail");
                                    tcpClient.Close();
                                    //canHole = true;
                                }
                                //clientPClogger.Info(e.ToString());
                                //udpClient.Client.ReceiveTimeout += 200;
                                //if (!getSendMsg)
                                //{
                                //    //for (int i = 0; i < 11; i++)
                                //    //{
                                //    //    int port = (int.Parse(senderPort)) - 5 + i;
                                //    //    if (port > 1024 && port < 65535)
                                //    //    {
                                //    //      senderEP = new IPEndPoint(senderIP, int.Parse(senderPort));
                                //            SendHoleMsg(senderEP, udpClient, "IAmReceiver," + id + "," + offset);
                                //    //    }
                                //    //}
                                //}
                                //else
                                //{
                                //    SendHoleMsg(trueSenderEP, udpClient, "IAmReceiver," + id + "," + offset);
                                //}
                                //clientPClogger.Info("接收连接超时--------" + sendCount);
                                //sendCount++;
                                //if (sendCount == 10)
                                //{
                                //    string[] path = Regex.Split(filePath, "\\\\");
                                //    string fileName = path[path.Length - 1];
                                //    sw.WriteLine("holeFail," + senderId + "," + receiverId + "," + fileName + "," + id);
                                //    sw.Flush();
                                //    clientPClogger.Info("holeFail");
                                //    tcpClient.Close();
                                //    //canHole = true;
                                //}
                                continue;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    //canHole = true;
                    clientPClogger.Info(e.ToString());
                }
            }
        }
        /// <summary>
        /// 接收文件线程
        /// </summary>
        private class ReceiveFile
        {
            private String filePath;
            private UdpClient udpClient;
            //private String senderIp;
            //private String senderPort;
            private IPEndPoint senderEP;
            private String fileType;
            private long offset;
            private String fileSize;
            private string id;

            public ReceiveFile(string filePath, UdpClient udpClient, IPEndPoint trueSenderEP, string fileType, long offset, string fileSize, string id)
            {
                this.filePath = filePath;
                this.udpClient = udpClient;
                //this.senderIp = senderIp;
                //this.senderPort = senderPort;
                this.senderEP = trueSenderEP;
                this.fileType = fileType;
                this.offset = offset;
                this.fileSize = fileSize;
                this.id = id;
            }

            public void Run()
            {
                Console.WriteLine("开始接收");
                ReceiveState receiveState = new ReceiveState();
                ReceiveLittle receive = new ReceiveLittle(udpClient,senderEP, receiveState);
                FileStream fileStream = new FileStream(filePath + ".temp", FileMode.Create | FileMode.Append, FileAccess.Write);
                byte[] buff = new byte[1024 * 1024];
                while (isKeepAlive)
                {
                    int length = receive.ReceiveAll(buff);
                    if (length == 3)
                    {
                        String str = Encoding.UTF8.GetString(buff, 0, 3);
                        if (str.Equals("end"))
                        {
                            Console.WriteLine("receive end");
                            fileStream.Close();
                            FileInfo fi = new FileInfo(filePath + ".temp");
                            fi.MoveTo(Path.Combine(filePath + "." + fileType));
                            break;
                        }
                    }
                    fileStream.Write(buff, 0, length);
                    Console.WriteLine(length);
                }
                fileStream.Close();
                clientPCStateList.RemoveAll(s => s.Id == id);
                ClearUpUPnP(((IPEndPoint)udpClient.Client.LocalEndPoint).Port);
            }
        }
    }

}
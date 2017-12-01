using AreaParty.command;
using AreaParty.info;
using AreaParty.util.config;
using client;
using http;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using transferinfomation;

namespace AreaParty.function.pcapp
{
    class PCApp
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Process process_cmd,p;
        private static Process[] process_utorrent;
        private static Thread thread_httpserver, receiveBtServer;
        private static string ip = AreaParty.Program.ip;
        public static bool Open()
        {
            bool opht = OpenHttpService();
            bool oppc = OpenPCinfoService();
            bool oput = OpenUtorrent();
            bool opbt = OpenBtReceive();
            if (opht&&oppc&&opbt)
            {
                if (oput)
                {
                    return true;
                }
                else
                {
                    oput = OpenUtorrent();
                }
                
            }
            return false;


        }
        public static void CLoseAll()
        {
            CloseHttpService();
            ClientPCNew2.StopService();
            ClosePCinfoService();
            CloseUtorrent();
            CloseBtReceive();
        }

        private static bool OpenHttpService()
        {
            Console.WriteLine("{0} {1}",ip , ConfigResource.HTTP_PORT);
            if (!TestOnline(ip, ConfigResource.HTTP_PORT))
            {
                try
                {
                    HttpServer httpServer = new HttpServer(ConfigResource.HTTP_PORT);
                    thread_httpserver = new Thread(new ThreadStart(httpServer.listen));
                    thread_httpserver.IsBackground = true;
                    thread_httpserver.Start();
                    log.Info("HttpService线程已经启动");
                    return true;
                }
                catch(Exception e)
                {
                    
                    log.Error("HttpService 启动失败",e);
                    CloseHttpService();
                }

            }
            return false;
        }

        private static void OpenKeyboardService()
        {
            if (!TestOnline("127.0.0.1", ConfigResource.KEYBOARD))
            {
                try
                {
                    String hole = Application.StartupPath + "\\" + ConfigUtil.GetValue("keyboardService");
                    hole = "\"" + hole + "\"";
                    log.Info(hole);
                    Process rdcProcess = new Process();
                    rdcProcess.StartInfo.FileName = "java.exe";
                    rdcProcess.StartInfo.Arguments = "-jar " + hole;
                    //rdcProcess.StartInfo.WorkingDirectory = hole.Substring(0, hole.LastIndexOf("\\"));
                    rdcProcess.StartInfo.UseShellExecute = false;
                    rdcProcess.StartInfo.CreateNoWindow = true;//不显示程序窗口
                    //rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    rdcProcess.Start();
                }
                catch (Exception e)
                {
                    log.Error("KeyboardService 启动失败",e);
                }
                log.Info("KeyBoard程序启动成功");

            }
            else
            {
                log.Info("KeyBoard程序已经启动");
            }
            
        }

        private static bool OpenPCinfoService()
        {
            if ((!TestOnline(ip, ConfigResource.PCINFO_PORT)) && (!TestOnline(ip, 7777)))
            {
                try
                {
                    Thread TransferInformationThread = new Thread(TransferInformationService.StartService);
                    TransferInformationThread.IsBackground = true;
                    TransferInformationThread.Start();
                    log.Info("PCinfoService线程已经启动");
                    return true;
                }
                catch (ThreadAbortException e)
                {
                    log.Info(string.Format("PCinfoService启动失败：{0}", e.Message));
                    ClosePCinfoService();
                }
            }
            
            return false;
        }

        private static bool OpenUtorrent()
        {
            /*获取uTorrent.exe绝对路径并以/HIDE方式打开*/
            //string utPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //Directory.SetCurrentDirectory(Directory.GetParent(utPath).FullName);
            //utPath = Directory.GetCurrentDirectory();
            string utPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string root = utPath.Split(':')[0]+":";
            utPath = utPath + "uTorrent";//uTorrent.exe /HIDE";
            //string str = "C:\\Users\\wyc\\AppData\\Roaming\\uTorrent\\uTorrent.exe /HIDE";
            if (Process.GetProcessesByName("uTorrent").Length == 0)
            {
                process_cmd = new Process();
                process_cmd.StartInfo.FileName = "cmd.exe";
                process_cmd.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                process_cmd.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                process_cmd.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                process_cmd.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                process_cmd.StartInfo.CreateNoWindow = true;//不显示程序窗口
                try
                {
                    process_cmd.Start();//启动程序
                }
                catch (Exception e)
                {
                    Console.WriteLine("启动程序:{0}", e.Message);
                }
                try
                {
                    process_cmd.StandardInput.WriteLine(root);
                    process_cmd.StandardInput.WriteLine("cd " + utPath);
                    process_cmd.StandardInput.WriteLine("uTorrent.exe /HIDE");
                    process_cmd.StandardInput.AutoFlush = true;
                    //string output = process_cmd.StandardOutput.ReadToEnd();
                }catch(Exception e)
                {
                    Console.WriteLine("utorrent error:" + e.Message);
                }
                
                //process_cmd.StandardInput.Close();
                // + "&exit"向cmd窗口发送输入信息
                //process_cmd.StandardInput.AutoFlush = true;
                //output = process_cmd.StandardOutput.ReadToEnd();
                //process_cmd.WaitForExit();//等待程序执行完退出进程
                process_cmd.Close();
                log.Info("uTorrent进程已经启动");
                return true;
            }
            return false;
        }

        private static bool OpenBtReceive()
        {
            /*启动接收bt文件线程*/
            try
            {
                ReceiveBtServer receivebtserver = new ReceiveBtServer();
                Console.WriteLine("启动接收bt文件线程");
                receiveBtServer = new Thread(new ThreadStart(receivebtserver.run));
                receiveBtServer.IsBackground = true;
                receiveBtServer.Start();
                log.Info("BtReceive线程已经启动");
                return true;
            }
            catch(ThreadAbortException e)
            {
                log.Info(string.Format("BtReceive启动失败：{0}", e.Message));
                CloseBtReceive();
            }
            return false;
        }

        
        private static void CloseHttpService()
        {
            //IPAddress ipAddr = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            //String ip = ipAddr.ToString();
            try{
                thread_httpserver.Abort();
                Close("127.0.0.1", ConfigResource.HTTP_PORT);
                log.Info(string.Format("HttpServer线程退出成功，关闭端口{0}", ConfigResource.HTTP_PORT));
            }
            catch (Exception e)
            {
                
                return;
            }
            Console.WriteLine("HttpServer线程退出成功");
        }
        
        private static void ClosePCinfoService()
        {
            
            TransferInformationService.StopService();
            Console.WriteLine("PCinfoService线程退出成功");
            Close("127.0.0.1", ConfigResource.PCINFO_PORT);
            Close("127.0.0.1", 7777);
            log.Info(string.Format("PCinfoService线程退出成功，关闭端口{0}，{1}", ConfigResource.PCINFO_PORT, 7777));
        }

        private static void CloseUtorrent()
        {
            process_utorrent = Process.GetProcessesByName("uTorrent");
            if (process_utorrent.Length == 0)
            {
                return;
            }
            else
            {
                process_utorrent[0].Kill();
                log.Info(string.Format("Utorrent进程退出成功"));
                Console.WriteLine("Utorrent进程退出成功");
            }
        }

        private static void CloseBtReceive()
        {
            receiveBtServer.Abort();
            Close("127.0.0.1", ConfigResource.BTRECEIVE_PORT);
            log.Info(string.Format("BtReceive线程退出成功，关闭端口{0}",ConfigResource.BTRECEIVE_PORT));
            Console.WriteLine("BtReceive线程退出成功");
        }

        private static bool TestOnline(string ip,int port)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect using a timeout (5 seconds)

                IAsyncResult result = socket.BeginConnect(ip, port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                if (!socket.Connected)
                {
                    // NOTE, MUST CLOSE THE SOCKET

                    socket.Close();
                    //throw new ApplicationException("Failed to connect server.");
                    return false;
                }
                else
                {
                    socket.Close();
                    return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }
            
        }
        public static bool Close(string ipadress,int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipadress);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(new IPEndPoint(ip, port));
                byte[] result = new byte[1024];
                Command c = new Command("PC", "close", null);
                log.Info(JsonConvert.SerializeObject(c));
                serverSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c)));
                log.InfoFormat("关闭端口号{0}软件成功",port);
                serverSocket.Close();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
            
        }
    }
}

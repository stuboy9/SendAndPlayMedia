using AreaParty.info;
using AreaParty.pages;
using AreaParty.util.config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using transferinformation;
using transferinformation.informationFormat.fileManagerFormat;
using transferinformation.processesMonitor;

namespace transferinfomation
{
    public class TransferInformationService
    {
        private static ProcessInfo[] processes;
        private static List<ProcessFormat> processesFormat = new List<ProcessFormat>();
        private static double totalDownloadSpeed;
        private static double totalUploadSpeed;
        private static double totalMemory;
        private static double availableMemory;
        private static int totalCpuPercent;

        private static List<ReturnMessageFormat> folderContent;
        private static List<ReturnMessageFormat> exeContent;

        private static List<SharedFilePathFormat> sharedFileList;
        private static bool isRun = true;

        public static void StartService()
        {
            string sharedFilePathFile = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\AreaParty\sharedFilePathInfor.audio";
            transferinformation.Action.readSharedFileAndParse(ref sharedFilePathFile, ref sharedFileList);


            System.Threading.Thread thread = new System.Threading.Thread(MonitoringSys);
            thread.IsBackground = true;
            thread.Start();

            System.Threading.Thread thread1 = new System.Threading.Thread(MonitoringProcesses);
            thread1.IsBackground = true;
            thread1.Start();

            System.Threading.Thread thread2 = new System.Threading.Thread(TransferMonitorData);
            thread2.IsBackground = true;
            thread2.Start();

            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, ConfigResource.PCINFO_PORT);
            TcpListener listener = new TcpListener(localIP);
            listener.Start();
            Console.WriteLine("Action Server is listening...");
            while (isRun)
            {
                TcpClient remoteClient = listener.AcceptTcpClient();
                Console.WriteLine("Action Server is connected...");

                int RecvBytes = 0;
                byte[] RecvBuf = new byte[1024];
                string messageGet = null;
                try
                {
                    RecvBytes = remoteClient.Client.Receive(RecvBuf);
                    if (RecvBytes <= 0)
                    {
                        Console.WriteLine("Action socket 被动关闭");
                        continue;
                    }
                    messageGet = Encoding.UTF8.GetString(RecvBuf, 0, RecvBytes);
                    Console.WriteLine("Action Message: {0}", messageGet);

                    RequestMessageFormat request = JsonHelper.DeserializeJsonToObject<RequestMessageFormat>(messageGet);
                    ReturnMessageFormat returnMessage = new ReturnMessageFormat();
                    switch (request.name)
                    {
                        case Order.get_areaparty_path:
                            returnMessage.data = null;
                            returnMessage.message = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\";
                            returnMessage.status = 200;
                            break;
                        case Order.ownProgressAction_name:
                            if (request.command == Order.ownProgressAction_Close)
                            {
                                StopService();
                            }
                                //transferinformation.Action.CloseProcess(System.Diagnostics.Process.GetCurrentProcess().Id);
                            break;
                        case Order.processAction_name:
                            if (request.command == Order.processAction_command)
                            {
                                returnMessage = transferinformation.Action.CloseProcess(int.Parse(request.param));
                            }
                            break;
                        case Order.computerAction_name:
                            if (request.command == Order.computerAction_command_reboot)
                            {
                                transferinformation.Action.RebootComputer();
                            }
                            else if (request.command == Order.computerAction_command_shutdown)
                            {
                                transferinformation.Action.shutdownComputer();
                            }
                            break;
                        case Order.fileAction_name:
                            switch (request.command)
                            {
                                case Order.fileAction_share_command:
                                    SharedFilePathFormat sharedFile = JsonHelper.DeserializeJsonToObject<SharedFilePathFormat>(request.param);
                                    returnMessage = transferinformation.Action.AddSharedFile(ref sharedFile, ref sharedFileList, ref sharedFilePathFile);
                                    break;
                                case Order.fileAction_open_command:
                                    {
                                        // 此处是打开文件代码
                                    }
                                    break;
                                case Order.fileOrFolderAction_delete_command:
                                    {
                                        // 此处是删除文件的代码
                                        returnMessage = transferinformation.Action.DeleteFile(request.param);
                                    }
                                    break;
                                case Order.fileOrFolderAction_rename_command:
                                    {
                                        // 此处是重命名文件的代码
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.RenameFile(temp[0], temp[1]);
                                    }
                                    break;
                                case Order.fileOrFolderAction_copy_command:
                                    {
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.CopyFile(temp[2], temp[1]);
                                    }
                                    break;
                                case Order.fileOrFolderAction_cut_command:
                                    {
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.CutFile(temp[2] + @"\", temp[1]);
                                    }
                                    break;
                            }
                            break;
                        case Order.folderAction_name:
                            switch (request.command)
                            {
                                case Order.folderAction_addtohttp_command:
                                    ReturnMessageFormat message = new ReturnMessageFormat();
                                    string method = request.param.Substring(0,5);//request.param = "VIDIO/AUDIO/IMAGE" + uri
                                    string path = request.param.Remove(0, 5);//request.param = uri
                                    //FileInfo Info = new FileInfo(path);
                                    try
                                    {
                                        switch (method)
                                        {
                                            case AreaParty.pages.ListName.VIDIO:
                                                MediaConfig.AddMyVideoLibrary(path);
                                                //MediaPage.videoList.Add(new MediaPage.ListBoxMediaItem { Name = Info.Name, ImagePath = "/styles/skin/item/item_video.png" });
                                                //MediaPage.dictVideo.Add(Info.Name, path);
                                                //MediaPage..Add(new ListBoxMediaItem { Name = Info.Name, ImagePath = "/styles/skin/item/item_video.png" });
                                                break;
                                            case AreaParty.pages.ListName.AUDIO:
                                                MediaConfig.AddMyAudioLibrary(path);                                               
                                                break;
                                            case AreaParty.pages.ListName.IMAGE:
                                                MediaConfig.AddMyImageLibrary(path);
                                                break;
                                        }
                                        new AreaParty.function.media.MediaFunction().GetThumbnail(path);
                                        AreaParty.util.JAVAUtil.AddAlltoHttp(path);
                                        message.status = Order.success;
                                        message.message = "";
                                        message.data = null;
                                    }
                                    catch (Exception e)
                                    {
                                        message.status = Order.failure;
                                        message.message = e.Message;
                                        message.data = null;
                                    }
                                    returnMessage = message;
                                    break;
                                case Order.folderAction_open_command:
                                    {
                                        // 此处是打开文件夹代码
                                        if (request.param != Order.folderAction_open_more_param)
                                        {
                                            NodeFormat root = new NodeFormat();
                                            root.path = request.param;
                                            folderContent = transferinformation.Action.OpenFolder(root);
                                        }
                                        returnMessage = folderContent[0];
                                        folderContent.RemoveAt(0);
                                    }
                                    break;
                                case Order.fileOrFolderAction_delete_command:
                                    {
                                        // 此处是删除文件夹的代码
                                        returnMessage = transferinformation.Action.DeleteFolder(request.param);
                                    }
                                    break;
                                case Order.fileOrFolderAction_rename_command:
                                    {
                                        // 此处是重命名文件夹的代码
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.RenameFolder(temp[0], temp[1]);
                                    }
                                    break;
                                case Order.folderAction_add_command:
                                    {
                                        returnMessage = transferinformation.Action.CreateFolder(request.param);
                                    }
                                    break;
                                case Order.fileOrFolderAction_copy_command:
                                    {
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.CopyFolder(temp[1], temp[2]);
                                    }
                                    break;
                                case Order.fileOrFolderAction_cut_command:
                                    {
                                        string[] temp = System.Text.RegularExpressions.Regex.Split(request.param, "-PATH-",
                                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        returnMessage = transferinformation.Action.CutFolder(temp[2], temp[1]);
                                    }
                                    break;
                            }
                            break;
                        case Order.diskAction_name:
                            switch (request.command)
                            {
                                case Order.diskAction_get_command:
                                    returnMessage = transferinformation.Action.GetDiskList();
                                    break;
                            }
                            break;
                        case Order.appAction_name:
                            switch (request.command)
                            {
                                case Order.appAction_get_command:
                                    if (request.param != Order.appAction_get_more_param)
                                    {
                                        exeContent = transferinformation.Action.GetApplicationList();
                                    }
                                    returnMessage = exeContent[0];
                                    exeContent.RemoveAt(0);
                                    break;
                            }
                            break;

                    }

                    byte[] messageToSend = Encoding.UTF8.GetBytes(JsonHelper.SerializeObject(returnMessage));
                    int i = remoteClient.Client.Send(messageToSend);
                    Console.WriteLine("send Message: {0} ", JsonHelper.SerializeObject(returnMessage));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Action socket 被动关闭" + e.Message);
                }
                remoteClient.Close();
            }

            Console.ReadLine();
        }

        public static bool StopService()
        {
            isRun = false;
            return true;
        }

        /// <summary>
        /// 监听客户端获取监控信息的操作
        /// </summary>
        private static void TransferMonitorData()
        {
            System.Threading.Thread.Sleep(5000);
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, 7777);
            TcpListener listener = new TcpListener(localIP);
            
            listener.Start();
            Console.WriteLine("Monitoring Server is listening...");
            while (true)
            {
                TcpClient remoteClient = listener.AcceptTcpClient();
                Console.WriteLine("Monitoring Server is connected...");

                new System.Threading.Thread(delegate ()
                {
                    while (true)
                    {
                        int RecvBytes = 0;
                        byte[] RecvBuf = new byte[1024];
                        string messageGet = null;
                        Console.WriteLine("在这儿！！！");
                        try
                        {
                            RecvBytes = remoteClient.Client.Receive(RecvBuf);
                            if (RecvBytes <= 0)
                            {
                                Console.WriteLine("Monitoring socket 被动关闭");
                                break;
                            }
                            messageGet = Encoding.UTF8.GetString(RecvBuf, 0, RecvBytes);
                            Console.WriteLine("Message: {0}", messageGet);
                            Console.WriteLine("Client Message's length: {0}", RecvBytes);
                            if (messageGet.Contains(Order.monitorData_name))
                            {
                                string message = PackingMonitorData();
                                //Console.WriteLine(message);
                                byte[] messageToSend = Encoding.UTF8.GetBytes(message);
                                int i = remoteClient.Client.Send(messageToSend);
                                Console.WriteLine("send {0} ", i);
                            }

                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Monitoring socket 被动关闭");
                            break;
                        }

                    }
                }).Start();
                //remoteClient.Close();
            }
        }

        /// <summary>
        /// 监控所有进程，定时更新数据
        /// </summary>
        private static void MonitoringProcesses()
        {
            while (true)
            {
                ProcessMonitor monitor = new ProcessMonitor();
                monitor.StartMonitoring();
                processes = monitor.ProcessesList;
                System.Threading.Thread.Sleep(4000);
                processesFormat.Clear();

                foreach (ProcessInfo process in processes)
                {
                    ProcessFormat temp = new ProcessFormat();
                    temp.id = process.ID;
                    temp.name = process.Name;
                    temp.cpu = process.CpuPercent;
                    temp.memory = process.Memory;
                    temp.path = process.FileName;
                    processesFormat.Add(temp);
                }

                monitor.StopMonitoring();

            }

        }

        /// <summary>
        /// 监控系统，定时更新数据(CPU、网络、内存)
        /// </summary>
        private static void MonitoringSys()
        {
            transferinformation.systemMonitor.NetworkAdapter[] netAdapters;
            while (true)
            {
                transferinformation.systemMonitor.SystemMonitor monitor = new transferinformation.systemMonitor.SystemMonitor();
                monitor.StartMonitoring();
                System.Threading.Thread.Sleep(4000);
                netAdapters = monitor.Adapters;

                totalCpuPercent = monitor.CpuLoad;
                totalMemory = monitor.PhysicalMemory;
                availableMemory = monitor.MemoryAvailable;

                double downLoadSpeed = 0, upLoadSpeed = 0;
                foreach (transferinformation.systemMonitor.NetworkAdapter adapter in netAdapters)
                {
                    downLoadSpeed += adapter.DownloadSpeedKbps;
                    upLoadSpeed += adapter.UploadSpeedKbps;
                }
                totalDownloadSpeed = downLoadSpeed;
                totalUploadSpeed = upLoadSpeed;
                monitor.StopMonitoring();
            }
        }

        /// <summary>
        /// 封装监控数据
        /// </summary>
        /// <returns>封装后的字符串</returns>
        private static string PackingMonitorData()
        {
            ReturnMessageFormat returnData = new ReturnMessageFormat();
            try
            {
                MonitorData data = new MonitorData();
                data.cpu = totalCpuPercent;
                data.memory_available = availableMemory;
                data.memory_total = totalMemory;
                data.memory_used = Math.Round(totalMemory - availableMemory, 1);
                data.net_up = totalUploadSpeed;
                data.net_down = totalDownloadSpeed;
                data.processes = processesFormat;

                returnData.data = data;
                returnData.message = "";
                returnData.status = Order.success;
            }
            catch (Exception)
            {
                returnData.data = null;
                returnData.message = "cannot get data";
                returnData.status = Order.failure;
            }

            return JsonHelper.SerializeObject(returnData);
        }
    }
}

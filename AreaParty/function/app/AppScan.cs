using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using AreaParty.info.applacation;
using System.Net;
using System.Net.Sockets;
using AreaParty.command;
using ProtoBuf;
using protocol;
using AreaParty.util.config;
using AreaParty.info;
using log4net;
using System.Reflection;

namespace AreaParty.function.app
{
    class AppScan
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static List<Item> GetTestList()
        {
            List<Item> list = new List<Item>();
            Item i = new Item();
            i.name = "Potplayer";
            i.EXEname = "PotPlayerMini.exe";
            list.Add(i);
            i = new Item();
            i.name = "网易云音乐";
            i.EXEname = "cloudmusic.exe";
            list.Add(i);
            i = new Item();
            i.name = "Note";
            i.EXEname = "notepad++.exe";
            list.Add(i);
            i = new Item();
            i.name = "EXCEL";
            i.EXEname = "EXCEL.EXE";
            list.Add(i);
            i = new Item();
            i.name = "WINWORD";
            i.EXEname = "WINWORD.EXE";
            list.Add(i);
            i = new Item();
            i.name = "POWERPNT";
            i.EXEname = "POWERPNT.EXE";
            list.Add(i);
            i = new Item();
            i.name = "有道词典";
            i.EXEname = "YoudaoDict.exe";
            list.Add(i);
            return list;
        }

        /// <summary>
        /// 向服务器请求软件和游戏白名单
        /// </summary>
        /// <param name="ipadress">服务器IP</param>
        /// <param name="port">服务器端口</param>
        /// <returns>软件和游戏白名单</returns>
        public static GetExeInfoRsp GetExe(string ipadress,int port)
        {
            GetExeInfoReq builder = new GetExeInfoReq();
            builder.requestType = GetExeInfoReq.RequestType.ALL;
            try
            {
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize<GetExeInfoReq>(ms, builder);
                byte[] data = ms.ToArray();
                IPAddress ip = IPAddress.Parse(ipadress);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(new IPEndPoint(ip, port));
                log.InfoFormat("连接服务器{0}成功", ip);
                byte[] result = new byte[4096];

                byte[] byteArray = PackMessage((int)ENetworkMessage.GET_EXE_INFO_REQ, data);
                serverSocket.Send(byteArray);
                log.InfoFormat("向服务器{0}发送请求软件白名单消息成功", ip);
                int count = 5;
                while (count-->0)
                {
                    int receiveNumber = serverSocket.Receive(result);
                    int size = BytesToInt(result, 0);

                    ENetworkMessage type = (ENetworkMessage)BytesToInt(result, 4);

                    if (type == ENetworkMessage.GET_EXE_INFO_RSP)
                    {
                        byte[] objBytes = new byte[size - GetMessageObjectStartIndex()];
                        for (int i = 0; i < objBytes.Length; i++)
                            objBytes[i] = result[GetMessageObjectStartIndex() + i];

                        MemoryStream ms1 = new MemoryStream(objBytes);
                        GetExeInfoRsp response = Serializer.Deserialize<GetExeInfoRsp>(ms1);
                        log.Info("服务器返回状态码Response : " + response.resultCode);
                        try
                        {
                            serverSocket.Close();
                        }
                        catch (Exception e) {

                        }
                        
                        return response;
                    }
                }
                return null;
            }
            catch (IOException e)
            {
                log.Info("向服务器请求软件白名单异常");
                return null;
            }
        }
        public static byte[] IntToByteArray(int a)
        {
            return new byte[]{
                (byte)((a>>24)&0xff),
                (byte)((a>>16)&0xff),
                (byte)((a>>8)&0xff),
                (byte)(a&0xff)
        };
        }
        public static int BytesToInt(byte[] bytes, int offset)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                int shift = (4 - 1 - i) * 8;
                value += (bytes[i + offset] & 0x000000FF) << shift;
            }
            return value;
        }
        public static byte[] FloatToBytes(float number)
        {
            return BitConverter.GetBytes(number);
        }

        public static byte[] PackMessage(int messageType, byte[] packetBytes)
        {
            return PackMessage(messageType, FloatToBytes((float)new Random().NextDouble()), packetBytes);
        }

        public static byte[] PackMessage(int messageType, byte[] messageIdBytes, byte[] packetBytes)
        {

            int size = GetMessageObjectStartIndex() + packetBytes.Length;
            byte[] messageBytes = new byte[size];

            // 1.添加size
            byte[] sizeBytes = IntToByteArray(size);
            Console.WriteLine(sizeBytes.Length + " " + BytesToInt(sizeBytes, 0));
            for (int i = 0; i < sizeBytes.Length; i++)
                messageBytes[i] = sizeBytes[i];

            // 2.加入类型
            byte[] typeBytes = IntToByteArray(messageType);
            for (int i = 0; i < typeBytes.Length; i++)
                messageBytes[GetTypeStartIndex() + i] = typeBytes[i];

            // 3.添加MessageId
            for (int i = 0; i < messageIdBytes.Length; i++)
                messageBytes[GetMessageIdStartIndex() + i] = messageIdBytes[i];

            // 4.加入数据包
            for (int i = 0; i < packetBytes.Length; i++)
                messageBytes[GetMessageObjectStartIndex() + i] = packetBytes[i];

            return messageBytes;
        }

        public static int GetSizeStartIndex()
        {
            return 0;
        }

        public static int GetTypeStartIndex()
        {
            return GetSizeStartIndex() + 4;
        }

        public static int GetMessageIdStartIndex()
        {
            return GetTypeStartIndex() + 4;
        }

        public static int GetMessageObjectStartIndex()
        {
            return GetMessageIdStartIndex() + 4;
        }
        /// <summary>
        /// 获取服务器软件白名单，浏览本机安装软件，更新软件列表
        /// </summary>
        public static void ScanAndUpdateMySoftware()
        {
            string ip = ConfigResource.SERVER_IP;
            int port = ConfigResource.SERVER_PORT;
            List<Item> applist = GetOnlineAppList(ip, port);//获取白名单软件
            List<Item> gamelist = GetOnlineGameList(ip, port);//获取白名单游戏
            if (applist == null || gamelist == null) return;
            List<ApplacationItem> myapp = GetSoftwareInList(applist);//获取已安装软件，而且在白名单里面
            List<ApplacationItem> mygame = GetSoftwareInList(gamelist);//获取已安装游戏，而且在白名单里面
            UpdateMyAPP(myapp);
            UpdateMyGame(mygame);

        }
        /// <summary>
        /// 更新软件列表
        /// </summary>
        /// <param name="list">需要更新的软件列表</param>
        public static void UpdateMyAPP(List<ApplacationItem> list)
        {
            foreach(ApplacationItem a in list)
            {
                ApplicationConfig.AddApp(a.appName, a.packageName);
                util.IconUtil.GetIconFromFile(a.packageName, 2,  MyInfo.iconFolder + "\\" + a.appName + ".png");
            }
        }
        /// <summary>
        /// 更新软件列表
        /// </summary>
        /// <param name="list">需要更新的游戏列表</param>
        public static void UpdateMyGame(List<ApplacationItem> list)
        {
            foreach (ApplacationItem a in list)
            {
                GameConfig.AddGame(a.appName, a.packageName);
                util.IconUtil.GetIconFromFile(a.packageName, 2, MyInfo.iconFolder + "\\" + a.appName + ".png");
            }
        }


        public static List<Item> GetOnlineAppList(string ip,int port)
        {
            try
            {
                GetExeInfoRsp ger = GetExe(ip, port);
                List<Item> list = new List<Item>();
                foreach(ApplicationItem a in ger.applicationItem)
                {
                    Item i = new Item();
                    i.name = a.name;
                    i.EXEname = a.applicationName;
                    list.Add(i);
                }
                return list;
            }
            catch(Exception e)
            {
                return null;
            }
            
        }
        public static List<Item> GetOnlineGameList(string ip, int port)
        {
            try
            {
                GetExeInfoRsp ger = GetExe(ip, port);
                List<Item> list = new List<Item>();
                foreach (GameItem a in ger.gameItem)
                {
                    Item i = new Item();
                    i.name = a.name;
                    i.EXEname = a.gameName;
                    list.Add(i);
                }
                return list;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public static List<ApplacationItem> GetSoftwareInList(List<Item> list)
        {
            List<ApplacationItem> result = new List<ApplacationItem>();
            List<ApplacationItem> tep = null;
            tep = GetSoftware();
            foreach (ApplacationItem item in tep)
            {
                try
                {
                    //Console.WriteLine(item.packageName);
                    foreach (Item i in list)
                    {
                        //item.packageName.EndsWith(i.EXEname,StringComparison.CurrentCultureIgnoreCase)
                        if (i.EXEname.Equals(item.packageName.Substring(item.packageName.LastIndexOf("\\") + 1), StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Console.WriteLine(item.packageName);
                            if (!result.Exists(x => x.packageName.Equals(item.packageName, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                item.appName = i.name;
                                result.Add(item);
                            }

                            break;
                        }
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

            }
            tep = GetSoftware2();
            foreach (ApplacationItem item in tep)
            {
                try
                {
                    //Console.WriteLine(item.packageName.Substring(item.packageName.LastIndexOf("\\")));
                    foreach (Item i in list)
                    {
                        //item.packageName.EndsWith(i.EXEname,StringComparison.CurrentCultureIgnoreCase)
                        if (i.EXEname.Equals(item.packageName.Substring(item.packageName.LastIndexOf("\\") + 1), StringComparison.CurrentCultureIgnoreCase) && !result.Exists(x => x.packageName.Equals(item.packageName.Substring(item.packageName.LastIndexOf("\\") + 1), StringComparison.CurrentCultureIgnoreCase)))
                        {
                            if (!result.Exists(x => x.packageName.Equals(item.packageName, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                item.appName = i.name;
                                result.Add(item);
                            }
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine("exception");
                    continue;
                }

            }
            tep = GetDesktopSoftware();
            foreach (ApplacationItem item in tep)
            {
                try
                {
                    // Console.WriteLine(item.packageName.Substring(item.packageName.LastIndexOf("\\")));
                    foreach (Item i in list)
                    {
                        //item.packageName.EndsWith(i.EXEname,StringComparison.CurrentCultureIgnoreCase)
                        if (i.EXEname.Equals(item.packageName.Substring(item.packageName.LastIndexOf("\\") + 1), StringComparison.CurrentCultureIgnoreCase) && !result.Exists(x => x.packageName.Equals(item.packageName.Substring(item.packageName.LastIndexOf("\\") + 1), StringComparison.CurrentCultureIgnoreCase)))
                        {
                            if (!result.Exists(x => x.packageName.Equals(item.packageName, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                item.appName = i.name;
                                result.Add(item);
                            }
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine("exception");
                    continue;
                }

            }
            return result;
        }

        /// <summary>
        /// 使用注册表SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall来获取安装的应用
        /// </summary>
        /// <returns></returns>
        public static List<ApplacationItem> GetSoftware()
        {
            List<ApplacationItem> temp = new List<ApplacationItem>();
            RegistryKey key;

            // search in: CurrentUser
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            List<ApplacationItem> l = DisplayInstalledApps(key);
            temp.AddRange(l);
            // search in: LocalMachine_32
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            l = DisplayInstalledApps(key);
            temp.AddRange(l);
            // search in: CurrentUser_64
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            l = DisplayInstalledApps(key);
            temp.AddRange(l);
            // search in: LocalMachine_64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            l = DisplayInstalledApps(key);
            temp.AddRange(l);
            return temp;
        }
        public static List<ApplacationItem> DisplayInstalledApps(RegistryKey key)
        {
            List<ApplacationItem> temp = new List<ApplacationItem>();
            string displayName;
            string path;
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    path = subkey.GetValue("DisplayIcon") as string;
                    if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(path))
                    {
                        if (path.StartsWith("\""))
                            temp.Add(new ApplacationItem(displayName, path.Substring(1, path.Length - 2)));
                        else if (path.EndsWith(",0"))
                            temp.Add(new ApplacationItem(displayName, path.Substring(0, path.Length - 3)));
                        else
                            temp.Add(new ApplacationItem(displayName, path));

                        // Console.WriteLine(displayName);
                    }
                }
            }
            return temp;
        }
        /// <summary>
        /// 使用"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\"注册表来获取安装的应用
        /// </summary>
        /// <returns></returns>
        public static List<ApplacationItem> GetSoftware2()
        {
            string strPathResult = string.Empty;
            string strKeyName = "";     //"(Default)" key, which contains the intalled path 
            object objResult = null;
            List<ApplacationItem> result = new List<ApplacationItem>();

            Microsoft.Win32.RegistryValueKind regValueKind;
            Microsoft.Win32.RegistryKey regKey = null;
            Microsoft.Win32.RegistryKey regSubKey = null;

            try
            {
                //Read the key 
                regKey = Microsoft.Win32.Registry.LocalMachine;
                regSubKey = regKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\", false);
                foreach (String keyName in regSubKey.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey subkey = regSubKey.OpenSubKey(keyName);
                        objResult = subkey.GetValue(strKeyName);
                        regValueKind = subkey.GetValueKind(strKeyName);
                        if (regValueKind == Microsoft.Win32.RegistryValueKind.String)
                        {
                            string path = objResult.ToString();
                            strPathResult = objResult.ToString();
                            if (path.StartsWith("\""))
                                result.Add(new ApplacationItem(keyName, path.Substring(1, path.Length - 2)));
                            else if (path.EndsWith(",0"))
                                result.Add(new ApplacationItem(keyName, path.Substring(0, path.Length - 3)));
                            else
                                result.Add(new ApplacationItem(keyName, path));
                            //result.Add(new ApplacationItem(keyName, objResult.ToString()));
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                }
            }
            catch (System.Security.SecurityException ex)
            {
                return result;
                //throw new System.Security.SecurityException("You have no right to read the registry!", ex);
            }
            catch (Exception ex)
            {
                return result;
                //throw new Exception("Reading registry error!", ex);
            }
            finally
            {

                if (regKey != null)
                {
                    regKey.Close();
                    regKey = null;
                }

                if (regSubKey != null)
                {
                    regSubKey.Close();
                    regSubKey = null;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取当前用户的桌面（后面可以扩展所有用户）上的快捷方式，来获取安装的应用程序，为了速度，只遍历桌面上3层目录。
        /// </summary>
        /// <returns></returns>
        public static List<ApplacationItem> GetDesktopSoftware()
        {
            //int times = 5;
            //List<ApplacationItem> results = new List<ApplacationItem>();
            //string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //Stack<string> dirs = new Stack<string>(200);

            //if (!System.IO.Directory.Exists(desktop))
            //{
            //    throw new ArgumentException();
            //}
            //dirs.Push(desktop);
            //while (dirs.Count > 0 && times-- > 0)
            //{
            //    string currDir = dirs.Pop();
            //    string[] subDirs;

            //    try
            //    {
            //        subDirs = System.IO.Directory.GetDirectories(currDir);
            //    }
            //    catch (System.UnauthorizedAccessException e)
            //    {
            //        Console.WriteLine(e.Message);
            //        continue;
            //    }
            //    catch (System.IO.DirectoryNotFoundException e)
            //    {
            //        Console.WriteLine(e.Message);
            //        continue;
            //    }

            //    string[] files = null;
            //    try
            //    {
            //        files = System.IO.Directory.GetFiles(currDir);
            //    }
            //    catch (System.UnauthorizedAccessException e)
            //    {
            //        Console.WriteLine(e.Message);
            //        continue;
            //    }
            //    catch (System.IO.DirectoryNotFoundException e)
            //    {
            //        Console.WriteLine(e.Message);
            //        continue;
            //    }

            //    foreach (string file in files)
            //    {
            //        try
            //        {
            //            System.IO.FileInfo fi = new System.IO.FileInfo(file);
            //            //Console.WriteLine(fi.Name);
            //            if (fi.Extension.Equals(".lnk"))
            //            {
            //                WshShell shell = new WshShell();
            //                IWshShortcut iw = (IWshShortcut)shell.CreateShortcut(file);
            //                results.Add(new ApplacationItem(fi.Name.Substring(0, fi.Name.Length - 4), iw.TargetPath));
            //                //Console.WriteLine(iw.TargetPath);
            //            }

            //        }
            //        catch (System.IO.FileNotFoundException e)
            //        {
            //            //Console.WriteLine(e.Message);
            //            continue;
            //        }
            //        catch (System.ArgumentException e)
            //        {
            //            continue;
            //        }
            //    }

            //    foreach (string str in subDirs)
            //        dirs.Push(str);
            //}
            List<ApplacationItem> results = FindFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),2);
            return results;
        }
        private static List<ApplacationItem> FindFile(string sSourcePath, int depth)
        {
            //在指定目录及子目录下查找文件,在list中列出子目录及文件
            List<ApplacationItem> re = new List<ApplacationItem>();
            if (depth > 0)
            {
                string[] dirs = System.IO.Directory.GetDirectories(sSourcePath);
                foreach (string d in dirs)//查找子目录 
                {

                    re.AddRange(FindFile(d, depth - 1));
                }
            }
            string[] files = null;
            try
            {
                files = System.IO.Directory.GetFiles(sSourcePath);
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            foreach (string file in files)
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    //Console.WriteLine(fi.Name);

                    if (fi.Extension.Equals(".lnk"))
                    {
                        WshShell shell = new WshShell();
                        IWshShortcut iw = (IWshShortcut)shell.CreateShortcut(file);
                        if (!String.IsNullOrEmpty(iw.TargetPath))
                            re.Add(new ApplacationItem(fi.Name.Substring(0, fi.Name.Length - 4), iw.TargetPath));
                        //Console.WriteLine(iw.TargetPath);
                    }

                }
                catch (System.IO.FileNotFoundException e)
                {
                    Console.WriteLine(e);
                    continue;
                }
                catch (System.ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

            return re;
        }
    }
    public class Item
    {
        public string name { set; get; }
        public string EXEname { get; set; }
    }
}

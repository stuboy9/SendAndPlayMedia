using AreaParty.function.app;
using AreaParty.function.dlna;
using AreaParty.function.game;
using AreaParty.function.media;
using AreaParty.function.miracast;
using AreaParty.function.rdp;
using AreaParty.function.window;
using AreaParty.info;
using AreaParty.info.applacation;
using AreaParty.info.media;
using AreaParty.info.miracast;
using AreaParty.info.tv;
using AreaParty.pages;
using AreaParty.util.config;
using AreaParty.windows;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AreaParty.command
{
    class CommandName
    {
        //目前有很多字段没有实际过程并没有被使用，然而并没有被移除，主要起作用的是MEDIALIST APPLIST DEVICELIST NORMAL字段。
        public const string MEDIA = "MEDIA";//媒体头
        public const string MIRACAST = "MIRACAST";//miracast头
        public const string APPLACATION = "APPLACATION";//应用头
        public const string RDP = "RDP";//RDP头
        public const string DLNA = "DLNA";//DLNA头
        public const string TV = "TV";//tv头
        public const string MEDIALIST = "MEDIALIST";//获取媒体文件数据
        public const string APPLIST = "APPLIST";//获取应用文件数据
        public const string DEVICELIST = "DEVICELIST";//获取屏幕数据，指的是tv
        public const string DLNALIST = "DLNALIST";//获取dlna列表
        public const string PORTELIST = "PORTLIST";//废弃
        public const string NORMAL = "NORMAL";//新添加字段，智能播放媒体文件
        public const string PC = "PC";//将原来手机向服务器发消息改为向PC发消息

        //新字段
        public const string APP = "APP";//
        public const string GAME = "GAME";//
        public const string AUDIO = "AUDIO";//
        public const string VIDEO = "VIDEO";//
        public const string IMAGE = "IMAGE";//
        public const string SYS = "SYS";
        public const string SECURITY = "SECURITY";



    }
    class Command
    {

        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string name { set; get; }
        public string command { set; get; }
        public Dictionary<string, string> param { set; get; }
        public Command(string name,string command, Dictionary<string, string> param)
        {
            this.name = name;
            this.command = command;
            this.param = param;
        }

        public bool PlayMedia(string tvname, string path)
        {
            List<string> urls = new List<string>();
            
            bool isPlay = false;
            foreach (info.tv.TVInfo item in MyInfo.tvLibrary.value)
            {
                if (item.name.Equals(tvname))
                {
                    info.media.MediaItem mi = new info.media.MediaItem(path);
                    urls.Add(mi.url);
                    function.tv.TVFunction.sendCommand(item.ip, info.tv.TVCommand.GetInstance("OPEN_HTTP_MEDIA", null, false, "", mi.GetMediaType(),urls,null));
                    isPlay = true;
                    return true;
                }
            }
            if (!isPlay)
            {
                DLNA get_dlna = new DLNA();
                Command c = new Command("DLNALIST", "GET", new Dictionary<string, string>());
                c.name = CommandName.DLNALIST;
                string javaRev = get_dlna.SendToJava(JsonConvert.SerializeObject(c));
                Console.WriteLine("发送dlna后接收到的消息" + javaRev);
                List<string> dlna_list = JsonConvert.DeserializeObject<List<string>>(javaRev);
                if (dlna_list.Contains(tvname))
                {
                    c.name = CommandName.DLNA;
                    c.command = "PLAY";
                    c.param.Add("path", path);
                    c.param.Add("render", tvname);
                    get_dlna.SendToJava(JsonConvert.SerializeObject(this));
                    return true;
                }

            }
            return false;
        }
        public bool PlayMedias(string tvname,string type,string setname)
        {
            MediaFunction media = new MediaFunction();
            Dictionary<string, List<MediaMengMeng>> d = null;
            if (type.Equals("audio"))
            {
                d = media.GetAudioSet();
            }
            else if (type.Equals("image"))
            {
                d = media.GetImageSet();
            }

            if (!d.ContainsKey(setname)) return false;
            else
            {
                List<string> urls = new List<string>();
                List<MediaMengMeng> l = d[setname];
                foreach(MediaMengMeng m in l){
                    info.media.MediaItem mi = new info.media.MediaItem(m.pathName);
                    urls.Add(mi.url);
                }
                foreach (info.tv.TVInfo item in MyInfo.tvLibrary.value)
                {
                    if (item.name.Equals(tvname))
                    {
                        function.tv.TVFunction.sendCommand(item.ip, info.tv.TVCommand.GetInstance("OPEN_HTTP_MEDIA", null, false, "", type, urls,null));
                        return true;
                    }
                }
            }
            return false;
        }
        public bool PlayMedias(string tvname, string type, string setname, string asbgm)
        {
            MediaFunction media = new MediaFunction();
            Dictionary<string, List<MediaMengMeng>> d = null;
            if (type.Equals("audio"))
            {
                d = media.GetAudioSet();
            }
            else if (type.Equals("image"))
            {
                d = media.GetImageSet();
            }

            if (!d.ContainsKey(setname)) return false;
            else
            {
                List<string> urls = new List<string>();
                List<MediaMengMeng> l = d[setname];
                foreach (MediaMengMeng m in l)
                {
                    info.media.MediaItem mi = new info.media.MediaItem(m.pathName);
                    urls.Add(mi.url);
                }
                foreach (info.tv.TVInfo item in MyInfo.tvLibrary.value)
                {
                    if (item.name.Equals(tvname))
                    {
                        function.tv.TVFunction.sendCommand(item.ip, info.tv.TVCommand.GetInstance("CONTROL_MEDIA", "IMAGE_BACKGROUND_MUSIC", false, "", type, urls, null));
                        return true;
                    }
                }
            }
            return false;
        }
        //public class JoPaths
        //{
        //    public string getpath { get; set; }
        //}
        
        public async void handle(Object myClientSocket)
        {
            Socket ClientSocket = null;
            try
            {
                ClientSocket = (Socket)myClientSocket;
                Projection projection = new Projection();
                WindowControl wc = new WindowControl();
                Applacation app = new Applacation();
                MediaFunction media = new MediaFunction();
                Game game = new Game();
                Response response = null;
                Console.WriteLine("name is:{0}",name);
                switch (name)
                {
                    case CommandName.PC:
                        if (command.Equals("AddDirsHTTP"))
                        {
                            string status = null;
                            bool flag = true;
                            Console.WriteLine("paths is:{0}", param["paths"]);
                            String[] pathlist = param["paths"].Split(new char[]{'?'}, StringSplitOptions.RemoveEmptyEntries);

                            //List<JoPaths> pathlist = JsonConvert.DeserializeObject<List<JoPaths>>(param["paths"]);
                            //List<JoPaths> err_pathlist = new List<JoPaths>();
                            //String[] err_pathlist = null;
                            List<string> err_pathlist = new List<string>();
                            foreach (string path in pathlist)
                            {
                                Console.WriteLine("path is:{0}" + path);
                                string dirPath = path;
                                if (File.Exists(dirPath) || Directory.Exists(dirPath))
                                {
                                    if (Directory.Exists(dirPath))
                                    {
                                        DirectoryInfo Folder = new DirectoryInfo(dirPath);
                                        foreach (FileInfo file in Folder.GetFiles())
                                        {
                                            if (File.Exists(file.FullName))
                                            {
                                                Console.WriteLine("file fullname_1 is:{0}", file.FullName);
                                                Node node = new Node(file.FullName);
                                                NodeContainer.addNode(node);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        FileInfo file = new FileInfo(dirPath);
                                        Console.WriteLine("file fullname_2 is:{0}", file.FullName);
                                        Node node = new Node(file.FullName);
                                        NodeContainer.addNode(node);
                                    }
                                    Console.WriteLine("status:200");
                                }
                                else
                                {
                                    flag = false;
                                    err_pathlist.Add(path);
                                    Console.WriteLine("status:404");
                                }
                            }
                            if (flag)
                            {
                                status = "200";
                            }
                            else
                            {
                                status = "404";
                            }
                            //string errpaths = "{\"errorPaths:\"" + "\"" + err_pathlist +"\"}";
                            response = new Response(status, "", CommandName.PC, err_pathlist);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("close"))
                        {
                            Environment.Exit(0);
                        }
                        break;

                    case CommandName.VIDEO:
                        if (command.Equals("PLAY"))
                        {
                            PlayMedia(param["tvname"], param["path"]);
                            media.AddRecentMeiaList(new MediaItem(param["path"]));
                            response = new Response("200", "", CommandName.VIDEO, null);//////////////////////////////////////
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETTOTALLIST"))
                        {

                            response = new Response("200", "", CommandName.VIDEO, JsonConvert.SerializeObject(media.getMediasByPath("VIDEO", param["folder"])));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETRECENTLIST"))
                        {
                            response = new Response("200", "", CommandName.VIDEO, JsonConvert.SerializeObject(media.GetRecentVideoList()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("DELETE"))
                        {

                            if(media.DeletMyMediaLibrary("UserVideoConfig", param["folder"]))
                            {
                                response = new Response("200", "DELETE_SUCCESS", CommandName.VIDEO, null);
                            }
                            else
                            {
                                response = new Response("404", "DELETE_FAILED", CommandName.VIDEO, null);
                            }
                            
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        
                        break;
                    case CommandName.AUDIO:
                        if (command.Equals("PLAY"))
                        {
                            PlayMedia(param["tvname"], param["path"]);
                            media.AddRecentMeiaList(new MediaItem(param["path"]));
                            response = new Response("200", "", CommandName.AUDIO, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETTOTALLIST"))
                        {
                            response = new Response("200", "", CommandName.AUDIO, JsonConvert.SerializeObject(media.getMediasByPath("AUDIO", param["folder"])));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETRECENTLIST"))
                        {
                            response = new Response("200", "", CommandName.AUDIO, JsonConvert.SerializeObject(media.GetRecentAudioList()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETSETS"))
                        {
                            response = new Response("200", "", CommandName.AUDIO, JsonConvert.SerializeObject(media.GetAudioSet()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("ADDSET"))
                        {
                            if(media.AddAudioSet(param["setname"]))
                                response = new Response("200", "", CommandName.AUDIO,null);
                            else
                                response = new Response("404", "", CommandName.AUDIO, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("ADDFILESTOSET"))
                        {
                            string s = param["liststr"];
                            if (media.AddAudioSet(param["setname"], JsonConvert.DeserializeObject<List<MediaMengMeng>>(s)))
                            {
                                response = new Response("200", "", CommandName.AUDIO, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.AUDIO, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("DELETESET"))
                        {
                            if (media.DeleteAudiSet(param["setname"]))
                            {
                                response = new Response("200", "", CommandName.AUDIO, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.AUDIO, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAYSET"))
                        {
                            bool re;
                            //if (!param.ContainsKey("asBGM"))
                            //{
                            //    re = PlayMedias(param["tvname"], "audio", param["setname"]);
                            //}
                            //else
                            //{
                            re = PlayMedias(param["tvname"], "audio", param["setname"]);
                            //}
                            if (re)
                            {
                                List<MediaMengMeng> list = media.GetAudioSet()[param["setname"]];
                                if (list.Count < 10)
                                    foreach (MediaMengMeng m in list)
                                        media.AddRecentAudioList(m);
                                else
                                    for (int i = list.Count - 10; i < list.Count; i++)
                                        media.AddRecentAudioList(list[i]);
                                response = new Response("200", "", CommandName.AUDIO, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.AUDIO, null);
                            }

                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAYSETASBGM"))
                        {
                            bool re;
                            re = PlayMedias(param["tvname"], "audio", param["setname"],null);
                            if (re)
                            {
                                List<MediaMengMeng> list = media.GetAudioSet()[param["setname"]];
                                if (list.Count < 10)
                                    foreach (MediaMengMeng m in list)
                                        media.AddRecentAudioList(m);
                                else
                                    for (int i = list.Count - 10; i < list.Count; i++)
                                        media.AddRecentAudioList(list[i]);
                                response = new Response("200", "", CommandName.AUDIO, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.AUDIO, null);
                            }

                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("DELETE"))
                        {

                            if (media.DeletMyMediaLibrary("UserAudioConfig", param["folder"]))
                            {
                                response = new Response("200", "DELETE_SUCCESS", CommandName.AUDIO, null);
                            }
                            else
                            {
                                response = new Response("404", "DELETE_FAILED", CommandName.AUDIO, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    case CommandName.IMAGE:
                        if (command.Equals("PLAY"))
                        {
                            PlayMedia(param["tvname"], param["path"]);
                            response = new Response("200", "", CommandName.IMAGE, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAYALL"))
                        {
                            List<MediaMengMeng> l = media.getMediasByPath("IMAGE", param["folder"]);
                            List<string> urls = new List<string>();
                       
                            foreach (MediaMengMeng m in l)
                            {
                                info.media.MediaItem mi = new info.media.MediaItem(m.pathName);
                                urls.Add(mi.url);
                            }
                            foreach (info.tv.TVInfo item in MyInfo.tvLibrary.value)
                            {
                                if (item.name.Equals(param["tvname"]))
                                {
                                    function.tv.TVFunction.sendCommand(item.ip, info.tv.TVCommand.GetInstance("OPEN_HTTP_MEDIA", null, false, "", "image", urls, null));
                                }
                            }
                            //foreach (var ls in list)
                            //{
                            //    PlayMedia(param["tvname"], ls[""]);
                            //}
                            

                            response = new Response("200", "", CommandName.IMAGE, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETTOTALLIST"))
                        {
                            List<MediaMengMeng> list = media.getMediasByPath("IMAGE", param["folder"]);
                            response = new Response("200", "", CommandName.IMAGE, JsonConvert.SerializeObject(list));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETSETS"))
                        {
                            response = new Response("200", "", CommandName.IMAGE, JsonConvert.SerializeObject(media.GetImageSet()));
                            
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("ADDSET"))
                        {
                            if (media.AddImageSet(param["setname"]))
                                response = new Response("200", "", CommandName.IMAGE, null);
                            else
                                response = new Response("404", "", CommandName.IMAGE, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("ADDFILESTOSET"))
                        {
                            string s = param["liststr"];
                            if (media.AddImageSet(param["setname"], JsonConvert.DeserializeObject<List<MediaMengMeng>>(s)))
                            {
                                response = new Response("200", "", CommandName.IMAGE, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.IMAGE, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("DELETESET"))
                        {
                            if (media.DeleteImageSet(param["setname"]))
                            {
                                response = new Response("200", "", CommandName.IMAGE, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.IMAGE, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAYSET"))
                        {
                            bool re = PlayMedias(param["tvname"], "image", param["setname"]);
                            if (re)
                            {
                                List<MediaMengMeng> list = media.GetImageSet()[param["setname"]];
                                if (list.Count < 10)
                                    foreach (MediaMengMeng m in list)
                                        media.AddRecentImageList(m);
                                else
                                    for (int i = list.Count - 10; i < list.Count; i++)
                                        media.AddRecentImageList(list[i]);
                                response = new Response("200", "", CommandName.IMAGE, null);
                            }
                            else
                            {
                                response = new Response("404", "", CommandName.IMAGE, null);
                            }

                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("DELETE"))
                        {

                            if (media.DeletMyMediaLibrary("UserImageConfig", param["folder"]))
                            {
                                response = new Response("200", "DELETE_SUCCESS", CommandName.VIDEO, null);
                            }
                            else
                            {
                                response = new Response("404", "DELETE_FAILED", CommandName.VIDEO, null);
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    case CommandName.APP:
                        if (command.Equals("GETTOTALLIST"))
                        {
                            response = new Response("200", "", CommandName.APP, JsonConvert.SerializeObject(app.GetAppList()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("OPEN_RDP"))
                        {
                            //WindowControl.SetTaskBarState(0);

                            if((string)param["path"] == "desk")
                            {
                                //Type oleType = Type.GetTypeFromProgID("Shell.Application");
                                //object oleObject = System.Activator.CreateInstance(oleType);
                                //Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
                                //void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
                                //{
                                //    log.Info(string.Format("屏幕状态:{0}",e));
                                //    if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock|| e.Reason == SessionSwitchReason.RemoteConnect)
                                //    {
                                //        // 屏幕锁定
                                //        log.Info(string.Format("屏幕锁定状态"));
                                //        try
                                //        {
                                //            App.Current.Dispatcher.Invoke((Action)(() =>
                                //            {
                                //                System.Windows.Window test = windows.BackGroundWindow.Instance();
                                //                test.Show();
                                //            }));
                                //            app.CloseAll();
                                //        }
                                //        catch(Exception ex)
                                //        {
                                //            log.Info(string.Format("关闭当前进程出错：{0}",ex));
                                //        }
                                //        log.Info(string.Format("当前进程关闭"));
                                //    }
                                //    else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.ConsoleConnect)
                                //    {
                                //        // 屏幕解锁  
                                //        oleType.InvokeMember("ToggleDesktop", BindingFlags.InvokeMethod, null, oleObject, null);
                                //    }
                                //}
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    System.Windows.Window test = windows.BackGroundWindow.Instance();
                                    test.Hide();
                                }));
                                app.CloseAll();
                                Type oleType = Type.GetTypeFromProgID("Shell.Application");
                                object oleObject = System.Activator.CreateInstance(oleType);
                                oleType.InvokeMember("ToggleDesktop", BindingFlags.InvokeMethod, null, oleObject, null);
                            }
                            //else if((string)param["path"] == "back")
                            //{
                            //    //从打开的应用退回到桌面
                            //    App.Current.Dispatcher.Invoke((Action)(() =>
                            //    {
                            //        System.Windows.Window test = windows.BackGroundWindow.Instance();
                            //        test.Hide();
                            //    }));
                            //    app.CloseAll();
                            //    Type oleType = Type.GetTypeFromProgID("Shell.Application");
                            //    object oleObject = System.Activator.CreateInstance(oleType);
                            //    oleType.InvokeMember("ToggleDesktop", BindingFlags.InvokeMethod, null, oleObject, null);
                            //}
                            else
                            {
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    System.Windows.Window test = windows.BackGroundWindow.Instance();
                                    test.Show();
                                }));
                                app.OpenApp((string)param["path"], "");
                            }
                            
                        }
                        else if (command.Equals("OPEN_MIRACAST"))
                        {
                            //WindowControl.SetTaskBarState(0);
                            if (!projection.IsConnection())
                            {
                                app.CloseAll();
                                //projection.EnterQS();
                                //projection.ClosePopup();
                            }
                            Process pro;
                            pro = app.OpenApp((string)param["path"], "");
                            //Console.WriteLine(System.Windows.Forms.Screen.AllScreens.Last());
                            DeviceItem item = util.config.UserScreenConfig.GetScreen(param["tvname"]);
                            if(item != null)
                            {
                                await projection.SelectProjectionDevice(item.screen);
                                wc.MoveWindow(pro, System.Windows.Forms.Screen.AllScreens.Last(), true);
                                response = new Response("200", "", CommandName.APP, null);
                            }
                            else
                            {
                                response = new Response("404", "don't have set miracast", CommandName.APP, null);
                            }
                            //projection.ClearMiracastPopup();
                            
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    case CommandName.GAME:
                        if (command.Equals("GETTOTALLIST"))
                        {
                            response = new Response("200", "", CommandName.GAME, JsonConvert.SerializeObject(game.GetGameList()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if(command.Equals("OPEN"))
                        {
                            game.OpenGame(param["path"],"");
                            response = new Response("200", "", CommandName.GAME, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    case CommandName.SYS:
                        if (command.Equals("GETINFOR"))
                        {
                            response = new Response("200", "", CommandName.SYS, JsonConvert.SerializeObject(function.pcapp.PCInformation.GetInstance()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("GETSCREENSTATE"))
                        {
                            if (MyInfo.IsSreenLock) 
                                response =new Response("404","",CommandName.SYS, null);
                            else
                                response = new Response("200", "", CommandName.SYS, null);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    case CommandName.SECURITY:
                        if (command.Equals("PAIR")) {
                            string code = param["code"];
                            if (code.Equals(PairCodeConfig.GetPairCode()))
                            {
                                response = new Response("200", "", CommandName.SECURITY, "true");
                            }
                            else
                            {
                                response = new Response("200", "", CommandName.SECURITY, "false");
                            }
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        break;
                    

                }
                log.InfoFormat("Command 接受到的命令:  {0},返回的结果:  {1}", JsonConvert.SerializeObject(this), JsonConvert.SerializeObject(response));
                ClientSocket.Close();
            }
            catch (Exception e)
            {
                if (ClientSocket != null) ClientSocket.Close();
                log.Error("Comand 执行出错",e);
            }

        }

    }
}

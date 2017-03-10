using Newtonsoft.Json;
using SendAndPlayMedia.info;
using SendAndPlayMedia.info.device;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test.info;
using Test.info.applacation;
using Test.info.miracast;

namespace SendAndPlayMedia.command
{
    class CommandName
    {
        public  const string MEDIA = "MEDIA";
        public  const string MIRACAST = "MIRACAST";
        public  const string APPLACATION = "APPLACATION";
        public  const string RDP = "RDP";
        public  const string DLNA = "DLNA";
        public const string TV = "TV";
        public const string MEDIALIST = "MEDIALIST";
        public const string APPLIST = "APPLIST";
        public const string DEVICELIST = "DEVICELIST";



    }
    class Command
    {
        public string name { set; get; }
        public string command { set; get; }
        public Dictionary<string, string> param { set; get; }
        public Command(string name,string command, Dictionary<string, string> param)
        {
            this.name = name;
            this.command = command;
            this.param = param;
        }
        public void handle(Object myClientSocket)
        {
            try
            {
                Socket ClientSocket = (Socket)myClientSocket;
                Projection projection = new Projection();
                WindowControl wc = new WindowControl();
                Applacation app = new Applacation();
                MediaFunction media = new MediaFunction();
                Response response = null;

                switch (name)
                {
                    case CommandName.MEDIALIST:
                        Console.WriteLine("medias mengmeng:get");
                        MediaLibrary mediaLirary = JsonConvert.DeserializeObject<MediaLibrary>(media.getMedias());
                        Console.WriteLine(JsonConvert.SerializeObject(mediaLirary.value));
                        ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mediaLirary.value)));
                        break;
                    case CommandName.APPLIST:
                        Console.WriteLine("app mengmeng:get");
                        AppLibrary appLirary = JsonConvert.DeserializeObject<AppLibrary>(app.GetAppList());
                        ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(appLirary.value)));
                        break;
                    case CommandName.DEVICELIST:
                        Console.WriteLine("device mengmeng:get");
                        DeviceLibrary deviceLibrary = new DeviceLibrary();
                        if (MyInfo.tvLibrary.value.Count > 0)
                        {
                            Console.WriteLine("有数据");
                            Console.WriteLine(MyInfo.tvLibrary.value.First().name);
                            deviceLibrary.value.Add(new DeviceItem(MyInfo.tvLibrary.value.First(), MyInfo.tvLibrary.value.First().name, "2"));

                        }
                        //Console.WriteLine(deviceLibrary.value.First());
                        ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deviceLibrary.value)));
                        JsonConvert.SerializeObject(JsonConvert.SerializeObject(deviceLibrary.value));
                        break;
                    case CommandName.MEDIA:
                        if (command.Equals("GET"))
                        {
                            Console.WriteLine("medias:get");
                            response = new Response("200", "", "media", JsonConvert.DeserializeObject<MediaLibrary>(media.getMedias()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAY"))
                        {
                            media.openPlayer((string)param["path"]);
                            response = new Response("200", "", "media", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                            //process = program.openPlayer(@"E:\software\PotPlayer\PotPlayerMini.exe", r[1]);
                        }
                        else if (command.Equals("KILL"))
                        {
                            response = new Response("200", "", "media", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                            media.KillPlayer();
                        }
                        Console.WriteLine(JsonConvert.SerializeObject(response));
                        break;
                    case CommandName.MIRACAST:

                        if (command.Equals("GET"))
                        {
                            Console.WriteLine("miracast:get");
                            //Console.WriteLine(program.ToJson("200", "", projection.GetDeviceList()));
                            response = new Response("200", "", "miracast", JsonConvert.DeserializeObject<MiracastLibrary>(projection.GetDeviceList()));
                            Console.WriteLine(JsonConvert.SerializeObject(response));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));

                        }
                        else if (command.Equals("PLAY"))
                        {
                            projection.SelectProjectionDevice(Int32.Parse((string)param["screen"]));
                            Process pro;
                            if (param["name"].Equals("media"))
                            {
                                pro = media.openPlayer((string)param["path"]);
                            }
                            else if (param["name"].Equals("application"))
                            {
                                pro = app.OpenApp((string)param["path"], "");
                            }
                            else
                            {
                                pro = null;
                                projection.CloseProjection();
                                break;
                            }
                            wc.MoveWindow(pro, Projection.current.screen, true);

                            response = new Response("200", "", "miracast", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("CONNECT"))
                        {
                            Console.WriteLine("miracast:play");
                            projection.SelectProjectionDevice(Int32.Parse((string)param["screen"]));

                            response = new Response("200", "", "miracast", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        Console.WriteLine(JsonConvert.SerializeObject(response));
                        break;
                    case CommandName.APPLACATION:

                        if (command.Equals("GET"))
                        {
                            Console.WriteLine("applacation:get");
                            //Console.WriteLine(program.ToJson("200", "", app.GetAppList()));
                            response = new Response("200", "", "applacation", JsonConvert.DeserializeObject<AppLibrary>(app.GetAppList()));
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("START"))
                        {
                            app.OpenApp((string)param["app"], "");
                            response = new Response("200", "", "applacation", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("STOP"))
                        {
                            string path = (string)param["app"];
                            Console.WriteLine(path.Split('\\').Last().Split('.').First());
                            app.CloseApp(path.Split('\\').Last().Split('.').First());
                            response = new Response("200", "", "applacation", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        Console.WriteLine(JsonConvert.SerializeObject(response));
                        break;
                    case CommandName.RDP:
                        RDP rdp = new RDP("192.168.1.149", "123456", "123456");
                        if (command.Equals("OPEN"))
                        {
                            rdp = new RDP((string)param["ip"], (string)param["username"], (string)param["password"]);
                            rdp.OpenRDP();
                            response = new Response("200", "", "applacation", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("CLOSE"))
                        {
                            rdp.CloseRDP();
                            response = new Response("200", "", "applacation", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        else if (command.Equals("PLAY"))
                        {
                            if (param["name"].Equals("media"))
                            {
                                media.openPlayer((string)param["path"]);
                            }
                            else if (param["name"].Equals("application"))
                            {
                                app.OpenApp((string)param["path"], "");
                            }
                            else
                            {
                                break;
                            }
                            response = new Response("200", "", "RDP", new Info());
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }
                        Console.WriteLine(JsonConvert.SerializeObject(response));
                        break;
                    case CommandName.DLNA:
                        DLNA dlna = new DLNA();
                        ClientSocket.Send(Encoding.UTF8.GetBytes(dlna.SendToJava(JsonConvert.SerializeObject(this))));
                        break;
                    case CommandName.TV:
                        if (command.Equals("GET"))
                        {
                            response = new Response("200", "", "tv", MyInfo.tvLibrary);
                            ClientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        }

                        break;

                }
                ClientSocket.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

    }
}

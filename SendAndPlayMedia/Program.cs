using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SendAndPlayMedia.command;
using SendAndPlayMedia.function.tv;
using SendAndPlayMedia.function;

namespace SendAndPlayMedia
{
    class Program
    {
        static void Main(string[] args)
        {
            Applacation app = new Applacation();
            //Process p = app.OpenApp(@"E:\software\PotPlayer\PotPlayerMini.exe","");
            //Console.WriteLine(p.ProcessName);
            //app.CloseApp("WINWORD");
            //Thread.Sleep(100000);
            Program program = new Program();
            TVFunction tv = new TVFunction();
            tv.Start();
            Projection projection = new Projection();
            RDP rdp = new RDP("192.168.1.149", "123456", "123456");
            Process pro = new Process();
            pro.StartInfo.FileName = @"F:\VS2015WorkSpace\SendAndPlayMedia\StartProjection\bin\Debug\StartProjection.exe";
            pro.Start();

            MediaFunction media = new MediaFunction();
            media.init();
            Console.WriteLine("finish");
            
            DLNA dlna = new DLNA();

            Phone phone = new Phone();
            phone.start();
            //Console.WriteLine(dlna.SendToJava("{\"name\":\"DLNA\",\"command\":\"PLAY\",\"param\":{\"path\":\"G:\\\\DLNA\\\\s01e3.mp4\",\"render\":\"DroidDLNA Local Render (N1)\"}}"));
            
            //Thread.Sleep(100000);

            byte[] result = new byte[1024];
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8888));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求  
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            while (true)
            {
               Console.WriteLine("等待连接");
                Socket myClientSocket = serverSocket.Accept();
                Console.WriteLine("{0}使用端口{1}连接成功",((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Address, ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Port);
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    Console.WriteLine(rev+"yiyiyi");

                    //验证数据是tv还是手机传来，如果是tv，则转发给原来处理程序处理。
                    JsonTextReader reader = new JsonTextReader(new StringReader(rev));
                    JObject obj = JObject.Parse(rev);
                    if (obj.Property("uuid") != null)
                    {
                        Socket tvTest = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        
                        tvTest.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9752));
                        tvTest.Send(Encoding.UTF8.GetBytes(rev));
                        tvTest.Close();
                        myClientSocket.Close();
                        continue;
                    }
                    else if (obj.Property("source") != null)
                    {
                        phone.FuncMSG(rev);
                        myClientSocket.Close();
                        continue;
                    }

                    Command com = JsonConvert.DeserializeObject<Command>(rev);
                    Thread t = new Thread(new ParameterizedThreadStart(com.handle));
                    t.Start(myClientSocket);
                    //JsonTextReader reader = new JsonTextReader(new StringReader(rev));
                    //JObject obj = JObject.Parse(rev);
                    //string name =(string)obj["name"];
                    //string command = (string)obj["command"];
                    //Console.WriteLine(name);
                    //Console.WriteLine(command);
                    //JObject param = (JObject)obj["param"];
                    //switch (name)
                    //{
                    //    case "MEDIA":
                    //        if (command.Equals("GET"))
                    //        {
                    //            Console.WriteLine("medias:get");
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200","", media.getMedias())));
                    //        }
                    //        else if (command.Equals("PLAY"))
                    //        {
                    //            media.openPlayer((string)param["path"]);
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "",new JObject().ToString() )));
                    //            //process = program.openPlayer(@"E:\software\PotPlayer\PotPlayerMini.exe", r[1]);
                    //        }
                    //        else if (command.Equals("KILL"))
                    //        {
                    //            media.KillPlayer();
                    //        }
                    //        break;
                    //    case "MIRACAST":
                    //        if (command.Equals("GET"))
                    //        {
                    //            Console.WriteLine("miracast:get");
                    //            //Console.WriteLine(program.ToJson("200", "", projection.GetDeviceList()));

                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", projection.GetDeviceList())));

                    //        }
                    //        else if(command.Equals("PLAY")){
                    //            Console.WriteLine("miracast:play");
                    //            projection.SetDisplayMode(DisplayMode.Duplicate);
                    //            projection.SelectProjectionDevice(Int32.Parse((string)param["screen"]));
                    //            Thread.Sleep(2000);
                    //            projection.ClearMiracastPopup();
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes("SUCCESS"));
                    //            Thread.Sleep(10000);
                    //            projection.SetDisplayMode(DisplayMode.Duplicate);
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", new JObject().ToString())));
                    //        }
                    //        break;
                    //    case "APPLACATION":
                    //        if (command.Equals("GET"))
                    //        {
                    //            Console.WriteLine("applacation:get");
                    //            //Console.WriteLine(program.ToJson("200", "", app.GetAppList()));
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200","", app.GetAppList())));
                    //        }
                    //        else if (command.Equals("START"))
                    //        {
                    //            app.OpenApp((string)param["app"], "");
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", new JObject().ToString())));
                    //        }
                    //        else if (command.Equals("STOP"))
                    //        {
                    //            string path = (string)param["app"];
                    //            Console.WriteLine(path.Split('\\').Last().Split('.').First());
                    //            app.CloseApp(path.Split('\\').Last().Split('.').First());
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", new JObject().ToString())));
                    //        }
                    //        break;
                    //    case "RDP":
                    //        if (command.Equals("OPEN"))
                    //        {
                    //            rdp = new RDP((string)param["ip"], (string)param["username"], (string)param["password"]);
                    //            rdp.OpenRDP();
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", new JObject().ToString())));
                    //        }
                    //        else if (command.Equals("CLOSE"))
                    //        {
                    //            rdp.CloseRDP();
                    //            myClientSocket.Send(Encoding.UTF8.GetBytes(program.ToJson("200", "", new JObject().ToString())));
                    //        }
                    //        break;
                    //    case "DLNA":
                    //        myClientSocket.Send(Encoding.UTF8.GetBytes(dlna.SendToJava(rev)));
                    //        break;
                    //}
                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                }

            }
        }
        
        public string ToJson(string status, string message, string data)
        {
            JObject obj = new JObject();
            obj.Add("status", status);
            obj.Add("message", message);
            obj.Add("data", JObject.Parse(data));
            return obj.ToString();
        }


    }
}

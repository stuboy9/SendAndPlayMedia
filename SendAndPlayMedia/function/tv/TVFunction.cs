using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendAndPlayMedia.command;
using SendAndPlayMedia.info;
using SendAndPlayMedia.info.tv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendAndPlayMedia.function.tv
{
    class TVFunction
    {
        private int timeout = 15;
        public void Start()
        {
            new Thread(Connect).Start();
            new Thread(CheckTimeout).Start();
        }
        public void Connect()
        {
            byte[] result = new byte[1024];
           
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 9752));  //绑定IP地址：端口  
            serverSocket.Listen(40);    //设定最多10个排队连接请求  
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            while (true)
            {
                // Console.WriteLine("等待连接");
                Socket myClientSocket = serverSocket.Accept();
                //Console.WriteLine("保持连接程序连接成功");
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    //Console.WriteLine(rev);

                    JsonTextReader reader = new JsonTextReader(new StringReader(rev));
                    JObject obj = JObject.Parse(rev);
                    if (obj.Property("command") != null)
                    {
                        Command com = JsonConvert.DeserializeObject<Command>(rev);
                        Thread t = new Thread(new ParameterizedThreadStart(com.handle));
                        t.Start(myClientSocket);
                        continue;
                    }
                    TVInfo tv =  JsonConvert.DeserializeObject<TVInfo>(rev);
                    lock (MyInfo.locker)
                    {
                        DLNA get_dlna = new DLNA();
                        command.Command c = new command.Command("DLNALIST","GET",new Dictionary<string, string>());
                        c.name = command.CommandName.DLNALIST;
                        List<string> device_list = JsonConvert.DeserializeObject<List<string>>(get_dlna.SendToJava(JsonConvert.SerializeObject(c)));
                        if (!MyInfo.tvLibrary.value.Contains(tv))
                        {
                            if (!device_list.Contains(tv.name))
                            {
                                tv.dlnaOk = false;
                            }
                            else
                            {
                                tv.dlnaOk = true;
                            }
                            Console.WriteLine("当前tv{0}是否支持dlna：{1}", tv.name, tv.dlnaOk);
                            tv.timeStamp = ConvertDateTimeInt(DateTime.Now);
                            MyInfo.tvLibrary.value.Add(tv);
                        }
                        else
                        {
                            TVInfo t = MyInfo.tvLibrary.value.Find(delegate (TVInfo user){return user.Equals(tv);});
                            t.timeStamp = ConvertDateTimeInt(DateTime.Now);  
                        }

                    }
                    myClientSocket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                }
            }
        }
        public void CheckTimeout()
        {
            while (true)
            {
                int time = ConvertDateTimeInt( DateTime.Now);
                int len = MyInfo.tvLibrary.value.Count();
                for (int i = len - 1; i >= 0; i--)
                {
                    
                    if (time - MyInfo.tvLibrary.value[i].timeStamp >= timeout)
                    {
                        Console.WriteLine(time - MyInfo.tvLibrary.value[i].timeStamp + "   " + ConvertStringToDateTime(Convert.ToString(time)) + "    " + ConvertStringToDateTime(Convert.ToString(MyInfo.tvLibrary.value[i].timeStamp)));
                        lock (MyInfo.locker)
                        {
                            MyInfo.tvLibrary.value.RemoveAt(i);
                        }
                    }

                }
                    
                Thread.Sleep(timeout * 1000);
            }
            
        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        /// <summary>        
        /// 时间戳转为C#格式时间        
        /// </summary>        
        /// <param name=”timeStamp”></param>        
        /// <returns></returns>        
        private static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

    }
     
}

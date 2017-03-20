using Newtonsoft.Json;
using SendAndPlayMedia.info;
using SendAndPlayMedia.info.tv;
using System;
using System.Collections.Generic;
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
        
        public void Start()
        {
            new Thread(Connect).Start();
        }
        public void Connect()
        {
            byte[] result = new byte[1024];
           
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 9752));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求  
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            while (true)
            {
                // Console.WriteLine("等待连接");
                Socket myClientSocket = serverSocket.Accept();
                Console.WriteLine("连接成功");
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);

                    Console.WriteLine(rev);
                    TVInfo tv =  JsonConvert.DeserializeObject<TVInfo>(rev);
                    lock (MyInfo.locker)
                    {
                        DLNA get_dlna = new DLNA();
                        command.Command c = new command.Command("DLNALIST","GET",new Dictionary<string, string>());
                        c.name = command.CommandName.DLNALIST;
                        List<string> device_list = JsonConvert.DeserializeObject<List<string>>(get_dlna.SendToJava(JsonConvert.SerializeObject(c)));
                        
                        if (!MyInfo.tvLibrary.value.Exists(x => x.name == tv.name))
                            {
                                if (!device_list.Exists(y => y == tv.name))
                                {
                                    tv.dlnaOk = false;
                                }
                            tv.dlnaOk = false;
                            Console.WriteLine("当前tv{0}是否支持dlna：{1}",tv.name,tv.dlnaOk);
                            MyInfo.tvLibrary.value.Add(tv);
                        }
                        
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                }
            }
        }
    }
}

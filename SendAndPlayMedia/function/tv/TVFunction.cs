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
            IPAddress ip = IPAddress.Parse("192.168.1.126");
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
                        MyInfo.tvLibrary.value.Add(tv);
                    }
                    Console.WriteLine(tv.name +" "+tv.uuid);
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

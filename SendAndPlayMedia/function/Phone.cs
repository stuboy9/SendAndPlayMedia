using Newtonsoft.Json;
using SendAndPlayMedia.info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendAndPlayMedia.function
{
    class Phone
    {
        int port = 9321;
        Boolean flag = false;
        public Phone(int port=9321)
        {
            this.port = port;
        }
        public void start()
        {
            if (flag == false)
            {
                flag = true;
                new Thread(run).Start();
            }
            
            

        }
        /// <summary>
        /// 循环运行，监听广播消息，接收到了就保存起来，同时返回自己的消息
        /// </summary>
        private void run()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, port);
            sock.Bind(iep);
            EndPoint ep = (EndPoint)iep;
            Console.WriteLine("Ready to receive…");
            while (flag)
            {
                try
                {

                    byte[] data = new byte[1024];
                    int recv = sock.ReceiveFrom(data, ref ep);
                    string stringData = Encoding.UTF8.GetString(data, 0, recv);

                    Console.WriteLine("received: {0} from: {1}", stringData, ep.ToString());
                    PhoneInfo phone = JsonConvert.DeserializeObject<PhoneInfo>(stringData);
                    CommonInformation.phone = phone;//保存当前连接手机

                    Socket phoneSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    phoneSocket.Connect(new IPEndPoint(IPAddress.Parse(phone.param.First().ip), phone.param.First().port));
                    PhoneParam pp = new PhoneParam();
                    pp.ip = GetInternalIP();
                    pp.port = 8888;
                    List<PhoneParam> param = new List<PhoneParam>();
                    param.Add(pp);
                    PhoneInfo info = new PhoneInfo(param, "PC_Y", "default");
                    phoneSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info)));
                    Console.WriteLine("返回手机信息： "+ JsonConvert.SerializeObject(info));
                    phoneSocket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("broadcast error:  "+e);
                }
            }
        }
        //获取内网IP
        private string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        public void close()
        {
            flag = false;
        }
        //处理手机发送过的tv信息,保存起来.
        public void FuncMSG(string msg)
        {
            PhoneInfo phone = JsonConvert.DeserializeObject<PhoneInfo>(msg);
            foreach(PhoneInfo p in CommonInformation.tvs)
            {
                if (p.equals(p)) return;
            }
            CommonInformation.tvs.Add(phone);
        }
    }
}

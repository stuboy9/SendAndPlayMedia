using Newtonsoft.Json;
using SendAndPlayMedia.info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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

                    
                    BroadInfo phone = JsonConvert.DeserializeObject<BroadInfo>(stringData);
                    //Console.WriteLine("received: {0} from: {1}", stringData, ep.ToString());
                    foreach(BroadParam bp in phone.param)
                    {
                        if (bp.launch_time_id == null) continue;
                        if (!MyInfo.phone.Contains(bp))
                        {
                            Console.WriteLine("received: {0} from: {1}", stringData, ep.ToString());
                            Console.WriteLine("test    " + JsonConvert.SerializeObject(bp));
                            Socket phoneSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            phoneSocket.Connect(new IPEndPoint(IPAddress.Parse(bp.ip), bp.port));
                            BroadParam pp = new BroadParam();
                            pp.ip = GetInternalIP();
                            pp.port = 8888;
                            pp.launch_time_id = MyInfo.launch_time_id;
                            List<BroadParam> param = new List<BroadParam>();
                            param.Add(pp);
                            BroadInfo info = new BroadInfo(param, "PC_Y", "default");
                            phoneSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info)));
                            Console.WriteLine("返回手机信息： " + JsonConvert.SerializeObject(info));
                            phoneSocket.Close();
                            int len = MyInfo.phone.Count;
                            if (len >= 50)
                            {
                                MyInfo.phone.Clear();
                                continue;
                            }
                            for (int i = MyInfo.phone.Count - 1; i >= 0; i--)
                            {
                                if (MyInfo.phone[i].ip == bp.ip && MyInfo.phone[i].port == bp.port)
                                {
                                    MyInfo.phone.RemoveAt(i);
                                    break;
                                }
                            }
                            MyInfo.phone.Add(bp);
                        }
                    }
                    
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("broadcast error:  "+e);
                }
            }
        }
        //获取内网IP
        private static string getIPAddress()
        {
            System.Net.IPAddress addr;
            addr = new System.Net.IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            return addr.ToString();
        }
        private static string GetInternalIP()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection moc = mc.GetInstances();
            string MACAddress = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    return ((String[])mo["IPAddress"])[0];
                }
            }
            return "127.0.0.1";
        }

        public void close()
        {
            flag = false;
        }
        //处理手机发送过的tv信息,保存起来.
        public void FuncMSG(string msg)
        {
            BroadInfo phone = JsonConvert.DeserializeObject<BroadInfo>(msg);
            BroadParam b = phone.param.First();
            if (MyInfo.tvs.Contains(b)) return;
            if (MyInfo.tvs.Count > 50) MyInfo.tvs.Clear();
            MyInfo.tvs.Add(b);
        }
         
    }
}

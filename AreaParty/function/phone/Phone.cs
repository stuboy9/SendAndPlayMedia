using AreaParty.info;
using AreaParty.info.phone;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AreaParty.function.phone
{
    class Phone
    {
        int port = ConfigResource.BROADCAST_PORT;
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Boolean flag = false;
        public Phone(int port= ConfigResource.BROADCAST_PORT)
        {
            this.port = port;
        }
        public void start()
        {
            if (flag == false)
            {
                flag = true;
                Thread t = new Thread(run);
                t.IsBackground = true;
                t.Start();
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
            log.Info("Broadcast Ready to receive…");
            while (flag)
            {
                try
                {

                    byte[] data = new byte[1024];
                    int recv = sock.ReceiveFrom(data, ref ep);
                    string stringData = Encoding.UTF8.GetString(data, 0, recv);

                    
                    BroadInfo phone = JsonConvert.DeserializeObject<BroadInfo>(stringData);

                    foreach (BroadParam bp in phone.param)
                    {
                        if (bp.launch_time_id == null) continue;
                        //log.InfoFormat("received: {0} from: {1}", stringData, ep.ToString());
                        Socket phoneSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        phoneSocket.Connect(new IPEndPoint(IPAddress.Parse(bp.ip), bp.port));
                        BroadParam pp = new BroadParam();
                        pp.name = Environment.MachineName;
                        pp.ip = GetInternalIP();
                        pp.mac = MyInfo.myMAC;
                        pp.port = ConfigResource.MYSERVER_PORT;
                        pp.launch_time_id = MyInfo.launch_time_id;
                        List<BroadParam> param = new List<BroadParam>();
                        param.Add(pp);
                        BroadInfo info = new BroadInfo(param, "PC_Y", "default");
                        phoneSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info)));
                        //log.Info("返回手机信息： " + JsonConvert.SerializeObject(info));
                        
                        phoneSocket.Close();
                        int len = MyInfo.phone.Count;
                        if (len >= 50)
                        {
                            MyInfo.phone.Clear();
                            continue;
                        }
                        MyInfo.phone.Add(bp);
                    }
                }
                catch (SocketException e)
                {
                    log.Error("广播错误", e);
                }
            }
        }
        /// <summary>
        /// 获取内网IP
        /// </summary>
        private static string getIPAddress()
        {
            System.Net.IPAddress addr;
            addr = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
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
            foreach(BroadParam b in phone.param)
            {
                if (!MyInfo.tvs.Contains(b)) MyInfo.tvs.Add(b);
                info.tv.TVInfo tv = new info.tv.TVInfo(b.launch_time_id,b.name,b.launch_time_id,b.ip,false,false,false);
                tv.timeStamp = tv.timeStamp = function.tv.TVFunction.ConvertDateTimeInt(DateTime.Now);
                if (!MyInfo.tvLibrary.value.Contains(tv)) MyInfo.tvLibrary.value.Add(tv);
            }
        }
         
    }
}

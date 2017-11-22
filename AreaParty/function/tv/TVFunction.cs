using AreaParty.command;
using AreaParty.function.dlna;
using AreaParty.info;
using AreaParty.info.tv;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AreaParty.function.tv
{
    class TVFunction
    {
        private int timeout = 15;//超时时间
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Start()
        {
           
            Thread t2 = new Thread(CheckTimeout);
            t2.IsBackground = true;
            t2.Start();
        }
        /// <summary>
        /// 检测超时机制，定期检测tv是否存活(向pc发送消息),否则从tv列表中移除.
        /// </summary>
        public void CheckTimeout()
        {
            while (true)
            {
                int time = ConvertDateTimeInt( DateTime.Now);
                int len = MyInfo.tvLibrary.value.Count();
                for (int i = len - 1; i >= 0; i--)
                {


                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    // Connect using a timeout (5 seconds)

                    IAsyncResult result = socket.BeginConnect(MyInfo.tvLibrary.value[i].ip,12548 , null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                    
                    if (!socket.Connected)
                    {
                        // NOTE, MUST CLOSE THE SOCKET

                        socket.Close();
                        //throw new ApplicationException("Failed to connect server.");
                        lock (MyInfo.locker)
                        {
                            MyInfo.tvLibrary.value.RemoveAt(i);
                        }
                        Console.WriteLine("删除");
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
        /// <summary>
        /// 此功能在miracast情况下使用，用于pc向tv发送命令，通知pc打开miracast接受软件
        /// </summary>
        /// <param name="ipadress">ip地址</param>
        public static void sendCommand(string ipadress,TVCommand tvcommand)
        {
            Socket serverSocket = null;
            try
            {
                IPAddress ip = IPAddress.Parse(ipadress);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(new IPEndPoint(ip, 12548));
                byte[] result = new byte[1024];
                log.InfoFormat("向TV {0} 发送命令: {1}", ipadress, JsonConvert.SerializeObject(tvcommand));
                serverSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tvcommand)));
                serverSocket.Close();
            }
            catch(Exception e)
            {
                log.Error("向TV发送命令失败", e);
                //Console.WriteLine(e);
            }
            finally
            {
                serverSocket.Close();
            }
        }
        
    }
    
     
}

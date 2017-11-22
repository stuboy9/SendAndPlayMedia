using AreaParty.command;
using AreaParty.info;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AreaParty.function.webservice
{
    
    class WebOperation
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 向服务器报告自己软件信息，使用UDP
        /// </summary>
        public static void ReportInfo()
        {
            MsgObj msg = new MsgObj("pc", Environment.MachineName, MyInfo.myMAC, ConvertDateTimeInt(DateTime.Now));

            Thread t = new Thread(new ThreadStart(delegate () {
                while (true)
                {
                    try
                    {

                        IPAddress remoteIP = IPAddress.Parse(ConfigResource.SERVER_2_IP); //假设发送给这个IP
                        IPEndPoint remotePoint = new IPEndPoint(remoteIP, ConfigResource.SERVER_2_PORT);
                        UdpClient client = new UdpClient();
                        byte[] sendData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));//要发送的字节数组 
                        client.Send(sendData, sendData.Length, remotePoint);//将数据发送到远程端点 
                        log.InfoFormat("向服务器 {0}汇报消息成功", ConfigResource.SERVER_2_IP);
                        client.Close();//关闭连接 
                    }
                    catch (Exception e)
                    {
                        log.Error("向服务器汇报消息失败", e);
                    }
                    Thread.Sleep(1000 * 60 * 20);
                }
                

            }));
            t.IsBackground = true;
            t.Start();
            
        }
        /// <summary>
        /// 向服务器发送自己的版本，服务器判断是否是最新版本，如果不是最新版本，则返回新版本的URL
        /// </summary>
        public static void GetUudateInfo()
        {
            string msg = JsonConvert.SerializeObject(new UpdateMsgObj("pc", "1.0.0."));
            Thread t = new Thread(new ThreadStart(delegate ()
             {
                 IPAddress remoteIP = IPAddress.Parse(ConfigResource.SERVER_2_IP); //假设发送给这个IP
                IPEndPoint remotePoint = new IPEndPoint(remoteIP, ConfigResource.UPDATE_PORT);
                 UdpClient client = new UdpClient();
                 client.Client.ReceiveTimeout = 10000;
                 byte[] sendData = Encoding.UTF8.GetBytes(msg);//要发送的字节数组 
                while (true)
                 {
                     try
                     {
                         client.Send(sendData, sendData.Length, remotePoint);//将数据发送到远程端点 
                        byte[] rec = client.Receive(ref remotePoint);
                         UpdateMsgObjREV msgrev = JsonConvert.DeserializeObject<UpdateMsgObjREV>(Encoding.UTF8.GetString(rec, 0, rec.Length));
                         if (!msgrev.isNew)
                         {
                             log.Info("最新版本为" + msgrev.version);
                             if (System.Windows.MessageBox.Show("当前有新版本更新", "更新", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                             {

                                 System.Diagnostics.Process.Start(msgrev.url);
                             }
                         }
                         return;
                     }
                     catch (Exception e)
                     {
                     }

                 }

                //client.Close();//关闭连接 
            }));
            t.IsBackground = true;
            t.Start();
        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static long ConvertDateTimeInt(System.DateTime time)
        {
            
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        internal class MsgObj
        {
            public string id;
            public string mac;
            public long time;
            public string type;

            public MsgObj(string type,string id,string mac,long time)
            {
                this.type = type;
                this.id = id;
                this.mac = mac;
                this.time = time;
            }
        }

        internal class UpdateMsgObj
        {
            public string type;
            public string version;
            public UpdateMsgObj(string type,string version)
            {
                this.type = type;
                this.version = version;
            }
        }
        internal class UpdateMsgObjREV
        {
            public bool isNew;
            public string url;
            public string version;
            public UpdateMsgObjREV(string url, string version,bool isNew)
            {
                this.url = url;
                this.version = version;
                this.isNew = isNew;
            }
        }
    }
    
}

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
using AreaParty.function.tv;
using AreaParty.function.dlna;
using AreaParty.function.media;
using AreaParty.function.phone;
using AreaParty.command;
using System.Configuration;
using log4net;
using System.Reflection;
using Microsoft.Win32;

namespace AreaParty
{
    
    class Program
    {
        public static string ip = util.config.GetIPAddress.GetActivatedAdaptorMacAddress();
        
        private static void IpExit(string IP)
        {
            if(IP == null)
            {
                IPAddress ipAddr = Dns.Resolve(Dns.GetHostName()).AddressList[0];
                ip = ipAddr.ToString();
            }
        }

        private static void InitLog4Net()
        {
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logCfg);
        }
        public static void Main1(string[] args)
        {

            bool isOpen = false;
            function.window.WindowControl.SetTaskbarState(function.window.WindowControl.AppBarStates.AutoHide);//隐藏任务栏
            InitLog4Net();//初始化日志
            ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            log.Info(string.Format("通过连接8.8.8.8获得的ip为：{0}", ip));
            try
            {
                IpExit(ip);
                log.Info("当前正在使用的IP为：" + ip);
            }
            catch (Exception e)
            {
                log.Info(string.Format("捕获异常：{0}", e.Message));
            }
           
            TVFunction tv = new TVFunction();
            tv.Start();//启动tv线程，检测TV是否在线
            isOpen = function.pcapp.PCApp.Open();//打开其他PC应用
            while (!isOpen)
            {
                function.pcapp.PCApp.CLoseAll();
                isOpen = function.pcapp.PCApp.Open();
            }
            //Thread.Sleep(3000);//必须等待一段时间，下个模块初始化依赖于上个模块。

            MediaFunction media = new MediaFunction();
            media.init();//初始化多媒体，将媒体库和缩略图添加到HTTP服务器
            
           

            Phone phone = new Phone();
            phone.start();//开启手机线程，接受广播

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            byte[] result = new byte[1024];
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, info.ConfigResource.MYSERVER_PORT));  //绑定IP地址：端口  
            serverSocket.Listen(40);    //设定最多40个排队连接请求  
            //Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            log.Info(String.Format("启动监听{0}成功", serverSocket.LocalEndPoint.ToString()));
            //通过Clientsoket发送数据  
            while (true)
            {
               //Console.WriteLine("等待连接");
                Socket myClientSocket = serverSocket.Accept();
                log.Info(String.Format("{0}使用端口{1}连接成功", ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Address, ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Port));
                //Console.WriteLine("{0}使用端口{1}连接成功", ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Address, ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Port);
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    log.Info(String.Format("接受客户端{0}发送过来的消息: {1}", ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Address, rev));
                    //Console.WriteLine("接受客户端{0}发送过来的消息: {1}", ((System.Net.IPEndPoint)myClientSocket.RemoteEndPoint).Address, rev);
                    if (receiveNumber <= 0)
                    {
                        myClientSocket.Close();
                        continue;
                    }
                    //验证数据是tv还是手机传来，如果是tv，则转发给原来处理程序处理。
                    JsonTextReader reader = new JsonTextReader(new StringReader(rev));
                    JObject obj = JObject.Parse(rev);
                    if (obj.Property("source") != null)//phone发过来有关tv的消息，依然交给phone处理模块
                    {
                        phone.FuncMSG(rev);
                        myClientSocket.Close();
                        continue;
                    }

                    Command com = JsonConvert.DeserializeObject<Command>(rev);
                    Thread t = new Thread(new ParameterizedThreadStart(com.handle));
                    t.Start(myClientSocket);
                    
                }
                catch (Exception ex)
                {
                    log.Error("网络连接错误或者接受的数据格式不对",ex);
                    //Console.WriteLine(ex.Message);
                    //myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                }

            }
        }
        /// <summary>
        /// 检测PC是否被锁屏和是否进入RDP锁屏界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.RemoteConnect)
            {
                //I left my desk
                //Console.WriteLine("lock");
                info.MyInfo.IsSreenLock = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.ConsoleConnect)
            {
                //I returned to my desk
                info.MyInfo.IsSreenLock = false;
                //Console.WriteLine(DateTime.Now);
                //Console.WriteLine("Unlock");
            }
        }

    }
}

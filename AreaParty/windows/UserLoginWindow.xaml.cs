﻿using AreaParty.command;
using AreaParty.info;
using client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AreaParty.windows
{
    /// <summary>
    /// UserLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserLoginWindow : Window
    {
        public UserLoginWindow()
        {
            InitializeComponent();
            string checkbox = util.config.ConfigUtil.GetValue("usercheckbox");
            if (checkbox.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                checkBox.IsChecked = true;
                this.UserNameTextBox.Text = util.config.ConfigUtil.GetValue("username");
                this.PasswordTextBox.Password = util.config.ConfigUtil.GetValue("password");
            }
            else
            {
                checkBox.IsChecked = false;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {

            //this.AutoLoginCheckBox.IsChecked;
            //this.RememberPasswordCheckBox.IsChecked;
            //this.UserNameTextBox.Text;
            //this.PasswordTextBox.Text;


            string name = this.UserNameTextBox.Text;
            string password = this.PasswordTextBox.Password;
            if(checkBox.IsChecked == true)
            {
                util.config.ConfigUtil.SetValue("usercheckbox", "true");
            }
            else
            {
                util.config.ConfigUtil.SetValue("usercheckbox", "false");
            }
            User u = new User(name, password);
            Thread t = new Thread(Login);
            //t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start(u);

        }
        public void Login(object o)
        {
            
            User u = o as User;
            Init(u.name, u.password);
            //Login(u.name, u.password);
            //System.Windows.Threading.Dispatcher.Run();
        }

        public void SendToServer(TcpClient tcpClient, String userId, String psw)
        {
            BinaryWriter bw;
            LoginReq loginReq = new LoginReq
            {
                userId = userId,
                userPassword = psw,
                loginType = LoginReq.LoginType.PC,
                userMac = GetMacAddress().Split(',')[0],
                mobileInfo = GetMacAddress().Split(',')[1]
            };
            MemoryStream ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, loginReq);
            byte[] data = ms.ToArray();
            NetworkStream clientStream = tcpClient.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write(data, 0, data.Length);
            bw.Flush();
        }

        public void Init(string name, string password)
        {
            string serverIp = "119.23.12.116";
            try
            {
                IPAddress remoteIP = IPAddress.Parse(serverIp);
                TcpClient mainServerTcpClient = new TcpClient();
                mainServerTcpClient.Connect(new IPEndPoint(remoteIP, 13333));
                SendToServer(mainServerTcpClient, name, password);
                LoginRsp result = LoginResult(mainServerTcpClient);
                if (LoginRsp.ResultCode.FAIL.Equals(result.resultCode))
                {
                    MessageBox.Show("用户名或密码错误，请重新输入");
                    mainServerTcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.LOGGEDIN.Equals(result.resultCode))
                {
                    MessageBox.Show("该账号已登录");
                    mainServerTcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.NOTMAINPHONE.Equals(result.resultCode))
                {
                    MessageBox.Show("该设备不是主设备");
                    Window loading = null;
                    Thread t = new Thread(new ThreadStart(delegate
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            loading = new LoadingWindow();
                            loading.ShowDialog();
                        }));
                    }));
                    t.IsBackground = true;
                    t.Start();

                    Console.WriteLine("waite result");
                    AccreditRsp accreditResult = AccreditResult(mainServerTcpClient);
                    if (AccreditRsp.ResultCode.CANLOGIN.Equals(accreditResult.resultCode))
                    {
                        SendToServer(mainServerTcpClient, name, password);
                        MessageBox.Show("主设备同意登录");
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (loading != null) loading.Close();
                        }));
                        MessageBox.Show("授权成功");
                        util.config.ConfigUtil.SetValue("longconnect", "true");
                        util.config.ConfigUtil.SetValue("username", name);
                        util.config.ConfigUtil.SetValue("password", password);
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            this.Close();
                        }));
                        MainWindow.main.Status = "长连接开启";
                    }
                    else if (AccreditRsp.ResultCode.FAIL.Equals(accreditResult.resultCode))
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (loading != null) loading.Close();
                        }));
                        MessageBox.Show("授权被拒绝");
                        MainWindow.main.Status = "长连接失败";
                        mainServerTcpClient.Close();
                        return;
                    }
                }
                else if (LoginRsp.ResultCode.MAINPHONEOUTLINE.Equals(result.resultCode))
                {
                    MessageBox.Show("主设备不在线");
                    MainWindow.main.Status = "长连接失败";
                    mainServerTcpClient.Close();
                    return;
                }
                else if (LoginRsp.ResultCode.SUCCESS.Equals(result.resultCode))
                {
                    MessageBox.Show("登录成功");
                    util.config.ConfigUtil.SetValue("longconnect", "true");
                    util.config.ConfigUtil.SetValue("username", name);
                    util.config.ConfigUtil.SetValue("password", password);
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        this.Close();
                    }));
                    // can also be any object you'd like to use as a parameter
                    MainWindow.main.Status = "长连接已开启";
                    //启动长连接
                    ClientPCNew2.StartService(mainServerTcpClient);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private static AccreditRsp AccreditResult(TcpClient tcpClient)
        {
            AccreditRsp result = null;
            BinaryReader br;
            try
            {
                byte[] byteArray = new byte[10000];
                NetworkStream clientStream = tcpClient.GetStream();
                br = new BinaryReader(clientStream);
                int len = br.Read(byteArray, 0, byteArray.Length);
                byte[] data = new byte[len];

                Array.Copy(byteArray, data, len);
                MemoryStream ms = new MemoryStream(data);
                result = Serializer.Deserialize<AccreditRsp>(ms);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            finally
            {

            }
            return result;
        }
        private static LoginRsp LoginResult(TcpClient tcpClient)
        {
            LoginRsp result = null;
            BinaryReader br;
            try
            {
                byte[] byteArray = new byte[2048];
                NetworkStream clientStream = tcpClient.GetStream();
                br = new BinaryReader(clientStream);
                int len = br.Read(byteArray, 0, byteArray.Length);
                byte[] data = new byte[len];
                Array.Copy(byteArray, data, len);
                MemoryStream ms1 = new MemoryStream(data);
                result = Serializer.Deserialize<LoginRsp>(ms1);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            return result;
        }
        /// <summary>
        /// 获取PCMac地址
        /// </summary>
        /// <returns>string mac</returns>
        public static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] bytes = nic.GetPhysicalAddress().GetAddressBytes();
                    foreach (sbyte b in bytes)
                    {
                        //与11110000作按位与运算以便读取当前字节高4位  
                        sb.Append(((b & 240) >> 4).ToString("x"));
                        //与00001111作按位与运算以便读取当前字节低4位  
                        sb.Append((b & 15).ToString("x"));
                        sb.Append("-");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    return sb.ToString().ToUpper() + "," + Dns.GetHostName();
                }
            }
            return null;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.Status = "长连接断开";
            this.Close();
        }

       

        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string name = this.UserNameTextBox.Text;
            if (name == "Areaparty账号")
            {
                this.UserNameTextBox.Text = "";
            }
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.UserNameTextBox.Text == "")
            {
                this.UserNameTextBox.Text = "Areaparty账号";
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                string user = util.config.ConfigUtil.GetValue("username");
                string password = util.config.ConfigUtil.GetValue("password");
                if ((this.UserNameTextBox.Text != "") &&(this.PasswordTextBox.Password != ""))
                {
                    util.config.ConfigUtil.SetValue("username", this.UserNameTextBox.Text);
                    util.config.ConfigUtil.SetValue("password", this.PasswordTextBox.Password);
                    
                }
                util.config.ConfigUtil.SetValue("usercheckbox", checkBox.IsChecked.ToString());
                //else
                //{
                //    this.UserNameTextBox.Text = user;
                //    this.PasswordTextBox.Password = password;
                //}
            }
            else
            {
                util.config.ConfigUtil.SetValue("usercheckbox", "false");
            }
            
        }
    }
    public class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }
        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }
        public static readonly DependencyProperty IsMonitoringProperty =
         DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));
        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }
        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }
        public static readonly DependencyProperty PasswordLengthProperty =
         DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));
        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }
        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
    class User
    {
        public string name = null;
        public string password = null;
        public User(string name,string password)
        {
            this.name = name;
            this.password = password;
        }
    }
}


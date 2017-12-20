using AreaParty.command;
using AreaParty.function.app;
using AreaParty.info;
using AreaParty.util;
using AreaParty.util.config;
using AreaParty.windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static AreaParty.App;

namespace AreaParty
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon notifyIcon;
        internal static MainWindow main;//为了设置登录状态所以使用如下方法
        internal string Status
        {
            get { return Login_Label.Content.ToString(); }
            set { try { Dispatcher.Invoke(new Action(() => { Login_Label.Content = value; })); } catch (Exception e) { } }
        }
        internal string Status_Nas
        {
            get { return Nas_Label.Content.ToString(); }
            set { try { Dispatcher.Invoke(new Action(() => { Nas_Label.Content = value; })); } catch (Exception e) { } }
        }

        public MainWindow()
        {
            main = this;
            if (!System.IO.Directory.Exists(MyInfo.myDataPath))
            {
                //Console.WriteLine(MyInfo.myDataPath);
                System.IO.Directory.CreateDirectory(MyInfo.myDataPath);
                //Console.WriteLine(MyInfo.iconFolder);
                System.IO.Directory.CreateDirectory(MyInfo.iconFolder);
                //Console.WriteLine(MyInfo.myDataPath + "\\MediaLibraryTemp");
                System.IO.Directory.CreateDirectory(MyInfo.myDataPath + "\\MediaLibraryTemp");
                System.IO.File.Copy(System.Windows.Forms.Application.StartupPath + "./AreaParty.exe.config", MyInfo.execonfig);
            }
            
            if (System.IO.File.Exists(MyInfo.execonfig))
                AppConfig.Change(MyInfo.execonfig);
            if (String.IsNullOrEmpty(PairCodeConfig.GetPairCode()))
            {
                bool result = new PairCodeWindow().ShowDialog().Value;

                if (result == false)
                {
                    System.Windows.Application.Current.Shutdown();
                    System.Windows.Forms.Application.Exit();
                }

            }
            Thread t = new Thread(new ThreadStart(Run));
            t.Name = "NoParameterThread";
            t.IsBackground = true;
            t.Start();
            
            if (ConfigUtil.GetValue("init").Equals("false"))
            {
                windows.InitSetting ist = new windows.InitSetting();
                ist.ShowDialog();
                AppScan.ScanAndUpdateMySoftware();
                util.JAVAUtil.AddSourceToHTTP(info.MyInfo.iconFolder);
                ConfigUtil.SetValue("init", "true");
            }
            function.webservice.WebOperation.ReportInfo();
            function.webservice.WebOperation.GetUudateInfo();
            InitializeComponent();
            new Thread(new ThreadStart(delegate ()
            {
                if (ConfigUtil.GetValue("longconnect").Equals("true"))
                {
                    try
                    {
                        Thread.Sleep(1000 * 10);
                        IPAddress ip = IPAddress.Parse("127.0.0.1");
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(ip, ConfigResource.HOLE_PORT));
                        string name = ConfigUtil.GetValue("username");
                        string password = ConfigUtil.GetValue("password");
                        Command c = CommandFactory.GetLoginCommand(name, password);
                        socket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c)));
                        socket.Send(Encoding.UTF8.GetBytes("\r\n"));
                        socket.Close();
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            this.Login_Label.Content = "长连接开启";
                        }));

                    }
                    catch (Exception e)
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            this.Login_Label.Content = "长连接失败";
                        }));

                        Console.WriteLine(e);
                    }

                }
            })).Start();
            
            Icon();
            //MessageBox.Show("" + ConfigUtil.IsExistsScreen("user"));
            
        }
        public void Icon()
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "hello,AreaParty软件正式启动";
            this.notifyIcon.Text = "AreaParty家庭多媒体";
            this.notifyIcon.Icon =  System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.StartupPath+ "\\Logo.ico");
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            this.notifyIcon.ShowBalloonTip(1000);

            System.Windows.Forms.ContextMenu cn = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem OpenMenuItem = new System.Windows.Forms.MenuItem();
            OpenMenuItem.Text = "打开主面板";
            OpenMenuItem.Click += new EventHandler(OpenClick);//菜单点击的事件
            cn.MenuItems.Add(OpenMenuItem);

            System.Windows.Forms.MenuItem CloseMenuItem = new System.Windows.Forms.MenuItem();
            CloseMenuItem.Text = "退出软件";
            CloseMenuItem.Click += new EventHandler(CloseClick);//菜单点击的事件

            cn.MenuItems.Add(CloseMenuItem);

            this.notifyIcon.ContextMenu = cn;
            
        }

        private void CloseClick(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            function.pcapp.PCApp.CLoseAll();
            //function.window.WindowControl.SetTaskBarState(9);//恢复任务栏
            this.Close();
            System.Windows.Application.Current.Shutdown();
            System.Windows.Forms.Application.Exit();
            Environment.Exit(0);
            //System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

        }

        private void OpenClick(object sender, EventArgs e)
        {
            //new function.window.WindowControl().ShowMyWindow("Like360Main");
            this.Show();
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            //new function.window.WindowControl().ShowMyWindow("Like360Main");
            this.Show();
        }

        private void Run()
        {
            String[] args = null;
            AreaParty.Program.Main1(args);
        }
        
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Collapsed;
            this.Hide();
            //new function.window.WindowControl().HideWindow(Process.GetCurrentProcess().MainWindowHandle);
        }
         
        private void maxButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void mniButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void menuButton_Click(object sender, RoutedEventArgs e)
        {
            Menu.IsOpen = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //拖动
            try
            {
                this.DragMove();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee);
            }
            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("main");
            if (e.Source is System.Windows.Controls.TabControl)
            {
                //do work when tab is changed 

            }
            else
            {
                return;
            }
            int index = this.tab360.SelectedIndex;
            //if (index == 0)
            //{
            //    //可以设置TransitionType WpfPage 来更改界面出入的动画效果
            //    //this.pTransitionControl_1.TransitionType = WpfPageTransitions.PageTransitionType.SpinAndFade;
            //    pages.Screen newPage = new pages.Screen();
            //    this.pTransitionControl_1.ShowPage(newPage);

            //}

            //else if (index == 1)
            if (index == 0)
            {
                pages.MediaPage newPage = new pages.MediaPage();
                this.pTransitionControl_1.ShowPage(newPage);
            }
            else if (index == 1)
            {
                pages.AppPage newPage = new pages.AppPage();
                this.pTransitionControl_2.ShowPage(newPage);
            }
            else if(index == 2)
            {
                pages.SettingPage newPage = new pages.SettingPage();
                this.pTransitionControl_3.ShowPage(newPage);
                //pages.GamePage newPage = new pages.GamePage();
                //this.pTransitionControl_3.ShowPage(newPage);
            }
            //else
            //{ 
            //    pages.SettingPage newPage = new pages.SettingPage();
            //    this.pTransitionControl_4.ShowPage(newPage);
            //}
        }

        private void MenuButton_Setting_Click(object sender, RoutedEventArgs e)
        {
            Window test = new windows.InitSetting();
            test.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            test.Owner = this;
            bool result =  test.ShowDialog().Value;
            
        }
 

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            CloseClick(sender, (EventArgs)e);
            //this.notifyIcon.Visible = false;
            //function.pcapp.PCApp.CLoseAll();
            //this.Close();
        }

        private void Pair_Click(object sender, RoutedEventArgs e)
        {
            Window PairCodeWindow = new PairCodeWindow();
            PairCodeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            PairCodeWindow.Owner = this;
            bool result = PairCodeWindow.ShowDialog().Value;

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            function.pcapp.PCApp.CLoseAll();
            Environment.Exit(0);
        }
    }
}

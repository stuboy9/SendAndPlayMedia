using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AreaParty
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }
        [STAThread]
        static void Main()
        {
            //单例模式创建唯一窗口
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(new[] { "teste" });
        }
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            //SingleInstanceManager manager = new SingleInstanceManager();
            //manager.app = this;
            //manager.Run(new[] { "teste" });

            //try
            //{
            //    base.OnStartup(e);
            //    Process p = RunningInstance();
            //    if (p == null)
            //    {
            //        Window main = new MainWindow();
            //        main.Show();
            //    }
            //    else
            //    {
            //        new function.window.WindowControl().ActivateWindow(p, "Like360Main");
            //        this.Shutdown();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(App.Current.MainWindow as MainWindow, ex.Message, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //    App.Current.Shutdown();
            //}

        }

        /// <summary>
        /// 获取正在运行的程序，没有运行的程序则返回null
        /// </summary>
        /// <returns></returns>
        //private static Process RunningInstance()
        //{
        //     获取当前活动的进程
        //    Process currentProcess = Process.GetCurrentProcess();
        //     根据当前进程的进程名获得进程集合
        //     如果该程序运行，进程的数量大于1
        //    Process[] processcollection = Process.GetProcessesByName(currentProcess.ProcessName.Replace(".vshost", ""));
        //    foreach (Process process in processcollection)
        //    {
        //         如果进程ID不等于当前运行进程的ID以及运行进程的文件路径等于当前进程的文件路径
        //         则说明同一个该程序已经运行了，此时将返回已经运行的进程
        //        if (process.Id != currentProcess.Id)
        //        {
        //            if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == process.MainModule.FileName)
        //            {
        //                return process;
        //            }
        //        }
        //    }
        //    return null;
        //}
        
    }

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        public SingleInstanceApplication app;

        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            //First time app is launched
            app = new SingleInstanceApplication();
            app.Run();
            //app.Start();
            //MessageBox.Show("first");
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            app.Activate();
        }
    }

    public class SingleInstanceApplication : Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Uri resourceLocater = new System.Uri("/AreaParty;component/app.xaml", System.UriKind.Relative);
            #line 1 "..\..\App.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            // Create and show the application's main window
            Window main = new MainWindow();
            
            main.Show();
        }

        public void Activate()
        {
            // Reactivate application's main window

            this.MainWindow.WindowState = WindowState.Normal;
            this.MainWindow.Show();
            this.MainWindow.Activate();
        }
    }


}

using AreaParty.info.applacation;
using AreaParty.util.config;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.function.app
{
    class Applacation
    {
        private string data = "";
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        private static extern void SetForegroundWindow(IntPtr hwnd);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;


        public Applacation()
        {
            
        }
        
        public void ProduceAPPIcon(string apppath, string name, string iconpath)
        {
            util.IconUtil.GetIconFromFile(apppath, 2, iconpath + "\\" + name + ".png");
        }
        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <returns>应用列表</returns>
        public List<ApplacationItem> GetAppList()
        {
            return ApplicationConfig.GetAllApp();
        }
        /// <summary>
        /// 打开应用
        /// </summary>
        /// <param name="name">应用名称</param>
        /// <param name="param">启动参数</param>
        /// <returns></returns>
        public Process OpenApp(string name, string param)
        {
            try
            {

                CloseAll();
                new game.Game().CloseAllGame();
                FileInfo fi = new FileInfo(name);
                Process rdcProcess = new Process();
                rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(name);
                rdcProcess.StartInfo.Arguments = param;
                rdcProcess.StartInfo.WorkingDirectory = fi.Directory.ToString();
                rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                rdcProcess.Start();
                ///*获取启动的进程的句柄并将窗口最大化*/
                //IntPtr MainWindowHande = rdcProcess.MainWindowHandle;
                //string mainwindowhande = MainWindowHande.ToString();
                //IntPtr mainHandle = FindWindow(mainwindowhande, mainwindowhande);
                
                //if (mainHandle ==IntPtr.Zero)
                //{
                //    while (mainHandle == IntPtr.Zero)
                //    {
                //        System.Threading.Thread.Sleep(1000);
                //        mainHandle = FindWindow(mainwindowhande, mainwindowhande);
                //    }
                //    log.Info(string.Format("获取应用\"{0}\"的窗口句柄\"{1}\"", name, mainHandle.ToString()));
                //    SetForegroundWindow(mainHandle);
                //    SendMessage(mainHandle, WM_SYSCOMMAND, SC_MAXIMIZE, 0); // 最大化
                //}
                return rdcProcess;
            }
            catch (Exception e)
            {
                log.Error("打开软件错误" , e);
                return null;
                //throw new Exception(e + "打开程序异常异常");
            }
        }
        /// <summary>
        /// 关闭所有已启动的应用
        /// </summary>
        public void CloseAll()
        {
            try
            {
                List<ApplacationItem> list = GetAppList();
                foreach (ApplacationItem item in list)
                {
                    CloseApp(item.packageName.Split('\\').Last().Split('.').First());
                }

            }
            catch (Exception e)
            {
                log.Error("调用CloseAll函数报错:", e);
            }

        }
        /// <summary>
        /// 关闭指定应用
        /// </summary>
        /// <param name="name">应用名称</param>
        public void CloseApp(string name)
        {
            try
            {
                Process[] ps = Process.GetProcesses();
                foreach (Process item in ps)
                {
                    if (item.ProcessName == name)
                    {
                        item.Kill();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("调用CloseApp函数报错:", e);
            }

        }
    }
}


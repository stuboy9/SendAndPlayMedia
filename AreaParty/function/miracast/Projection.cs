using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.Security.Principal;
using System.Security.AccessControl;
using Windows.System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using AreaParty.info.miracast;
using AreaParty.function.window;
using System.Reflection;
using log4net;

namespace AreaParty.function.miracast
{
    /// <summary>
    /// 投影模式
    /// </summary>
    enum DisplayMode
    {
        Internal,
        External,
        Extend,
        Duplicate
    }
    /// <summary>
    /// 当前屏幕
    /// </summary>
    struct CurrentScreen
    {
        public string name;
        public int index;
        public System.Windows.Forms.Screen screen;
        public CurrentScreen(string name,int index, System.Windows.Forms.Screen screen) : this()
        {
            this.name = name;
            this.index = index;
            this.screen = screen;
        }
    }
    /// <summary>
    /// 有关Miracast投影功能都在此类中实现，核心功能获取Miracast列表；选择其中一个设备进行投影。
    /// </summary>
   class Projection
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static CurrentScreen current = new CurrentScreen (null,0,null );
        private static string aqsFilter = "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:= \"{0407d24e-53de-4c9a-9ba1-9ced54641188}\"AND System.Devices.WiFiDirect.Services:~= \"Miracast\"";//获取Miracast设备所用的字符串
        private static DeviceInformationCollection deviceInfoColl = null;//设备信息集合
        /// <summary>
        /// 获取mriacast设备列表
        /// </summary>
        /// <returns></returns>
        public  async Task<MiracastLibrary> GetDeviceList()
        {
            try
            {
                MiracastLibrary library = new MiracastLibrary(new List<info.miracast.Screen>());
                deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
                for(int i = 0; i < deviceInfoColl.Count; i++)
                {
                    library.value.Add(new info.miracast.Screen(deviceInfoColl[i].Name,i));
                }
                //string json = JsonConvert.SerializeObject(library);
                return library;
            }
            catch (Exception e)
            {
                log.Error("读取MIRACAST设备出错", e);
                return new MiracastLibrary(new List<info.miracast.Screen>());
            }
        }
        /// <summary>
        /// 判断当前是否连接Miracast设备
        /// </summary>
        /// <returns></returns>
        public Boolean IsConnection()
        {
            return ProjectionManager.ProjectionDisplayAvailable;
        }
        /// <summary>
        /// 连接Miracast设备
        /// </summary>
        /// <param name="name">Miracast设备的名字</param>
        /// <returns></returns>
        public async Task<bool> SelectProjectionDevice(string name)
        {
            if (deviceInfoColl == null) await GetDeviceList();
            foreach (DeviceInformation d in deviceInfoColl)
            {
                if (d.Name.Equals(name) && IsConnection())
                {
                    current.name = name;
                    current.screen = System.Windows.Forms.Screen.AllScreens.Last();
                    return true;
                }
                else if (d.Name.Equals(name) && !IsConnection())
                {
                    bool b = StartProjection(d);
                    current.name = name;
                    current.screen = System.Windows.Forms.Screen.AllScreens.Last();
                    return b;
                }
            }
            GetDeviceList();
            current.name = name;
            current.screen = System.Windows.Forms.Screen.AllScreens.Last();
            return false;

        }
        /// <summary>
        /// 开始投影
        /// </summary>
        /// <param name="d">投影设备</param>
        /// <returns></returns>
        private static bool StartProjection(DeviceInformation d)
        {
            IAsyncAction projection = ProjectionManager.StartProjectingAsync(0, 0, d);
            int count = 5;
            while (count-- > 0)
            {
                Thread.Sleep(count * 1000);
                if (ProjectionManager.ProjectionDisplayAvailable) return true;
            }
            return false;
        }
        public void CloseProjection()
        {
            SetDisplayMode(DisplayMode.External);
            current.name = null;
            current.index = 0;
            current.screen = null;
        }
        //设置投影模式，external:仅第二屏幕; internal:仅电脑屏幕; extend:扩展; clone:复制;
        public  void SetDisplayMode(DisplayMode mode)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "DisplaySwitch.exe";
            switch (mode)
            {
                case DisplayMode.External:
                    proc.StartInfo.Arguments = "/external";
                    break;
                case DisplayMode.Internal:
                    proc.StartInfo.Arguments = "/internal";
                    break;
                case DisplayMode.Extend:
                    proc.StartInfo.Arguments = "/extend";
                    break;
                case DisplayMode.Duplicate:
                    proc.StartInfo.Arguments = "/clone";
                    break;
            }
            proc.Start();
        }

        //一下目前不用
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public void EnterQS()
        {
            keybd_event(Keys.LWin, 0, 0, 0);
            keybd_event(Keys.S, 0, 0, 0);
            keybd_event(Keys.S, 0, 2, 0);
            keybd_event(Keys.LWin, 0, 2, 0);
        }
        [System.Runtime.InteropServices.DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        public  void ClosePopup()
        {
            try
            {
                WindowControl wc = new WindowControl();
                wc.ActivateWindow("ApplicationFrameWindow", null);
                IntPtr ptr = wc.MyFindWindow("ApplicationFrameWindow", null);
                int k = 0; GetWindowThreadProcessId(ptr, out k);
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
                p.Kill();

            }
            catch(Exception e)
            {

            }
            
        }
        public void MiracastEnter()
        {
            keybd_event(Keys.Tab, 0, 0, 0);
            keybd_event(Keys.Tab, 0, 2, 0);
            keybd_event(Keys.Enter, 0, 0, 0);
            keybd_event(Keys.Enter, 0, 2, 0);
        }
        public void ClearMiracastPopup()
        {
            WindowControl wc = new WindowControl();
            if(wc.ActivateWindow("ApplicationFrameWindow", null))
            {
                Console.WriteLine("发现窗口ApplicationFrameTitleBarWindow");
                //Thread.Sleep(1000);
                MiracastEnter();
            }
        }


    }
}

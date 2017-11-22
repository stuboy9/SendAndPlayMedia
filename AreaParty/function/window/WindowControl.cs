using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AreaParty.function.window
{
    public struct WindowInfo
    {
        public IntPtr hWnd;             // 窗口句柄
        public string szWindowName;     // 窗口标题
        public string szClassName;      // 窗口类名
        public int ProcessId;           // 窗口所属进程ID
    }
    class WindowControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindowAsync(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwndw);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
        public int MyMoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool BRePaint)
        {
            return MoveWindow(hwnd, x, y, nWidth, nHeight, BRePaint);
        }
        /// <summary>
        /// 将窗口移动到制定屏幕
        /// </summary>
        /// <param name="pro">窗口的进程句柄</param>
        /// <param name="screen">移动到的屏幕</param>
        /// <param name="BRePaint">是否刷新</param>
        public void  MoveWindow(Process pro,Screen screen,bool BRePaint)
        {
            new Thread(delegate () {
                int i = 0;
                while (i++ < 15)
                {
                    Thread.Sleep(1000);
                    pro.Refresh();
                    try
                    {
                        if (!pro.MainWindowTitle.Equals(""))
                        {
                            Console.WriteLine(pro.MainWindowTitle);
                            MoveWindow(pro.MainWindowHandle, screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, BRePaint);
                            //break;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                    
                }
            }).Start();
        }
        /// <summary>
        /// 隐藏掉任务栏
        /// </summary>
        /// <param name="Type">0隐藏，9恢复</param>
        /// <returns></returns>
        public static bool SetTaskBarState(int Type)
        {
            string className1 = "Shell_TrayWnd";
            string className2 = "TrayDummySearchControl";
            string mir_className1 = "Shell_SecondaryTrayWnd";
            try
            {
                IntPtr TaskBarFormHandle1 = FindWindow(className1, null);
                IntPtr TaskBarFormHandle2 = FindWindow(className2, null);
                IntPtr TaskBarFormHandle3 = FindWindow(mir_className1, null);
                ShowWindowAsync(TaskBarFormHandle1, Type);
                ShowWindowAsync(TaskBarFormHandle2, Type);
                ShowWindowAsync(TaskBarFormHandle3, Type);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);

        public enum AppBarMessages
        {
            New =
            0x00000000,
            Remove =
            0x00000001,
            QueryPos =
            0x00000002,
            SetPos =
            0x00000003,
            GetState =
            0x00000004,
            GetTaskBarPos =
            0x00000005,
            Activate =
            0x00000006,
            GetAutoHideBar =
            0x00000007,
            SetAutoHideBar =
            0x00000008,
            WindowPosChanged =
            0x00000009,
            SetState =
            0x0000000a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public Rectangle rc;
            public Int32 lParam;
        }

        public enum AppBarStates
        {
            AutoHide =
            0x00000001,
            AlwaysOnTop =
            0x00000002
        }

        /// <summary>
        /// Set the Taskbar State option
        /// </summary>
        /// <param name="option">AppBarState to activate</param>
        public static void SetTaskbarState(AppBarStates option)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("Shell_TrayWnd", null);
            msgData.lParam = (Int32)(option);
            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
            msgData.hWnd = FindWindow("Shell_SecondaryTrayWnd", null);
            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }

        /// <summary>
        /// Gets the current Taskbar state
        /// </summary>
        /// <returns>current Taskbar state</returns>
        public static AppBarStates GetTaskbarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            return (AppBarStates)SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
        }

        /// <summary>
        /// 获指定窗口的id，可以使用vs的spy++工具辅助，参数允许一个为null类型
        /// </summary>
        /// <param name="lpClassName">窗口名</param>
        /// <param name="lpWindowName">窗口类名</param>
        /// <returns></returns>
        public IntPtr MyFindWindow(string lpClassName,string lpWindowName)
        {
            return FindWindow(lpClassName,lpWindowName);
        }
        /// <summary>
        /// 激活指定的窗口
        /// </summary>
        /// <param name="lpClassName">窗口名</param>
        /// <param name="lpWindowName">窗口类名</param>
        /// <returns></returns>
        public bool ActivateWindow(string lpClassName, string lpWindowName)
        {
            IntPtr ptr= FindWindow(lpClassName, lpWindowName);
            //System.Console.WriteLine("当前切换窗口为" + lpWindowName);
            //Console.WriteLine("dfadfasdf窗口指针:     " + (ptr==IntPtr.Zero));
            if (ptr == null||ptr==IntPtr.Zero) return false;
            ShowWindowAsync(ptr, 3);
            SetForegroundWindow(ptr);
            return true;
        }
        

        public void ActivateWindow(int ProcessId, string className)
        {
            WindowInfo[] temp = GetAllDesktopWindows();
            for (int i = 0; i < temp.Length; ++i)
            {

                if (temp[i].szClassName == className)
                {
                    System.Console.WriteLine("当前切换窗口为" + className);
                    ShowWindowAsync(temp[i].hWnd, 3);
                    SetForegroundWindow(temp[i].hWnd);
                    return;
                }

            }
        }
        [DllImport("user32.dll ", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        public void ActivateWindow(Process instance,string windowName)
        {
            
            Process[] processcollection = Process.GetProcessesByName(instance.ProcessName.Replace(".vshost", string.Empty));
            //  该程序已经运行，
            if (processcollection.Length >= 1)
            {
                foreach (Process process in processcollection)
                {
                    if (process.Id != instance.Id)
                    {
                        // 如果进程的句柄为0，即代表没有找到该窗体，即该窗体隐藏的情况时
                        if (process.MainWindowHandle.ToInt32() == 0)
                        {
                            //MessageBox.Show("switch");
                            // 获得窗体句柄
                            IntPtr formhwnd = FindWindow(null, windowName);
                            // 重新显示该窗体并切换到带入到前台
                            ShowWindowAsync(formhwnd, 1);
                            //ShowWindow(formhwnd, 1);
                            SwitchToThisWindow(formhwnd, true);
                        }
                        else
                        {
                            // 如果窗体没有隐藏，就直接切换到该窗体并带入到前台
                            // 因为窗体除了隐藏到托盘，还可以最小化
                            SwitchToThisWindow(process.MainWindowHandle, true);
                        }
                    }
                }
            }


            // 显示窗口
            ShowWindowAsync(instance.MainWindowHandle, 1);
            // 把窗体放在前端
            SetForegroundWindow(instance.MainWindowHandle);
            return;

        }

        public void HideWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, 0);
        }
        public void ShowMyWindow(string windowname)
        {
            IntPtr formhwnd = FindWindow(null, windowname);
            // 重新显示该窗体并切换到带入到前台
            ShowWindowAsync(formhwnd, 1);
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);
        
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);
        //static IntPtr Game;

        //获取当前Window的所有进程集合，windowInfo为全部进程集合
        static WindowInfo[] GetAllDesktopWindows()
        {
            List<WindowInfo> wndList = new List<WindowInfo>();
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);
                //get hwnd
                wnd.hWnd = hWnd;
                //get window name
                GetWindowTextW(hWnd, sb, sb.Capacity);
                wnd.szWindowName = sb.ToString();
                // get processID
                GetWindowThreadProcessId(hWnd, out wnd.ProcessId);
                //get window class
                GetClassNameW(hWnd, sb, sb.Capacity);
                wnd.szClassName = sb.ToString();

                //add it into list
                wndList.Add(wnd);
                return true;
            }, 0);
            return wndList.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SendAndPlayMedia
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
        public void  MoveWindow(Process pro,Screen screen,bool BRePaint)
        {
            new Thread(delegate () {
                int i = 0;
                while (i++ < 10)
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
        public IntPtr MyFindWindow(string lpClassName,string lpWindowName)
        {
            return FindWindow(lpClassName,lpWindowName);
        }
        public void ActivateWindow(string lpClassName, string lpWindowName)
        {
            IntPtr ptr= FindWindow(lpClassName, lpWindowName);
            //System.Console.WriteLine("当前切换窗口为" + lpWindowName);
            ShowWindowAsync(ptr, 3);
            SetForegroundWindow(ptr);
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

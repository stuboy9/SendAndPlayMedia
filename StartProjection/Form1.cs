using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartProjection
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public Form1()
        {
            InitializeComponent();
            Thread thread = new Thread(startProjection);
            thread.Start();
        }

        private void startProjection()
        {
            //打开多屏投影应用
            try
            {
                keybd_event(Keys.LWin, 0, 0, 0);
                keybd_event(Keys.S, 0, 0, 0);
                keybd_event(Keys.S, 0, 2, 0);
                keybd_event(Keys.LWin, 0, 2, 0);
                Thread.Sleep(300);
                SendKeys.SendWait("MyProjection");
                //SendKeys.Send("MyProjection");
                //SendKeys.Send("投影应用");
                Thread.Sleep(500);
                SendKeys.SendWait("{enter}");
                SendKeys.SendWait("{enter}");
                //SendKeys.Send("{enter}");
                //SendKeys.Send("{enter}");

                
            }
            catch (Exception e) { }
            finally
            {
                System.Environment.Exit(0);
            }
            
        }
    }
}

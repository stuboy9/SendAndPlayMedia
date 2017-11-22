using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AreaParty.function.rdp
{
    class RDP
    {
        private string ip;
        private string username;
        private string password;
        public RDP(string ip,string username,string password)
        {
            this.ip = ip;
            this.username = username;
            this.password = password;
        }
        public Process OpenRDP()
        {
            try
            {
                Process rdcProcess = new Process();
                rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe");
                rdcProcess.StartInfo.Arguments = "/generic:TERMSRV/" + ip + " /user:" + username + " / pass:" + password; //+ Program.VM_user + " /pass:" + Program.VM_password; //user: " 
                rdcProcess.Start();

                rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
                rdcProcess.StartInfo.Arguments = "/v " + ip;// 
                rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                rdcProcess.Start();
                ClearRDPPopup();
                return rdcProcess;
            }
            catch (Exception e)
            {
                throw new Exception("远程桌面异常");
            }
        }
        public void CloseRDP()
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process item in ps)
            {
                if (item.ProcessName == "mstsc")
                {
                    item.Kill();
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public void ClearRDPPopup()
        {
            Thread.Sleep(3000);
            keybd_event(Keys.Left, 0, 0, 0);
            keybd_event(Keys.Left, 0, 2, 0);
            keybd_event(Keys.Enter, 0, 0, 0);
            keybd_event(Keys.Enter, 0, 2, 0);
        }
    }
}

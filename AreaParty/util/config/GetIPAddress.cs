using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class GetIPAddress
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetActivatedAdaptorMacAddress()
        {
            UdpClient u = new UdpClient("8.8.8.8", 3325);
            IPAddress localAddr = ((IPEndPoint)u.Client.LocalEndPoint).Address;
            return localAddr.ToString();
            //string mac = "";
            //ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //ManagementObjectCollection moc = mc.GetInstances();
            //foreach (ManagementObject mo in moc)
            //{
            //    if (mo["IPEnabled"].ToString() == "True")
            //    {
            //        string[] macx = (string[])mo["IPAddress"];
            //        mac = macx[0].ToString();
            //        return mac;
            //    }
            //}
            //return mac;
        }
    }
}

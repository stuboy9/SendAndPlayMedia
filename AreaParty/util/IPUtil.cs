using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AreaParty.util
{
    class IPUtil
    {
        public static string GetInternalIP()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection moc = mc.GetInstances();
            string MACAddress = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    return ((String[])mo["IPAddress"])[0];
                }
            }
            return "127.0.0.1";
        }
        public static string GetInternalMAC()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection moc = mc.GetInstances();
            string MACAddress = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    return ((String)mo["MacAddress"]);
                }
            }
            return "";
        }
    }
}

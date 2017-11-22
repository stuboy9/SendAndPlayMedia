using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.function.pcapp
{
    class PCInformation
    {
        public string systemVersion;
        public string systemType;
        public string totalmemory;
        public string cpuName;
        public string totalStorage;
        public string freeStorage;
        public string pcName;
        public string pcDes;
        public string workGroup;
        [JsonIgnore]
        private static PCInformation _instance;
        public static PCInformation GetInstance()
        {
            if (_instance == null) return new PCInformation();
            else return _instance;
        }
        private PCInformation()
        {
            this.systemVersion = GetSystemVersion();
            this.systemType = GetSystemType();
            this.totalmemory = GetTotalMemory();
            this.cpuName = GetCpuName();
            this.totalStorage = GetTotalStorage();
            this.freeStorage = GetFreeStorage();
            this.pcName = GetPCName();
            this.pcDes = GetPCDes();
            this.workGroup = GetWorkGroup();
        }
        private string GetSystemVersion()
        {
            string result = "unknow";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }
        private string GetSystemType()
        {
            if (Environment.Is64BitOperatingSystem) return "64 位操作系统";
            else return "32 位操作系统";
        }

        private string GetTotalMemory()
        {

            return ((Math.Round(Int64.Parse(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory.ToString()) / 1024 / 1024 / 1024.0, 1))).ToString() + "G";
        }

        private string GetCpuName()
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mo in mos.Get())
                {
                    return mo["Name"].ToString();
                }
            }
            catch
            {
                return "unknow";
            }
            return "unknow";

        }
        private string GetTotalStorage()
        {
            long totalSize = new long();
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {

                    totalSize += drive.TotalSize / (1024 * 1024 * 1024);
                }


            }
            return totalSize.ToString() + "G";
        }
        private string GetFreeStorage()
        {
            long freeSpace = new long();
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    freeSpace += drive.TotalFreeSpace / (1024 * 1024 * 1024);
                }

            }
            return freeSpace.ToString() + "G";
        }
        private string GetPCName()
        {
            return Environment.MachineName;
        }
        private string GetPCDes()
        {
            try
            {
                string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lanmanserver\parameters";
                string computerDescription = (string)Registry.GetValue(key, "srvcomment", null);
                return computerDescription;
            }
            catch (Exception e)
            {
                return "unknow";
            }
        }
        private string GetWorkGroup()
        {
            try
            {
                ManagementObjectSearcher mosComputer = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject moComputer in mosComputer.Get())
                {
                    if (moComputer["Workgroup"] != null)
                        return moComputer["Workgroup"].ToString();
                }
                return "unknow";
            }
            catch (Exception e)
            {
                return "unknow";
            }
        }

    }
}

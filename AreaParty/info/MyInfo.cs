using AreaParty.info.phone;
using AreaParty.info.tv;
using AreaParty.util.config;
using AreaParty.util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AreaParty.info
{
    class MyInfo
    {
        public static object locker = new object();
        public static TVLibrary tvLibrary = new TVLibrary() { name = "tv", value = new List<TVInfo>() };
        public static List<BroadParam> phone = new List<BroadParam>();
        public static List<BroadParam> tvs = new List<BroadParam>();
        public static string iconFolder = GetIconFolder();
        public static string launch_time_id = GetLaunch_time_id();
        public static string myIp = IPUtil.GetInternalIP();
        public static string myMAC = IPUtil.GetInternalMAC();
        public static bool IsSreenLock = false;
        public static string execonfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\AreaParty\AreaParty.exe.Config";
        public static string myDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\AreaParty";
        public static string GetIconFolder()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Areaparty\\icon";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //return System.Windows.Forms.Application.StartupPath + "\\" + ConfigUtil.GetValue("iconfolder");
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\" + ConfigUtil.GetValue("iconfolder");
        }
        public static string GetLaunch_time_id()
        {
            if (launch_time_id == null) return Guid.NewGuid().ToString();
            else return launch_time_id;
        }
    }
}

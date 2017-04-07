using SendAndPlayMedia.info.tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendAndPlayMedia.info
{
    class MyInfo
    {
        public static object locker = new object();
        public static TVLibrary tvLibrary = new TVLibrary() { name = "tv", value = new List<TVInfo>() };
        public static List<BroadParam> phone = new List<BroadParam>();
        public static List<BroadParam> tvs = new List<BroadParam>();
        public static string launch_time_id = GetLaunch_time_id();
        public static string GetLaunch_time_id()
        {
            if (launch_time_id == null) return Guid.NewGuid().ToString();
            else return launch_time_id;
        }
    }
}

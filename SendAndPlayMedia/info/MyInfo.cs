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
    }
}

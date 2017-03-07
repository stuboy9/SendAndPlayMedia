using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.info;

namespace SendAndPlayMedia.info.tv
{
    class TVInfo
    {
        public string uuid { set; get; }
        public string name { set; get; }
        public string type { set; get; }
        public string ip { set; get; }
        public Boolean dlnaOk { set; get; }
        public Boolean miracastOk { set; get; }
        public Boolean rdpOk { set; get; }
        public TVInfo(string uuid,string name,string type,string ip, Boolean dlnaOk, Boolean miracastOk, Boolean rdpOk)
        {
            this.uuid = uuid;
            this.name = name;
            this.type = type;
            this.ip = ip;
            this.dlnaOk = dlnaOk;
            this.miracastOk = miracastOk;
            this.rdpOk = rdpOk;
        }
    }
    class TVLibrary :Info
    {
        public string name { set; get; }
        public List<TVInfo> value { set; get; }
    }
}

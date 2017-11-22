using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.tv
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
        [JsonIgnore]
        public int timeStamp = -1;
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

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(this.GetType())) == false)
            {
                return false;
            }
            TVInfo temp = null;
            temp = (TVInfo)obj;

            return this.name.Equals(temp.name) && this.ip.Equals(temp.ip)&&this.uuid.Equals(temp.uuid);

        }

        //重写GetHashCode方法（重写Equals方法必须重写GetHashCode方法，否则发生警告

        public override int GetHashCode()
        {
            return this.name.GetHashCode() + this.ip.GetHashCode()+this.uuid.GetHashCode();
        }

    }
    class TVLibrary :Info
    {
        public string name { set; get; }
        public List<TVInfo> value { set; get; }
    }
}

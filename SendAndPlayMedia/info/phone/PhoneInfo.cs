using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendAndPlayMedia.info
{
    class PhoneInfo
    {
        public string source { set; get; }
        public string type { set; get; }
        public  List<PhoneParam> param{set;get;}
        public PhoneInfo(List<PhoneParam> param,string source="PC_Y",string type="DEFAULT")
        {
            this.source = source;
            this.type = type;
            this.param = param;
        }
        public Boolean equals(PhoneInfo p)
        {
            if (this.param.Count == p.param.Count)
            {
                if (this.source == p.source && this.type == p.type && this.param.First().ip== p.param.First().ip && this.param.First().port == p.param.First().port) return true;
            }
            return false;
        }
    }
    class PhoneParam
    {
        public string ip { set; get; }
        public int port { set; get; }
        public string function { set; get; }
    }
}

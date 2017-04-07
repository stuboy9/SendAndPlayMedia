using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendAndPlayMedia.info
{
    /// <summary>
    /// 广播交互格式
    /// </summary>
    class BroadInfo
    {
        public string source { set; get; }
        public string type { set; get; }
        public  List<BroadParam> param{set;get;}
        public BroadInfo(List<BroadParam> param,string source="PC_Y",string type="DEFAULT")
        {
            this.source = source;
            this.type = type;
            this.param = param;
        }
        public Boolean equals(BroadInfo p)
        {
            if (this.param.Count == p.param.Count)
            {
                if (this.source == p.source && this.type == p.type && this.param.First().ip== p.param.First().ip && this.param.First().port == p.param.First().port) return true;
            }
            return false;
        }
    }
    class BroadParam
    {
        public string ip { set; get; }
        public int port { set; get; }
        public string function { set; get; }
        public string launch_time_id { set; get; }
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
            BroadParam temp = null;
            temp = (BroadParam)obj;

            return this.ip.Equals(temp.ip) && this.port.Equals(temp.port) ;

        }

        //重写GetHashCode方法（重写Equals方法必须重写GetHashCode方法，否则发生警告

        public override int GetHashCode()
        {
            return this.ip.GetHashCode() +  this.port.GetHashCode();
        }
        
    }
}

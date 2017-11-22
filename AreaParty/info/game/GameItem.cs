using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.game
{
    class GameItem
    {
        public string appName { set; get; }//名字
        public string packageName { set; get; }//路径
        public string iconURL = null;
        public GameItem(string name, string path)
        {
            //this.name = name;
            //this.path = path;
            this.appName = name;
            this.packageName = path;
            this.iconURL = "http://" + info.MyInfo.myIp + ":" + 8634 + "/" + System.Web.HttpUtility.UrlEncode(this.appName) + ".png"; ;
        }
    }
}

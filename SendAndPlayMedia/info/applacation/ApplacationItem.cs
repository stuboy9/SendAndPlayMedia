using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.applacation
{
    class ApplacationItem
    {
        //public string path { get; set; }
       // public string name { get; set; }
        public string appName {set;get;}
        public string packageName { set; get; }//路径
        public string location = "pc";
        public ApplacationItem(string name,string path)
        {
            //this.name = name;
            //this.path = path;
            this.appName = name;
            this.packageName = path;
        }
    }
}

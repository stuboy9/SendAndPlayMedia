using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.applacation
{
    class AppLibrary :Info
    {
        public string name { set; get; }
        public List<ApplacationItem> value { set; get; }
        public AppLibrary(List<ApplacationItem> value)
        {
            this.name = "applacation";
            this.value = value;
        }
    }
}

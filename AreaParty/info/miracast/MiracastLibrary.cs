using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.miracast
{
    class MiracastLibrary:Info
    {
        public string name { set; get; }
        public List<Screen> value { set; get; }
        public MiracastLibrary(List<Screen> value)
        {
            this.name = "miracast";
            this.value = value;
        }
    }
}

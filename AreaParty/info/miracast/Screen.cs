using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.miracast
{
    class Screen
    {
        public string name { set; get; }
        public int  index { set; get; }
        public Screen(string name,int index)
        {
            this.name = name;
            this.index = index;
        }
    }
}

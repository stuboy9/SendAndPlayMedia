using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.command
{
    class Command
    {
        public string name { set; get; }
        public string command { set; get; }
        public Dictionary<string, string> param { set; get; }

    }
}

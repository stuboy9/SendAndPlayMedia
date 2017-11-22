using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.tv
{
    class TVCommand
    {
        public string firstcommand;
        public string secondcommand;
        public bool thirdcommand;
        public string fourthcommand;
        public string fifthcommand;
        public List<string> sixthcommand;
        public string sevencommand;
        public static TVCommand GetInstance(string first, string sen, bool third, string fourthcommand, string fifthcommand, List<string> sixthcommand, string sevencommand)
        {
            TVCommand c = new TVCommand();
            c.firstcommand = first;
            c.secondcommand = sen;
            c.thirdcommand = third;
            c.fourthcommand = fourthcommand;
            c.fifthcommand = fifthcommand;
            c.sixthcommand = sixthcommand;
            c.sevencommand = sevencommand;
            return c;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.command
{
    class CommandFactory
    {
        public static Command GetLoginCommand(string name,string password)
        {


            Dictionary<string, string> d = new Dictionary<string, string>();
            Command c = new Command("JC", "login",d);
            c.param.Add("name", name);
            c.param.Add("password", password);
            return c;
        }
        public static Command GetLoginOutCommand()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            Command c = new Command("JC", "logout", d);
            return c;
        }
    }
}

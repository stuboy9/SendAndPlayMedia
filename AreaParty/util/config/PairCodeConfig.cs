using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class PairCodeConfig
    {
        public static String GetPairCode()
        {
            return ConfigUtil.GetValue("PairCode");
        }
        public static bool SetPairCode(string value)
        {
            return ConfigUtil.SetValue("PairCode", value);
        }
    }
}

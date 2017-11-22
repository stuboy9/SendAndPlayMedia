
using AreaParty.info.tv;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class ConfigUtil
    {
        public static String GetValue(String key)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings[key].Value;
            return name;
        }
        public static bool SetValue(string key,string value)
        {
            try
            {
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[key].Value = value;
                config.Save();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}

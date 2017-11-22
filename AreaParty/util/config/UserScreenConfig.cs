using AreaParty.info.tv;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class UserScreenConfig
    {
        public static bool IsExistsScreen(string name)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mySection = config.GetSection("Screen") as MySection;
            foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
            {
                if (add.Name.Equals(name)) return true;
            }
            return false;
        }
        public static DeviceItem GetScreen(string name)
        {
            DeviceItem screen = null;
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("Screen") as MySection;
                foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.Name.Equals(name))
                    {
                        var t = mySection.KeyValues[name];
                        screen = new DeviceItem(null, name, null, null, true, true, true, t.Dlna, t.Miracast);

                        return screen;
                    }
                }
                return null;
            }
            catch (Exception e) { return null; }
        }
        public static List<DeviceItem> GetScreen()
        {
            List<DeviceItem> list = new List<DeviceItem>();
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("Screen") as MySection;
                foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
                {

                    DeviceItem d = new DeviceItem(null, add.Name, null, null, true, true, true, add.Dlna, add.Miracast);
                    list.Add(d);
                }
                return list;
            }
            catch (Exception e) { return list; }
        }
        public static bool AddScreen(string name, string dlna, string miracast)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("Screen") as MySection;
                foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.Name.Equals(name))
                    {
                        var t = mySection.KeyValues[name];
                        t.Dlna = dlna;
                        t.Miracast = miracast;
                        config.Save();
                        ConfigurationManager.RefreshSection("Screen");  //刷新
                        return true;
                    }
                }
                mySection.KeyValues.Add(new MySection.MyKeyValueSetting() { Name = name, Dlna = dlna, Miracast = miracast });

                config.Save();
                ConfigurationManager.RefreshSection("Screen");  //刷新
                return true;
            }
            catch (Exception e) { return false; }

        }
        public static bool RemoveScreen(string name)
        {

            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("Screen") as MySection;
                foreach (MySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.Name.Equals(name))
                    {
                        mySection.KeyValues.Remove(name);
                        config.Save();
                        ConfigurationManager.RefreshSection("Screen");  //刷新
                        return true;
                    }
                }
                return true;
            }
            catch (Exception e) { return false; }
        }
    }
}

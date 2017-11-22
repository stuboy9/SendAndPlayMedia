using AreaParty.info.applacation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class ApplicationConfig
    {
        public static bool IsExistsApp(string name)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mySection = config.GetSection("AppInfo") as AppMySection;
            foreach (AppMySection.MyKeyValueSetting add in mySection.KeyValues)
            {
                if (add.AppName.Equals(name)) return true;
            }
            return false;
        }
        public static bool AddApp(string name, string packageName)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("AppInfo") as AppMySection;
                foreach (AppMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.AppName.Equals(name))
                    {
                        var t = mySection.KeyValues[name];
                        t.PackageName = packageName;
                        config.Save();
                        ConfigurationManager.RefreshSection("AppInfo");  //刷新
                        return true;
                    }
                }
                mySection.KeyValues.Add(new AppMySection.MyKeyValueSetting() { AppName = name, PackageName = packageName });

                config.Save();
                ConfigurationManager.RefreshSection("AppInfo");  //刷新
                return true;
            }
            catch (Exception e) { return false; }

        }
        public static bool RemoveApp(string name)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("AppInfo") as AppMySection;
                foreach (AppMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.AppName.Equals(name))
                    {
                        mySection.KeyValues.Remove(name);
                        config.Save();
                        ConfigurationManager.RefreshSection("AppInfo");  //刷新
                        return true;
                    }
                }
                return true;
            }
            catch (Exception e) { return false; }
        }
        public static List<ApplacationItem> GetAllApp()
        {
            List<ApplacationItem> list = new List<ApplacationItem>();
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("AppInfo") as AppMySection;
                foreach (AppMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    ApplacationItem item = new ApplacationItem(add.AppName, add.PackageName);
                    list.Add(item);
                }
                return list;
            }
            catch (Exception e) { return list; }
        }
    }
}

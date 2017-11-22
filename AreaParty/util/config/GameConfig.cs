using AreaParty.info.game;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util.config
{
    class GameConfig
    {
        public static bool IsExistsGame(string name)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mySection = config.GetSection("GameInfo") as GameMySection;
            foreach (GameMySection.MyKeyValueSetting add in mySection.KeyValues)
            {
                if (add.Name.Equals(name)) return true;
            }
            return false;
        }
        public static bool AddGame(string name, string path)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("GameInfo") as GameMySection;
                foreach (GameMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.Name.Equals(name))
                    {
                        var t = mySection.KeyValues[name];
                        t.Path = name;
                        config.Save();
                        ConfigurationManager.RefreshSection("GameInfo");  //刷新
                        return true;
                    }
                }
                mySection.KeyValues.Add(new GameMySection.MyKeyValueSetting() {Name = name, Path = path });

                config.Save();
                ConfigurationManager.RefreshSection("GameInfo");  //刷新
                return true;
            }
            catch (Exception e) { return false; }

        }
        public static bool RemoveGame(string name)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("GameInfo") as GameMySection;
                foreach (GameMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    if (add.Name.Equals(name))
                    {
                        mySection.KeyValues.Remove(name);
                        config.Save();
                        ConfigurationManager.RefreshSection("GameInfo");  //刷新
                        return true;
                    }
                }
                return true;
            }
            catch (Exception e) { return false; }
        }
        public static List<GameItem> GetAllGame()
        {
            List<GameItem> list = new List<GameItem>();
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mySection = config.GetSection("GameInfo") as GameMySection;
                foreach (GameMySection.MyKeyValueSetting add in mySection.KeyValues)
                {
                    GameItem item = new GameItem(add.Name, add.Path);
                    list.Add(item);
                }
                return list;
            }
            catch (Exception e) { return list; }
        }
    }
}

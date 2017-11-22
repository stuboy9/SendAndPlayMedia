using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AreaParty.util.config
{
    class MediaConfig
    {
        public static List<string> GetMediaLibrary()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserMediaConfig"].Value;
            if (string.IsNullOrEmpty(name)) return new List<String>();
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<String>(name.Split(';'));
        }
        public static void AddMediaLibrary(string mediaPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserMediaConfig"].Value;
            name += mediaPath + ";";
            config.AppSettings.Settings["UserMediaConfig"].Value = name;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void RemoveMediaLibrary(string mediaPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserMediaConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(mediaPath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["UserMediaConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }





        




        public static List<string> GetImageLibrary()
        {
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = Application.StartupPath + "\\" + config.AppSettings.Settings["ImageConfig"].Value;
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<string>( name.Split(';'));
        }

        public static void AddImageLibrary(string imagePath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["ImageConfig"].Value;
            name += imagePath + ";";
            config.AppSettings.Settings["ImageConfig"].Value = name;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void RemoveImageLibrary(string imagePath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["ImageConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(imagePath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["ImageConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// 获取视频库路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetVideoLibrary()
        {
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = Application.StartupPath + "\\" + config.AppSettings.Settings["VideoConfig"].Value;
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<string>(name.Split(';'));
        }
        public static List<string> GetDownLoadLibrary()
        {
            //获取Configuration对象
            
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Areaparty\\下载文件";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //DirectoryInfo Folder = new DirectoryInfo(path);
            List<string> list = new List<string>();
            list.Add(path);
            //foreach (DirectoryInfo file in Folder.GetDirectories())
            //{
            //    Console.WriteLine("file fullname is:{0}", file.FullName);
            //    list.Add(file.FullName);
            //}
            //根据Key读取<add>元素的Value
            //string name = path/*  +"\\"+ config.AppSettings.Settings["VideoConfig"].Value*/;
            //name = name.Substring(0, name.LastIndexOf(";"));
            return list;
        }

        public static void AddVideoLibrary(string videoPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["VideoConfig"].Value;
            name += videoPath + ";";
            config.AppSettings.Settings["VideoConfig"].Value = name;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void RemoveVideoLibrary(string videoPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["VideoConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(videoPath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["VideoConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }




        /// <summary>
        /// 获取音频库路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAudioLibrary()
        {
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = Application.StartupPath + "\\" + config.AppSettings.Settings["AudioConfig"].Value;
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<string>(name.Split(';'));
        }

        public static void AddAudioLibrary(string audioPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["AudioConfig"].Value;
            name += audioPath + ";";
            config.AppSettings.Settings["AudioConfig"].Value = name;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void RemoveAudioLibrary(string audioPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["AudioConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(audioPath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["AudioConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }



        /// <summary>
        /// 获取用户自定义视频库路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMyVideoLibrary()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //string name = config.AppSettings.Settings["UserVideoConfig"].Value;
            //config.AppSettings.Settings["UserVideoConfig"].Value = "";
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");
            //string del = "G:\\迅雷下载;";
            //Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserVideoConfig"].Value;
            //name.Replace(del, "").Trim();
            if (string.IsNullOrEmpty(name)) return new List<String>();
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<String>(name.Split(';'));
            
        }

        public static void AddMyVideoLibrary(string videoPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserVideoConfig"].Value;
            int index = name.IndexOf(videoPath);
            if (index > -1)
            {
                return;
            }
            else
            {
                name += videoPath + ";";
                config.AppSettings.Settings["UserVideoConfig"].Value = name;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        public static void RemoveMyVideoLibrary(string videoPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserVideoConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(videoPath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["UserVideoConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }




        /// <summary>
        /// 获取音频库路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMyAudioLibrary()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserAudioConfig"].Value;
            if (string.IsNullOrEmpty(name)) return new List<String>();
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<String>(name.Split(';'));
           
        }

        public static void AddMyAudioLibrary(string audioPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserAudioConfig"].Value;
            int index = name.IndexOf(audioPath);
            if (index > -1)
            {
                return;
            }
            else
            {
                name += audioPath + ";";
                config.AppSettings.Settings["UserAudioConfig"].Value = name;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        public static void RemoveMyAudioLibrary(string audioPath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserAudioConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(audioPath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["UserAudioConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


        public static List<string> GetMyImageLibrary()
        {

            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserImageConfig"].Value;
            if (string.IsNullOrEmpty(name)) return new List<String>();
            name = name.Substring(0, name.LastIndexOf(";"));
            return new List<String>(name.Split(';'));
            
        }

        public static void AddMyImageLibrary(string imagePath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserImageConfig"].Value;
            int index = name.IndexOf(imagePath);
            if (index > -1)
            {
                return;
            }
            else
            {
                name += imagePath + ";";
                config.AppSettings.Settings["UserImageConfig"].Value = name;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        public static void RemoveMyImageLibrary(string imagePath)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["UserImageConfig"].Value;
            if (String.IsNullOrEmpty(name)) return;
            name = name.Substring(0, name.LastIndexOf(";"));
            string[] libraries = name.Split(';');
            string temp = "";
            for (int i = 0; i < libraries.Length; i++)
            {
                if (!libraries[i].Equals(imagePath)) temp += libraries[i] + ";";
            }
            config.AppSettings.Settings["UserImageConfig"].Value = temp;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            
        }

    }
}

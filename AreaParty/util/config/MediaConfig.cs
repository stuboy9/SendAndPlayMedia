using AreaParty.function.media;
using AreaParty.info.media;
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
            //MediaFunction.ImageFolderPath.Add(imagePath);
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
            //MediaFunction.ImageFolderPath.Remove(imagePath);
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
            
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Areaparty\\下载文件";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            List<string> list = new List<string>();
            list.Add(path);
           
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
            //MediaFunction.VideoFolderPath.Add(videoPath);
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
            //MediaFunction.VideoFolderPath.Remove(videoPath);
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
            //MediaFunction.AudioFolderPath.Add(audioPath);
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
            //MediaFunction.AudioFolderPath.Remove(audioPath);
        }



        /// <summary>
        /// 获取用户自定义视频库路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMyVideoLibrary()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
        /// <summary>
        /// 在指定文件夹下搜索指定文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="foldname"></param>
        /// <returns></returns>
        public static List<MediaMengMeng> SearchMedia(string dir,string foldname)
        {
            List<MediaMengMeng> list = null;
            DirectoryInfo d = new DirectoryInfo(dir);
            FileSystemInfo[] fsinfos = d.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fsinfos)
            {
                if (fsinfo is DirectoryInfo)     //判断是否为文件夹  
                {
                    SearchMedia(fsinfo.FullName,foldname);//递归调用  
                }
                else
                {
                    if (fsinfo.FullName.IndexOf(foldname) > -1)
                    {
                        list.Add(new MediaMengMeng(new VideoItem(fsinfo.FullName)));
                    }
                }
            }
            return list;
        }

        public static List<MediaMengMeng> SearchAll(string key)
        {
            List<string> videolist = util.config.MediaConfig.GetVideoLibrary();
            List<string> audiolist = util.config.MediaConfig.GetAudioLibrary();
            List<string> imagelist = util.config.MediaConfig.GetImageLibrary();
            List<string> downloadlist = util.config.MediaConfig.GetDownLoadLibrary();
            List<string> myvideolist = util.config.MediaConfig.GetMyVideoLibrary();
            List<string> myaudiolist = util.config.MediaConfig.GetMyAudioLibrary();
            List<string> myimagelist = util.config.MediaConfig.GetMyImageLibrary();
            List<MediaMengMeng> f_v_media = null;
            List<MediaMengMeng> f_a_media = null;
            List<MediaMengMeng> f_i_media = null;
            foreach (string s in videolist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_v_media.AddRange(findlist);
            }
            foreach (string s in downloadlist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_v_media.AddRange(findlist);

            }
            foreach (string s in myvideolist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_v_media.AddRange(findlist);

            }
            foreach (string s in audiolist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_a_media.AddRange(findlist);

            }
            foreach (string s in myaudiolist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_a_media.AddRange(findlist);

            }
            foreach (string s in imagelist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_i_media.AddRange(findlist);

            }
            foreach (string s in myimagelist)
            {
                List<MediaMengMeng> findlist = util.config.MediaConfig.SearchMedia(s, key);
                f_i_media.AddRange(findlist);
            }
            List<MediaMengMeng> totalmedia = null;
            totalmedia.AddRange(f_v_media);
            totalmedia.AddRange(f_a_media);
            totalmedia.AddRange(f_i_media);
            return totalmedia;
        }


    }
}

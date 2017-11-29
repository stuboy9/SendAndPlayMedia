using AreaParty.info.media;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AreaParty.function.media
{

    /// <summary>
    /// 此类处理和媒体一切相关功能，核心功能有：初始化时提取缩略图，并将缩略图添加到http服务器；获取制定路径下制定类型媒体文件；打开制定路径的媒体文件并使播放器最大化。
    /// </summary>
    class MediaFunction
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        


        private static List<MediaMengMeng> recentAudioList = null;//音频最近播放列表
        private static List<MediaMengMeng> recentVideoList = null;//视频最近播放列表
        private static List<MediaMengMeng> recentImageList = null;//图片最近播放列表

        private static string MediaThumbnailTempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\MediaLibraryTemp";//缩略图存放文件夹
        private static string AudioSetPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\AudioSet.playlist";//音频集合路径
        private static string VideoSetPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\VideoSet.playlist";//视频集合路径
        private static string ImageSetPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\ImageSet.playlist";//图片集合路径

        private static string AudioRecentListPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\RecentAudioList.playlist";//音频最近播放列表路径
        private static string VideoRecentListPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\RecentVideoList.playlist";//视频最近播放列表路径
        private static string ImageRecentListPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\RecentImageList.playlist";//图片最近播放列表路径

        private static Dictionary<string, List<MediaMengMeng>> AudioSet = null;//音频集列表
        private static Dictionary<string, List<MediaMengMeng>> ImageSet = null;//图片集列表

        public static List<string> VideoFolderPath = null;
        public static List<string> AudioFolderPath = null;
        public static List<string> ImageFolderPath = null;
        
        /// <summary>
        /// 初始化，提取视频文件的缩略图，并将缩略图的路径添加到http服务器。
        /// </summary>
        public void init()
        {

            List<String> s = util.config.MediaConfig.GetMyVideoLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("用户视频媒体库{0}添加成功", library);
                GetThumbnail(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }

            s = util.config.MediaConfig.GetMyAudioLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("用户音频媒体库{0}添加成功", library);
                GetThumbnail(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }
            s = util.config.MediaConfig.GetMyImageLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("用户图片媒体库{0}添加成功", library);
                GetThumbnail(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }

            s = util.config.MediaConfig.GetDownLoadLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("BT视频库{0}添加成功", library);
                GetThumbnail(library);
                //util.JAVAUtil.AddSourceToHTTP(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }

            s = util.config.MediaConfig.GetVideoLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("默认视频库{0}添加成功", library);
                //VideoFolderPath.Add(library);
                GetThumbnail(library);
                //util.JAVAUtil.AddSourceToHTTP(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }
            s = util.config.MediaConfig.GetAudioLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("默认音频库{0}添加成功", library);
                //AudioFolderPath.Add(library);
                //GetThumbnail(library);
                //util.JAVAUtil.AddSourceToHTTP(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }
            s = util.config.MediaConfig.GetImageLibrary();
            foreach (string library in s)
            {
                log.InfoFormat("默认图片库{0}添加成功", library);
                //ImageFolderPath.Add(library);
                GetThumbnail(library);
                //util.JAVAUtil.AddSourceToHTTP(library);
                util.JAVAUtil.AddAlltoHttp(library);
            }
            util.JAVAUtil.AddSourceToHTTP(GetLibraryTemp());
            util.JAVAUtil.AddSourceToHTTP(info.MyInfo.iconFolder);
            
        }

        public List<MediaMengMeng> GetRecentAudioList()
        {
            if (recentAudioList == null)
            {
                if (!File.Exists(AudioRecentListPath))
                {
                    File.Create(AudioRecentListPath);
                    recentAudioList = new List<MediaMengMeng>();
                }
                else
                {
                    try
                    {
                        StreamReader sr = new StreamReader(AudioRecentListPath);
                        string s = sr.ReadToEnd();
                        recentAudioList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(s);
                        if (recentAudioList == null) recentAudioList = new List<MediaMengMeng>();
                        sr.Close();
                    }
                    catch (JsonException e)
                    {
                        log.Error("最近音频播放列表json反序列化失败",e);
                        recentAudioList = new List<MediaMengMeng>();
                    }
                }
            }

            return recentAudioList;

        }

        public List<MediaMengMeng> GetRecentVideoList()
        {
            if (recentVideoList == null)
            {
                if (!File.Exists(VideoRecentListPath))
                {
                    File.Create(VideoRecentListPath);
                    recentVideoList = new List<MediaMengMeng>();
                }
                else
                {
                    try
                    {
                        StreamReader sr = new StreamReader(VideoRecentListPath);
                        string s = sr.ReadToEnd();
                        recentVideoList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(s);
                        if (recentVideoList == null) recentVideoList = new List<MediaMengMeng>();
                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        log.Error("最近播放视频列表json反序列化失败", e);
                        recentVideoList = new List<MediaMengMeng>();
                    }

                }

            }
            return recentVideoList;

        }

        public List<MediaMengMeng> GetRecentImageList()
        {
            if (recentImageList == null)
            {
                if (!File.Exists(ImageRecentListPath))
                {
                    File.Create(ImageRecentListPath);
                    recentImageList = new List<MediaMengMeng>();
                }
                else
                {

                    try
                    {
                        StreamReader sr = new StreamReader(ImageRecentListPath);
                        recentImageList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(sr.ReadToEnd());
                        if (recentImageList == null) recentImageList = new List<MediaMengMeng>();
                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        log.Error("最近播放图片列表json反序列化失败", e);
                        recentImageList = new List<MediaMengMeng>();
                    }
                }

            }
            return recentImageList;

        }

        public void AddRecentMeiaList(MediaItem item)
        {
            if (item.GetMediaType().Equals("audio")) AddRecentAudioList(new MediaMengMeng(new AudioItem(item.pathName)));
            else if (item.GetMediaType().Equals("video")) AddRecentVedioList(new MediaMengMeng(new VideoItem(item.pathName)));
            else if (item.GetMediaType().Equals("image")) AddRecentImageList(new MediaMengMeng(new ImageItem(item.pathName)));
        }

        public void AddRecentAudioList(MediaMengMeng item)
        {
            if (recentAudioList == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(AudioRecentListPath);
                    recentAudioList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    recentAudioList = new List<MediaMengMeng>();
                }
            }
            if (recentAudioList.Exists(a => a.name == item.name && a.pathName == item.pathName))
            {
                MediaMengMeng index = recentAudioList.Find(a => a.name == item.name && a.pathName == item.pathName);
                recentAudioList.Remove(index);
                recentAudioList.Add(index);
            }
            else
            {
                if (recentAudioList.Count < 10)
                {
                    recentAudioList.Add(item);
                }
                else
                {
                    recentAudioList.RemoveAt(0);
                    recentAudioList.Add(item);
                }
            }
            UpDateRecentAudioList();
        }
        public void UpDateRecentAudioList()
        {
            try
            {
                StreamWriter sw = new StreamWriter(AudioRecentListPath, false);
                sw.Write(JsonConvert.SerializeObject(recentAudioList));
                sw.Close();
            }
            catch (Exception e)
            {
                log.Error("写入最近音频播放列表失败", e);
            }

        }

        public void AddRecentVedioList(MediaMengMeng item)
        {
            if (recentVideoList == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(VideoRecentListPath);
                    recentVideoList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    recentVideoList = new List<MediaMengMeng>();
                }
            }
            if (recentVideoList.Exists(a => a.name == item.name && a.pathName == item.pathName))
            {
                MediaMengMeng index = recentVideoList.Find(a => a.name == item.name && a.pathName == item.pathName);
                recentVideoList.Remove(index);
                recentVideoList.Add(index);
            }
            else
            {
                if (recentVideoList.Count < 10)
                {
                    recentVideoList.Add(item);
                }
                else
                {
                    recentVideoList.RemoveAt(0);
                    recentVideoList.Add(item);
                }
            }
            UpDateRecentVideoList();
        }
        public void UpDateRecentVideoList()
        {
            try
            {

                StreamWriter sw = new StreamWriter(VideoRecentListPath, false);
                sw.Write(JsonConvert.SerializeObject(recentVideoList));
                sw.Close();
            }
            catch (Exception e)
            {
                log.Error("写入最近视频播放列表失败", e);
            }

        }

        public void AddRecentImageList(MediaMengMeng item)
        {
            if (recentImageList == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(ImageRecentListPath);
                    recentImageList = JsonConvert.DeserializeObject<List<MediaMengMeng>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    recentImageList = new List<MediaMengMeng>();
                }
            }
            if (recentImageList.Exists(a => a.name == item.name && a.pathName == item.pathName))
            {
                MediaMengMeng index = recentImageList.Find(a => a.name == item.name && a.pathName == item.pathName);
                recentImageList.Remove(index);
                recentImageList.Add(index);
            }
            else
            {
                if (recentImageList.Count < 10)
                {
                    recentImageList.Add(item);
                }
                else
                {
                    recentImageList.RemoveAt(0);
                    recentImageList.Add(item);
                }
            }
            UpDateRecentImageList();
        }
        public void UpDateRecentImageList()
        {
            try
            {
                StreamWriter sw = new StreamWriter(ImageRecentListPath, false);
                sw.Write(JsonConvert.SerializeObject(recentImageList));
                sw.Close();
            }
            catch (Exception e)
            {
                log.Error("写入最近图片播放列表失败", e);
            }

        }

        /// <summary>
        /// 获取媒体文件
        /// </summary>
        /// <param name="type">可以使video、audio、和image</param>
        /// <param name="folder">获取根目录时为root，其他时候为绝对路径</param>
        /// <returns></returns>
        public List<MediaMengMeng> getMediasByPath(string type, string folder)
        {
            string currDir = folder;
            string[] subDirs = null;

            List<MediaMengMeng> vlist = new List<MediaMengMeng>();
            if (folder.Equals("root"))
            {
                List<string> roots = null;
                //List<string> roots = util.config.MediaConfig.GetMyVideoLibrary();
                //foreach (string sub in roots)
                //{
                //    vlist.Add(new MediaMengMeng(sub.Substring(sub.LastIndexOf(@"\") + 1), sub, "FOLDER",null));

                //}


                if (type == "VIDEO")
                {
                    roots = util.config.MediaConfig.GetVideoLibrary();
                    roots.AddRange(util.config.MediaConfig.GetDownLoadLibrary());
                    roots.AddRange(util.config.MediaConfig.GetMyVideoLibrary());
                    
                }
                else if (type == "AUDIO")
                {
                    roots = util.config.MediaConfig.GetAudioLibrary();
                    roots.AddRange(util.config.MediaConfig.GetMyAudioLibrary());
                }
                else if(type == "IMAGE")
                {
                    roots = util.config.MediaConfig.GetImageLibrary();
                    roots.AddRange(util.config.MediaConfig.GetMyImageLibrary());
                }
                foreach (string sub in roots)
                {
                    vlist.Add(new MediaMengMeng(sub.Substring(sub.LastIndexOf(@"\") + 1), sub, "FOLDER",null));
                }
                return vlist;
            }
            try
            {
                subDirs = System.IO.Directory.GetDirectories(currDir);
                foreach (string sub in subDirs)
                {
                    vlist.Add(new MediaMengMeng(sub.Substring(sub.LastIndexOf(@"\") + 1), sub, "FOLDER",null));

                }

            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            string[] files = null;
            try
            {
                files = System.IO.Directory.GetFiles(currDir);
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            string videoRegex = "(\\.mp4|\\.mkv|\\.rmvb|\\.mov|\\.flv|\\.avi|\\.mpg|\\.wmv|\\.mpeg)$";
            string audioRegex = "(\\.mp3|\\.wma)$";
            string pictureRegex = "(\\.jpg|\\.bmp|\\.png|\\.tiff|\\.tif|\\.gif)$";
            foreach (string file in files)
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);

                    Regex r;
                    if (type == "VIDEO")
                    {
                        r = new Regex(videoRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) vlist.Add(new MediaMengMeng(new VideoItem(fi.FullName)));
                    }
                    else if (type == "AUDIO")
                    {
                        r = new Regex(audioRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) vlist.Add(new MediaMengMeng(new AudioItem(fi.FullName)));
                    }
                    else if (type == "IMAGE")
                    {
                        r = new Regex(pictureRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) vlist.Add(new MediaMengMeng(new ImageItem(fi.FullName)));
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    //Console.WriteLine(e.Message);
                    continue;
                }
            }
            return vlist;
        }
        
        public Dictionary<string, List<MediaMengMeng>>  GetAudioSet()
        {
            if (AudioSet == null)
            {
                if (!File.Exists(AudioSetPath))
                {
                    File.Create(AudioSetPath);
                    AudioSet = new Dictionary<string, List<MediaMengMeng>>();
                }
                else
                {
                    try
                    {
                        StreamReader sr = new StreamReader(AudioSetPath);
                        AudioSet = JsonConvert.DeserializeObject<Dictionary<string, List<MediaMengMeng>>> (sr.ReadToEnd());
                        if (AudioSet == null) AudioSet = new Dictionary<string, List<MediaMengMeng>>();
                        sr.Close();
                    }
                    catch (JsonException e)
                    {
                        AudioSet = new Dictionary<string, List<MediaMengMeng>>();
                    }
                }
            }

            return AudioSet;
        }

        public bool AddAudioSet(string name, List<MediaMengMeng> list)
        {
            if (AudioSet == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(AudioSetPath);
                    AudioSet = JsonConvert.DeserializeObject<Dictionary<string,List<MediaMengMeng>>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    AudioSet = new Dictionary<string, List<MediaMengMeng>>();
                    return false;
                }
            }

            if (!AudioSet.ContainsKey(name)) return false;
            else
            {
                List<MediaMengMeng> temp = AudioSet[name];
                foreach(MediaMengMeng item in list)
                {
                    if (temp.Exists(a => a.name.Equals(item.name))) continue;
                    temp.Add(item);
                }
            }

            UpDateAudioSet();
            return true;
        }
        public bool AddAudioSet(string name)
        {
            if (AudioSet == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(Application.StartupPath + "\\AudioSet.playlist");
                    AudioSet = JsonConvert.DeserializeObject<Dictionary<string, List<MediaMengMeng>>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    AudioSet = new Dictionary<string, List<MediaMengMeng>>();
                    return false;
                }
            }

            if (AudioSet.ContainsKey(name)) return false;
            else
            {
                AudioSet.Add(name, new List<MediaMengMeng>());
            }
            UpDateAudioSet();
            return true;
        }
        public bool DeleteAudiSet(string name)
        {
            if (AudioSet == null) return false;
            else
            {
                if (AudioSet.ContainsKey(name))
                {
                    AudioSet.Remove(name);
                    UpDateAudioSet();
                }
                return true;
            }
        }
        public void UpDateAudioSet()
        {
            try
            {
                StreamWriter sw = new StreamWriter(AudioSetPath, false);
                sw.Write(JsonConvert.SerializeObject(AudioSet));
                sw.Close();
            }
            catch (Exception e)
            {

            }

        }

        

        public Dictionary<string, List<MediaMengMeng>> GetImageSet()
        {
            if (ImageSet == null)
            {
                if (!File.Exists(ImageSetPath))
                {
                    File.Create(ImageSetPath);
                    ImageSet = new Dictionary<string, List<MediaMengMeng>>();
                }
                else
                {
                    try
                    {
                        StreamReader sr = new StreamReader(ImageSetPath);
                        ImageSet = JsonConvert.DeserializeObject<Dictionary<string, List<MediaMengMeng>>>(sr.ReadToEnd());
                        if (ImageSet == null) ImageSet = new Dictionary<string, List<MediaMengMeng>>();
                        sr.Close();
                    }
                    catch (JsonException e)
                    {
                        ImageSet = new Dictionary<string, List<MediaMengMeng>>();
                    }
                }
            }

            return ImageSet;
        }

        public bool AddImageSet(string name, List<MediaMengMeng> list)
        {
            if (ImageSet == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(ImageSetPath);
                    ImageSet = JsonConvert.DeserializeObject<Dictionary<string, List<MediaMengMeng>>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    ImageSet = new Dictionary<string, List<MediaMengMeng>>();
                    return false;
                }
            }

            if (!ImageSet.ContainsKey(name)) return false;
            else
            {
                List<MediaMengMeng> temp = ImageSet[name];
                foreach (MediaMengMeng item in list)
                {
                    if (temp.Exists(a => a.name.Equals(item.name))) continue;
                    temp.Add(item);
                }
            }

            UpDateImageSet();
            return true;
        }
        public bool AddImageSet(string name)
        {
            if (ImageSet == null)
            {
                try
                {

                    StreamReader sr = new StreamReader(ImageSetPath);
                    ImageSet = JsonConvert.DeserializeObject<Dictionary<string, List<MediaMengMeng>>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {
                    ImageSet = new Dictionary<string, List<MediaMengMeng>>();
                    return false;
                }
            }

            if (ImageSet.ContainsKey(name)) return false;
            else
            {
                ImageSet.Add(name, new List<MediaMengMeng>());
            }
            UpDateImageSet();
            return true;
        }
        public bool DeleteImageSet(string name)
        {
            if (ImageSet == null) return false;
            else
            {
                if (ImageSet.ContainsKey(name))
                {
                    ImageSet.Remove(name);
                    UpDateImageSet();
                }
                return true;
            }
        }
        public void UpDateImageSet()
        {
            try
            {
                StreamWriter sw = new StreamWriter(ImageSetPath, false);
                sw.Write(JsonConvert.SerializeObject(ImageSet));
                sw.Close();
            }
            catch (Exception e)
            {

            }

        }
        
        
        
        
        /// <summary>
        /// 获取存放缩略图的临时路径
        /// </summary>
        /// <returns></returns>
        public string GetLibraryTemp()
        {
            return MediaThumbnailTempPath;
        }
        
        
        
        /// <summary>
        /// 从指定路径提取视频文件缩略图
        /// </summary>
        /// <param name="sourcePath">视频路径</param>
        /// <param name="dstPath">存放图片路径</param>
        public void GetThumbnail(string sourcePath, string dstPath = @"./MediaLibraryTemp")
        {
            try
            {
                if (dstPath.Equals("./MediaLibraryTemp")) dstPath = MediaThumbnailTempPath;
                if (!System.IO.Directory.Exists(dstPath))
                {
                    System.IO.Directory.CreateDirectory(dstPath);
                }
                string[] dsts = System.IO.Directory.GetFiles(dstPath);
                for (int i = 0; i < dsts.Length; i++)
                {
                    dsts[i] = dsts[i].Substring(dsts[i].LastIndexOf("\\") + 1);
                }

                Stack<string> dirs = new Stack<string>(200);

                if (!System.IO.Directory.Exists(sourcePath))
                {
                    throw new ArgumentException();
                }
                dirs.Push(sourcePath);
                while (dirs.Count > 0)
                {
                    string currDir = dirs.Pop();
                    string[] subDirs;

                    try
                    {
                        subDirs = System.IO.Directory.GetDirectories(currDir);
                    }
                    catch (System.UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    string[] files = null;
                    try
                    {
                        files = System.IO.Directory.GetFiles(currDir);
                    }
                    catch (System.UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    foreach (string file in files)
                    {
                        try
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(file);
                            string videoRegex = "(\\.mp4|\\.mkv|\\.rmvb|\\.mov|\\.flv|\\.avi|\\.mpg|\\.wmv|\\.mpeg)$";
                            //string audioRegex = "(\\.mp3|\\.wma)$";
                            string pictureRegex = "(\\.jpg|\\.bmp|\\.png|\\.tiff|\\.tif|\\.gif)$";

                            Regex r = new Regex(videoRegex, RegexOptions.IgnoreCase);
                            Regex imageR = new Regex(pictureRegex, RegexOptions.IgnoreCase);
                            if (r.IsMatch(fi.Extension))
                            {
                                VideoItem vi = new VideoItem(file);
                                if (dsts.Contains(vi.name + vi.fileSize + ".jpg"))
                                {
                                   // Console.WriteLine(vi);
                                    continue;
                                }
                                vi.GetImageFromVedio(dstPath);
                                //Console.WriteLine(vi);
                            }
                            else if (imageR.IsMatch(fi.Extension))
                            {
                                ImageItem vi = new ImageItem(file);
                                if (dsts.Contains(vi.name + vi.fileSize.Trim() + ".jpg"))
                                {
                                   // Console.WriteLine(vi);
                                    continue;
                                }
                                vi.MakeThumbnail(dstPath + "\\" + vi.name + vi.fileSize.Trim() + ".jpg", 80, 80, "HW");
                                //Console.WriteLine(vi);
                            }


                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }
                    }

                    foreach (string str in subDirs)
                        dirs.Push(str);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
            catch (ArgumentException e)
            {
                return;
            }
        }
        public bool DeletMyMediaLibrary(string type, string path)
        {
            try
            {
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string name = config.AppSettings.Settings[type].Value;
                name = name.Replace(path + ";", "");
                config.AppSettings.Settings[type].Value = name;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        

    }
    /// <summary>
    /// 兼容蒙蒙的mediaItem，重新封装我的MediaItem
    /// </summary>
    class MediaMengMeng
    {
        public string name { set; get; }
        public string pathName { set; get; }
        public string thumbnailurl { set; get; }
        public string type { set; get; }
        public string url { set; get; }
        public MediaMengMeng() { }
        public MediaMengMeng(MediaItem mi)
        {
            this.name = mi.name;
            this.pathName = mi.pathName;
            this.thumbnailurl = mi.thumbnailurl;
            this.url = mi.url;
            if (mi is VideoItem) this.type = "VIDEO";
            else if (mi is ImageItem) this.type = "IMAGE";
            else if (mi is AudioItem) this.type = "AUDIO";
            else this.type = "FOLDER";
        }
        public MediaMengMeng(string name, string pathName, string type , string thumbnailurl )
        {
            this.pathName = pathName;
            this.name = name;
            this.type = type;
            this.thumbnailurl = thumbnailurl;
        }
        
    }

}

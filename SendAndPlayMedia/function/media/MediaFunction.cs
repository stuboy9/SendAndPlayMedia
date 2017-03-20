using Newtonsoft.Json;
using SendAndPlayMedia.command;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.info;
using Test.info.media;

namespace SendAndPlayMedia
{

    /// <summary>
    /// 程序提供以json字符串形式获取制定目录下的媒体库文件，包括音频、视频、图片，同时运行线程每五分钟更新媒体库，提供添加和删除媒体库目录。
    /// </summary>
    class MediaFunction
    {
        private List<string> video = new List<string>();
        private List<string> audio = new List<string>();
        private List<string> picture = new List<string>();

        private string mediaConf = System.IO.Directory.GetCurrentDirectory() + "medias.txt";
        private string medias = null;
        private string[] paths = null;
        public string videoAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
        public string audioAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
        public string pictureAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
        static readonly Object objConf = new Object();//锁对象
        /// <summary>
        /// 返回json格式字符串的媒体库文件列表
        /// </summary>
        /// <returns></returns>
        public string getMedias()
        {
            return "";
        }
        public MediaJson getMediasByPath(string type, string folder)
        {
            string currDir = folder;
            string[] subDirs = null;

            List<MediaMengMeng> vlist = new List<MediaMengMeng>();
            if (folder.Equals("root"))
            {
                subDirs = GetMediaLibrary();
                foreach (string sub in subDirs)
                {
                    vlist.Add(new MediaMengMeng(sub.Substring(sub.LastIndexOf(@"\") + 1), sub,type:type));

                }
                return new MediaJson(vlist);
            }
            try
            {
                subDirs = System.IO.Directory.GetDirectories(currDir);
                foreach (string sub in subDirs)
                {
                    vlist.Add(new MediaMengMeng(sub.Substring(sub.LastIndexOf(@"\") + 1), sub, type: type));

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
                    if (type == "video")
                    {
                        r = new Regex(videoRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) vlist.Add(new MediaMengMeng(new VideoItem(fi.FullName)));
                    }
                    else if (type == "audio")
                    {
                        r = new Regex(audioRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) vlist.Add(new MediaMengMeng(new AudioItem(fi.FullName)));
                    }
                    else if (type == "image")
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

            MediaJson mj = new MediaJson(vlist);
            return mj;
        }
        public void AddPath(string path)
        {


            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["mediaConfig"].Value;
            //写入<add>元素的Value
            config.AppSettings.Settings["name"].Value = name + path + ";";
            //一定要记得保存，写不带参数的config.Save()也可以
            config.Save(ConfigurationSaveMode.Modified);
            //刷新，否则程序读取的还是之前的值（可能已装入内存）
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }
        /// <summary>
        /// 获取配置文件中媒体库文件夹
        /// </summary>
        /// <returns>媒体库文件夹</returns>
        public string[] GetMediaLibrary()
        {
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings["mediaConfig"].Value;
            name = name.Substring(0, name.LastIndexOf(";"));
            return name.Split(';');
        }
        public string GetLibraryTemp()
        {
            return System.Environment.CurrentDirectory + "\\MediaLibraryTemp";
        }
        /// <summary>
        /// 传入文件目录，判断文件类型，使用设置好的软件打开
        /// </summary>
        /// <param name="param">媒体文件类型</param>
        /// <returns></returns>
        public Process openPlayer(string param)
        {
            FileInfo fi = new FileInfo(param);
            string videoRegex = "(\\.mp4|\\.mkv|\\.rmvb|\\.mov|\\.flv|\\.avi|\\.mpg|\\.wmv|\\.mpeg)$";
            string audioRegex = "(\\.mp3|\\.wma)$";
            string pictureRegex = "(\\.jpg|\\.bmp|\\.png|\\.tiff|\\.tif|\\.gif)$";
            string pathAPP = "";

            Regex r = new Regex(videoRegex, RegexOptions.IgnoreCase);
            if (r.IsMatch(fi.Extension)) pathAPP = videoAPP;//Console.WriteLine("视频：{0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
            else if ((r = new Regex(audioRegex, RegexOptions.IgnoreCase)).IsMatch(fi.Extension)) pathAPP = audioAPP;//Console.WriteLine("音频： {0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
            else if ((r = new Regex(pictureRegex, RegexOptions.IgnoreCase)).IsMatch(fi.Extension)) pathAPP = pictureAPP; //Console.WriteLine("图片： {0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
            else return null;
            ProcessStartInfo startInfo = new ProcessStartInfo(pathAPP);
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = param;
            Process p = Process.Start(startInfo);
            //WindowControl wc = new WindowControl();
            ////Screen[] sc = Screen.AllScreens;
            //Thread.Sleep(1000*5);
            ////int result = wc.MyMoveWindow(p.Handle,sc.Last().Bounds.Left, sc.Last().Bounds.Top, sc.Last().Bounds.Width, sc.Last().Bounds.Height, true);
            ////Console.WriteLine(sc[0]);
            ////Console.WriteLine(sc[1]);

            ////wc.MyMoveWindow(wc.MyFindWindow("PotPlayer",null),0,0,800,600,true);
            //Console.WriteLine("THIS:"+p.MainWindowTitle.ToString());
            //Console.WriteLine("THAT:" + wc.MyFindWindow("PotPlayer", null));
            //wc.MyMoveWindow(p.MainWindowHandle, 0, 0, 800, 600, true);
            //wc.MyMoveWindow(p.Handle, 0, 0,800, 600, true);
            //隐藏窗口
            return p;

        }
        public void KillPlayer()
        {
            Console.WriteLine("位置1{0}位置2{1}", videoAPP.LastIndexOf(@"\"), videoAPP.LastIndexOf("."));
            Console.WriteLine(videoAPP.Substring(videoAPP.LastIndexOf(@"\") + 1, videoAPP.LastIndexOf(".") - videoAPP.LastIndexOf(@"\") - 1));
            string videoName = videoAPP.Substring(videoAPP.LastIndexOf(@"\") + 1, videoAPP.LastIndexOf(".") - videoAPP.LastIndexOf(@"\") - 1);
            string audioName = audioAPP.Substring(audioAPP.LastIndexOf(@"\") + 1, audioAPP.LastIndexOf(".") - audioAPP.LastIndexOf(@"\") - 1);
            string pictureName = pictureAPP.Substring(pictureAPP.LastIndexOf(@"\") + 1, pictureAPP.LastIndexOf(".") - pictureAPP.LastIndexOf(@"\") - 1);
            Process[] ps = Process.GetProcesses();
            foreach (Process item in ps)
            {
                if (item.ProcessName == videoName || item.ProcessName == audioName || item.ProcessName == pictureName)
                {
                    item.Kill();
                }
            }
        }
        /// <summary>
        ///  遍历root目录下所有文件，将所有视频，音频，图片存入video，audio和picture;
        /// </summary>
        /// <param name="root">路径</param>
        public void TraverseTree(string root)
        {
            Stack<string> dirs = new Stack<string>(200);

            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);
            Console.WriteLine("root start");
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
                        string audioRegex = "(\\.mp3|\\.wma)$";
                        string pictureRegex = "(\\.jpg|\\.bmp|\\.png|\\.tiff|\\.tif|\\.gif)$";

                        Regex r = new Regex(videoRegex, RegexOptions.IgnoreCase);
                        if (r.IsMatch(fi.Extension)) video.Add(fi.FullName);//Console.WriteLine("视频：{0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
                        else if ((r = new Regex(audioRegex, RegexOptions.IgnoreCase)).IsMatch(fi.Extension)) audio.Add(fi.FullName);//Console.WriteLine("音频： {0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
                        else if ((r = new Regex(pictureRegex, RegexOptions.IgnoreCase)).IsMatch(fi.Extension)) picture.Add(fi.FullName); //Console.WriteLine("图片： {0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);
                        //Console.WriteLine("{0}: {1} {2}", fi.Name, fi.Length, fi.CreationTime);

                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        //Console.WriteLine(e.Message);
                        continue;
                    }
                }

                foreach (string str in subDirs)
                    dirs.Push(str);
            }

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
                                    Console.WriteLine(vi);
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
                                    Console.WriteLine(vi);
                                    continue;
                                }
                                vi.MakeThumbnail(dstPath + "\\" + vi.name + vi.fileSize.Trim() + ".jpg", 80, 80, "HW");
                                //Console.WriteLine(vi);
                            }


                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            //Console.WriteLine(e.Message);
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
        /// <summary>
        /// 添加文件夹资源到http服务器
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        public void AddSourceToHTTP(string dirPath, Boolean isDir = true)
        {
            try
            {
                Dictionary<string, string> param = null;
                Command command = null;
                if (isDir == false)
                {
                    param = new Dictionary<string, string>();
                    param.Add("file", dirPath);
                    command = new Command("PC", "AddFileHTTP", param);
                }
                else
                {
                    param = new Dictionary<string, string>();
                    param.Add("dir", dirPath);
                    command = new Command("PC", "AddDirHTTP", param);
                }
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(new IPEndPoint(ip, 9816));
                Console.WriteLine("连接成功");
                byte[] result = new byte[1024];
                serverSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command)));
                int receiveNumber = serverSocket.Receive(result);
                string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);
                Console.WriteLine(rev);
                serverSocket.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

    }
    class MediaJson : Info
    {
        public List<MediaMengMeng> medias { set; get; }
        public MediaJson(List<MediaMengMeng> medias)
        {
            this.medias = medias;
        }
    }
    /// <summary>
    /// 兼容蒙蒙的mediaItem，重新封装我的MediaItem
    /// </summary>
    class MediaMengMeng
    {
        public string name { set; get; }
        public string pathName { set; get; }
        public string uri { set; get; }
        public string location { set; get; }
        public string thumbnailurl { set; get; }
        public string type { set; get; }
        public Boolean isFolder = false;
        public MediaMengMeng(MediaItem mi)
        {
            this.name = mi.name;
            this.pathName = mi.pathName;
            this.uri = mi.url;
            this.location = mi.location;
            this.thumbnailurl = mi.thumbnailurl;
            if (mi is VideoItem) this.type = "video";
            else if (mi is ImageItem) this.type = "image";
            else if (mi is AudioItem) this.type = "audio";
        }
        public MediaMengMeng(string name, string pathName, string uri = "", string location = "pc", string type = "folder", string thumbnailurl = "",Boolean isFolder=true)
        {
            this.pathName = pathName;
            this.name = name;
            this.uri = uri;
            this.location = location;
            this.type = type;
            this.thumbnailurl = thumbnailurl;
            this.isFolder = isFolder;
        }
    }
}

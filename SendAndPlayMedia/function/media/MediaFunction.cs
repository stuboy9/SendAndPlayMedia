using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static Boolean flag = false;
        private string medias = null;
        private string[] paths = null;
        public string videoAPP { get; set; }
        public string audioAPP { get; set; }
        public string pictureAPP { get; set; }
        static readonly Object objConf = new Object();//锁对象
        /// <summary>
        /// 返回json格式字符串的媒体库文件列表
        /// </summary>
        /// <returns></returns>
        public string getMedias()
        {
            if (medias == null)
            {
                string path = System.IO.Directory.GetCurrentDirectory();
                if (File.Exists(path + "\\medias.txt"))
                {
                    medias = File.ReadAllText(path + "\\medias.txt");
                    return medias;
                }
                else
                {
                    FileStream fs = null;
                    StreamWriter sw = null;
                    string json = null;
                    try
                    {
                        fs = new FileStream(mediaConf, FileMode.Create);
                        sw = new StreamWriter(fs);

                        MediaLibrary mediaLibrary = new MediaLibrary(new Dictionary<string, List<MediaItem>>());

                        mediaLibrary.value.Add("video", new List<MediaItem>());
                        mediaLibrary.value.Add("audio", new List<MediaItem>());
                        mediaLibrary.value.Add("picture", new List<MediaItem>());
                        json = JsonConvert.SerializeObject(mediaLibrary);
                        medias = json;
                        sw.WriteLine(json);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        sw.Close();
                        fs.Close();
                    }
                    
                    //StringWriter sw = new StringWriter();
                    //JsonWriter writer = new JsonTextWriter(sw);
                    //writer.WriteStartObject();
                    //writer.WritePropertyName("mediasLibrary");

                    //writer.WriteStartArray();

                    //writer.WriteStartObject();
                    //writer.WritePropertyName("video");
                    //writer.WriteStartArray();
                    //writer.WriteEndArray();
                    //writer.WriteEndObject();

                    //writer.WriteStartObject();
                    //writer.WritePropertyName("autio");
                    //writer.WriteStartArray();
                    //writer.WriteEndArray();
                    //writer.WriteEndObject();

                    //writer.WriteStartObject();
                    //writer.WritePropertyName("picture");
                    //writer.WriteStartArray();
                    //writer.WriteEndArray();
                    //writer.WriteEndObject();

                    //writer.WriteEndArray();

                    //writer.WriteEndObject();
                    return json;
                }
            }
            else
            {
                return medias;
            }
        }
        /// <summary>
        /// 从配置文件中移除媒体库路径
        /// </summary>
        /// <param name="path">路径</param>
        public void RemovePath(string path)
        {
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            FileStream fs = null;
            if (!File.Exists(currentPath + "\\Configuration.txt"))
            {
                return;
            }
            else
            {
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Append);
            }
        }
        /// <summary>
        /// 向配置文件中添加path媒体库文件路径，如果重复则跳过
        /// </summary>
        /// <param name="path">路径</param>
        public void AddPath(string path)
        {
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            FileStream fs = null;
            if (!File.Exists(currentPath + "\\Configuration.txt"))
            {
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Create);
            }
            else
            {
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (path.Equals(line))
                    {
                        sr.Close();
                        fs.Close();
                        return;
                    }
                }
                sr.Close();
                fs.Close();
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Append);
            }

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(path);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        /// <summary>
        /// 开始线程Run
        /// </summary>
        public void Start()
        {
            if (!flag)
            {
                flag = true;
                new Thread(Run).Start();
            }
            
        }
        /// <summary>
        /// 初始化参数，检测是否存在Configuration.txt，如果存在，从中读取媒体库文件目录，否则写入初始值，并初始化paths
        /// </summary>
        public MediaFunction()
        {
            videoAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
            audioAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
            pictureAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            if (!File.Exists(currentPath + "\\Configuration.txt"))
            {
                string[] drives = Directory.GetLogicalDrives();
                foreach (string drive in drives)
                {
                    if (drive.Equals(@"C:\", StringComparison.CurrentCultureIgnoreCase)) continue;
                    AddPath(drive + "mediasLibrary\\video");
                    AddPath(drive + "mediasLibrary\\music");
                    AddPath(drive + "mediasLibrary\\image");
                }
            }

            string path = "";
            string s = null;
            lock (objConf)
            {
                FileStream fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                
                while ((s = sr.ReadLine()) != null)
                {
                    path += s + ";";
                }
                sr.Close();
                fs.Close();
            }
            paths = path.TrimEnd(';').Split(';');

        }
        /// <summary>
        /// 关闭run线程
        /// </summary>
        public void Close()
        {
            flag = false;
        }
        /// <summary>
        /// 作为一个线程启动，每五分钟遍历一次path中目录，将获取的媒体文件以json格式更新到medias.txt
        /// </summary>
        public void Run()
        {
            Console.WriteLine("线程启动");
            video.Clear();
            audio.Clear();
            picture.Clear();
            //TraverseTree(@"G:\");
            foreach (string drive in paths)
            {
                Console.WriteLine(drive);
                TraverseTree(drive);
            }

            MediaLibrary mediaLibrary = new MediaLibrary(new Dictionary<string, List<MediaItem>>());
            mediaLibrary.value.Add("video", new List<MediaItem>());
            mediaLibrary.value.Add("audio", new List<MediaItem>());
            mediaLibrary.value.Add("image", new List<MediaItem>());

            foreach (string v in video)
            {
                VideoItem item = new VideoItem(new FileInfo(v).Name,v);
                mediaLibrary.value["video"].Add(item);
            }

            foreach (string a in audio)
            {
                AudioItem item = new AudioItem(new FileInfo(a).Name, a);
                mediaLibrary.value["audio"].Add(item);
            }

            foreach (string v in picture)
            {
                PictureItem item = new PictureItem(new FileInfo(v).Name, v);
                mediaLibrary.value["image"].Add(item);
            }
            string json = JsonConvert.SerializeObject(mediaLibrary);


            //StringWriter sw = new StringWriter();
            //JsonWriter writer = new JsonTextWriter(sw);
            //writer.WriteStartObject();
            //writer.WritePropertyName("mediasLibrary");
            //writer.WriteStartArray();

            //writer.WriteStartObject();
            //writer.WritePropertyName("name");
            //writer.WriteValue("video");
            //writer.WritePropertyName("value");
            //writer.WriteStartArray();
            //foreach (string v in video)
            //{
            //    writer.WriteStartObject();
            //    writer.WritePropertyName("path");
            //    writer.WriteValue(v);
            //    writer.WritePropertyName("name");
            //    writer.WriteValue(new FileInfo(v).Name);
            //    writer.WriteEndObject();
            //}
            //writer.WriteEndArray();
            //writer.WriteEndObject();

            //writer.WriteStartObject();
            //writer.WritePropertyName("name");
            //writer.WriteValue("audio");
            //writer.WritePropertyName("value");
            //writer.WriteStartArray();
            //foreach (string a in audio)
            //{
            //    writer.WriteStartObject();
            //    writer.WritePropertyName("path");
            //    writer.WriteValue(a);
            //    writer.WritePropertyName("name");
            //    writer.WriteValue(new FileInfo(a).Name);
            //    writer.WriteEndObject();
            //}
            //writer.WriteEndArray();
            //writer.WriteEndObject();

            //writer.WriteStartObject();
            //writer.WritePropertyName("name");
            //writer.WriteValue("picture");
            //writer.WritePropertyName("value");
            //writer.WriteStartArray();
            //foreach (string p in picture)
            //{
            //    writer.WriteStartObject();
            //    writer.WritePropertyName("path");
            //    writer.WriteValue(p);
            //    writer.WritePropertyName("name");
            //    writer.WriteValue(new FileInfo(p).Name);
            //    writer.WriteEndObject();
            //}
            //writer.WriteEndArray();
            //writer.WriteEndObject();

            //writer.WriteEndArray();
            //writer.WriteEndObject();
            //writer.Flush();

            medias = json;
            string path = System.IO.Directory.GetCurrentDirectory();
            FileStream fs = new FileStream(path + "\\medias.txt", FileMode.Create);
            StreamWriter w = new StreamWriter(fs);
            w.Write(medias.ToCharArray());
            w.Flush();
            w.Close();
            fs.Close();
            if (flag)
            {
                Thread.Sleep(1000 * 60 * 5);
                new Thread(Run).Start();
            }
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
            string videoName = videoAPP.Substring(videoAPP.LastIndexOf(@"\")+1, videoAPP.LastIndexOf(".")- videoAPP.LastIndexOf(@"\")-1);
            string audioName = audioAPP.Substring(audioAPP.LastIndexOf(@"\")+1, audioAPP.LastIndexOf(".")- audioAPP.LastIndexOf(@"\")-1);
            string pictureName = pictureAPP.Substring(pictureAPP.LastIndexOf(@"\")+1, pictureAPP.LastIndexOf(".")- pictureAPP.LastIndexOf(@"\")-1);
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
    }
}

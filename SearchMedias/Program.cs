using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SearchMedias
{
    class Program
    {
        List<string> video = new List<string>();
        List<string> audio = new List<string>();
        List<string> picture = new List<string>();
        private Boolean flag=true;
        private string medias = null;
        private string[] paths = null;
        /// <summary>
        /// 返回json格式字符串的媒体库文件列表
        /// </summary>
        /// <returns></returns>
        public string getMedias()
        {
            if (medias==null)
            {
                string path = System.IO.Directory.GetCurrentDirectory();
                if (File.Exists(path + "\\medias.txt"))
                {
                    medias = File.ReadAllText(path + "\\medias.txt");
                    return medias;
                }else
                {
                    StringWriter sw = new StringWriter();
                    JsonWriter writer = new JsonTextWriter(sw);
                    writer.WriteStartObject();
                    writer.WritePropertyName("mediasLibrary");

                    writer.WriteStartArray();

                    writer.WriteStartObject();
                    writer.WritePropertyName("video");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();

                    writer.WriteStartObject();
                    writer.WritePropertyName("autio");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();

                    writer.WriteStartObject();
                    writer.WritePropertyName("picture");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();

                    writer.WriteEndArray();

                    writer.WriteEndObject();
                    return sw.GetStringBuilder().ToString();
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
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Append);
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
                fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Append);
            }
            
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(path);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        static void Main()
        {
            Program p = new Program();
            //Console.WriteLine(p.getMedias());
            p.Init();
            p.Start();
            while (true)
            {
                Thread.Sleep(1000);
                p.getMedias();
                if (p.medias != null)
                {
                    Console.WriteLine("medias");
                    Console.WriteLine(JObject.Parse(p.getMedias()));
                    //System.IO.FileInfo fi = new System.IO.FileInfo(@"F:\VS2015WorkSpace\SendAndPlayMedia\SearchMedias\bin\Debug\medias.txt");
                    //Console.WriteLine(fi.Length);
                    break;
                }
            }
            /*
            p.TraverseTree(@"G:\");
            Console.WriteLine("检索结束");
            int len = p.video.Count;
            foreach(string s in p.audio)
            {
                Console.WriteLine(s);
            }
            */
            Console.Read();
        }
        /// <summary>
        /// 开始线程Run
        /// </summary>
        public void Start()
        {
            new Thread(Run).Start();
        }
        /// <summary>
        /// 初始化参数，检测是否存在Configuration.txt，如果存在，从中读取媒体库文件目录，否则写入初始值，并初始化paths
        /// </summary>
        public void Init()
        {
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            if (!File.Exists(currentPath + "\\Configuration.txt"))
            {
                string[] drives = Directory.GetLogicalDrives();
                foreach (string drive in drives)
                {
                    if (drive.Equals(@"C:\", StringComparison.CurrentCultureIgnoreCase)) continue;
                    AddPath(drive + "mediasLibrary\\video");
                    AddPath(drive + "mediasLibrary\\music");
                    AddPath(drive + "mediasLibrary\\picture");
                }
            }
            FileStream fs = new FileStream(currentPath + "\\Configuration.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string path = "";
            string s = null;
            while ((s = sr.ReadLine()) != null)
            {
                path += s + ";";
            }
            sr.Close();
            fs.Close();
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
            
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("mediasLibrary");
            writer.WriteStartArray();

            writer.WriteStartObject();
            writer.WritePropertyName("video");
            writer.WriteStartArray();
            foreach(string v in video)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("path");
                writer.WriteValue(v);
                writer.WritePropertyName("name");
                writer.WriteValue(new FileInfo(v).Name);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WritePropertyName("autio");
            writer.WriteStartArray();
            foreach (string a in audio)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("path");
                writer.WriteValue(a);
                writer.WritePropertyName("name");
                writer.WriteValue(new FileInfo(a).Name);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WritePropertyName("picture");
            writer.WriteStartArray();
            foreach (string p in picture)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("path");
                writer.WriteValue(p);
                writer.WritePropertyName("name");
                writer.WriteValue(new FileInfo(p).Name);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();

            medias = sw.GetStringBuilder().ToString();
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

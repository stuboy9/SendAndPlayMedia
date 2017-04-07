using MediaInfo.DotNetWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.info.media;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO.Pipes;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using System.Collections.Concurrent;
using System.Management;
using System.Net;
using System.Threading;
using Windows.System;
using System.Windows;
using SendAndPlayMedia.info;

namespace Test
{
    public delegate void ProcessDelegate(object sender, EventArgs e);

    class Program
    {
        public void GenThupImage(string oriVideoPath, string thubImagePath, string ffmpegPath = @".\ffmpeg", int frameIndex = 10, int thubWidth = 80, int thubHeight = 80)
        {
            string command = string.Format(" -i \"{0}\" -ss {1} -vframes 1 -r 1 -ac 1 -ab 2 -s {2}*{3} -y -f image2 \"{4}\"", oriVideoPath, frameIndex, thubWidth, thubHeight, thubImagePath+"test.jpg");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
        }
        static readonly Uri uri = new Uri("com.yi.myprojection:DoSomething?With=This");
        const string TargetPackageFamilyName = "06e0587e-3eeb-433c-a44a-b5145c6efc47_yrvdh1zv4g43y";
        /**//// <summary> 
            /// 生成缩略图 
            /// </summary> 
            /// <param name="originalImagePath">源图路径（物理路径）</param> 
            /// <param name="thumbnailPath">缩略图路径（物理路径）</param> 
            /// <param name="width">缩略图宽度</param> 
            /// <param name="height">缩略图高度</param> 
            /// <param name="mode">生成缩略图的方式</param>     
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {
            Image originalImage = Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                 
                    break;
                case "W"://指定宽，高按比例                     
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例 
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）                 
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片 
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板 
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                new Rectangle(x, y, ow, oh),
                GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图 
                bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        
        static void Main(string[] args)
        {
            //test();
            String launch = Guid.NewGuid().ToString();
            BroadInfo l = new BroadInfo(new List<BroadParam>());
            BroadParam b = new BroadParam();
            b.ip = getMyip();
            b.port = 8888;
            b.launch_time_id = launch;
            l.param.Add(b);

            BroadParam c = new BroadParam();
            c.ip = getMyip();
            c.port = 8888;
            c.launch_time_id = launch;
            Console.WriteLine(l.param.Contains(c));
            Console.ReadKey();

        }
        public static async void test()
        {

            var supportStatus = await Launcher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri, TargetPackageFamilyName);
            if (supportStatus != LaunchQuerySupportStatus.Available)
            {
                Console.WriteLine("软件未安装！");
            }
            var options = new LauncherOptions { TargetApplicationPackageFamilyName = TargetPackageFamilyName };
            bool success = await Launcher.LaunchUriAsync(uri, options);

        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        private static string getIPAddress()
        {
            System.Net.IPAddress addr;
            addr = new System.Net.IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            return addr.ToString();
        }
        private static  string getMyip()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection moc = mc.GetInstances();
            string MACAddress = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    return ((String[])mo["IPAddress"])[0];
                }
            }
            return "127.0.0.1";
        }
    }

}

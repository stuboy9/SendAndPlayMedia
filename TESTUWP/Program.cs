using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace TESTUWP
{
    class Program
    {

        public static string videoAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
        public static string audioAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";
        public static string pictureAPP = @"E:\software\PotPlayer\PotPlayerMini.exe";

        private static   DeviceInformationCollection deviceInfoColl = null;//设备信息集合
        private static string aqsFilter = "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:= \"{0407d24e-53de-4c9a-9ba1-9ced54641188}\"AND System.Devices.WiFiDirect.Services:~= \"Miracast\"";
        static  void Main(string[] args)
        {
            test();
            Console.ReadKey();
        }
        public static async void test()
        {
            Console.WriteLine(DateTime.Now);
            deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
            foreach (DeviceInformation di in deviceInfoColl)
            {
                Console.WriteLine(di.Name);
            }
            Console.WriteLine(DateTime.Now);
            //Process p = openPlayer(@"G:\mediasLibrary\21.jpg");
            //Thread.Sleep(10*1000);



           // Console.WriteLine(p.MainWindowHandle.ToInt32());
            //IAsyncAction projection = ProjectionManager.StartProjectingAsync(p.MainWindowHandle.ToInt32(), p.MainWindowHandle.ToInt32(), deviceInfoColl.First());


        }

        public static Process openPlayer(string param)
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

    }
}

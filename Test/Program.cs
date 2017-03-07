using MediaInfo.DotNetWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    
    class Program
    {
        public void GenThupImage(string oriVideoPath, string thubImagePath, string ffmpegPath = @".\ffmpeg", int frameIndex = 10, int thubWidth = 80, int thubHeight = 80)
        {
            string command = string.Format(" -i \"{0}\" -ss {1} -vframes 1 -r 1 -ac 1 -ab 2 -s {2}*{3} -f image2 \"{4}\"", oriVideoPath, frameIndex, thubWidth, thubHeight, thubImagePath);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
        }
        static void Main(string[] args)
        {
            using (var mediaInfo = new MediaInfo.DotNetWrapper.MediaInfo())
            {
                mediaInfo.Open(@"G:\mediasLibrary\image\21.jpg");
                int i = 0;
                //while(true)
                Console.WriteLine(mediaInfo.Get(StreamKind.General,0,"FileSize/String"));
                mediaInfo.Close();
                Console.ReadKey();
            }
        }
    }
}

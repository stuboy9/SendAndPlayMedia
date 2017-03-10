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

namespace Test
{
    
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
        static void Main(string[] args)
        {

            //new Program().GenThupImage(@"G:\mediasLibrary\video\157.9.4.mp4", @"G:\mediasLibrary\video\");
            //var ffmpeg = new NReco.VideoConverter.FFMpegConverter();
            //ffmpeg.GetVideoThumbnail(@"G:\mediasLibrary\video\2503.mkv", "test.jpg",10);
            ImageItem ai = new ImageItem(@"G:\mediasLibrary\image\21.jpg");
            Console.WriteLine(ai.ToString());

            using (var mediaInfo = new MediaInfo.DotNetWrapper.MediaInfo())
            {
                mediaInfo.Open(@"G:\mediasLibrary\image\21.jpg");
                string name = mediaInfo.Get(StreamKind.General, 0, "FileName");
                string pathName = mediaInfo.Get(StreamKind.General, 0, "CompleteName");
                string location = "pc";
                string url = mediaInfo.Get(StreamKind.General, 0, "CompleteName");
                string fileExtension = mediaInfo.Get(StreamKind.General, 0, "FileExtension");
                string fileSize = mediaInfo.Get(StreamKind.General, 0, "FileSize/String");
                string fileCreationDate = mediaInfo.Get(StreamKind.General, 0, "File_Created_Date");
                string f = mediaInfo.Get(StreamKind.General, 0, 46);
                Console.WriteLine(fileCreationDate);
                Console.ReadKey();
                int i = 0;
                while (true)
                {
                    Console.WriteLine(mediaInfo.Get(StreamKind.General, 0, i++,InfoKind.Name));
                }
                mediaInfo.Close();
                Console.WriteLine(fileSize);
                Console.ReadKey();
            }
            

        }
    }
}

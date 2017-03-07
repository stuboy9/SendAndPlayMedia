using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendAndPlayMedia.function.media
{
    class VideoFunction
    {
        /// <summary>
        /// 使用ffmpeg程序，生成视频缩略图
        /// </summary>
        /// <param name="oriVideoPath">源视频地址</param>
        /// <param name="thubImagePath">生成图片地址</param>
        /// <param name="ffmpegPath">ffmpeg程序地址</param>
        /// <param name="frameIndex">截取第几帧</param>
        /// <param name="thubWidth">生成图片宽度</param>
        /// <param name="thubHeight">生成图片高度</param>
        public static void GenThupImage(string oriVideoPath, string thubImagePath, string ffmpegPath = @".\ffmpeg", int frameIndex = 10, int thubWidth = 80, int thubHeight = 80)
        {
            string command = string.Format(" -i \"{0}\" -ss {1} -vframes 1 -r 1 -ac 1 -ab 2 -s {2}*{3} -f image2 \"{4}\"", oriVideoPath, frameIndex, thubWidth, thubHeight, thubImagePath);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
        }

    }
}

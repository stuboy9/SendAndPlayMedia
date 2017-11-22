using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.media
{
    class VideoItem:MediaItem
    {
        //public string duration { set; get; }
        public VideoItem(string name, string path, string location, string uri, string fileExtension, string fileSize, string fileCreationDate) : base(name, path, location, uri, fileExtension, fileSize, fileCreationDate)
        {
            //this.name = name;
            //this.path = path;

        }
        public VideoItem(string fileName) : base(fileName)
        {
            
            //this.thumbnailurl = "http://" + GetInternalIP() + ":" + 8634 + "/" + this.name + this.fileSize +".jpg";
                //mediaInfo.Close();
        }
        public override string ToString()
        {
            string text = base.ToString();
            //text += "duration: " + duration + "\r\n";
            text += "thumbnailurl: " + thumbnailurl + "\r\n";
            return text;
        }
        /// <summary>
        /// 使用ffmpeg程序，生成视频缩略图
        /// </summary>
        /// <param name="thubImagePath">生成图片地址</param>
        /// <param name="ffmpegPath">ffmpeg程序地址</param>
        /// <param name="frameIndex">截取第几帧</param>
        /// <param name="thubWidth">生成图片宽度</param>
        /// <param name="thubHeight">生成图片高度</param>
        public void GetImageFromVedio(string thubImagePath, string ffmpegPath = @".\ffmpeg", int frameIndex = 10, int thubWidth = 800, int thubHeight = 480)
        {
            string command = string.Format(" -i \"{0}\" -ss {1} -vframes 1 -r 1 -ac 1 -ab 2 -s {2}*{3} -y -f image2 \"{4}\"", pathName, frameIndex, thubWidth, thubHeight, thubImagePath+ "\\"+this.name + this.fileSize + ".jpg");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口 
            p.Start();//启动程序
        }
    }
}

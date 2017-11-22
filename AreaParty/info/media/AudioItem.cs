using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.media
{
    class AudioItem:MediaItem
    {

        public string duration { get; set; }//音乐总时间
        public string bitRate { get; set; }//比特率
        public string title { get; set; }//标题
        public string album { get; set; }//专辑
        public string trackName { get; set; }//轨道
        public string performer { get; set; }//表演者名字

        
        public AudioItem(string name, string path, string location, string uri, string fileExtension, string fileSize, string fileCreationDate) :base( name,  path,  location,  uri,  fileExtension,  fileSize,  fileCreationDate)
        {
            //base(name, path);
            //this.name = name;
            //this.pathName = path;
        }
        public AudioItem(string fileName) : base(fileName)
        {
            //using (var mediaInfo = new MediaInfo.DotNetWrapper.MediaInfo())
            //{
                

            //    //当文件过的时候此函数有性能瓶颈，可以考虑使用mediaItem中的方式重写此函数。
            //    mediaInfo.Open(fileName);
            //    this.duration = mediaInfo.Get(StreamKind.Audio, 0, "Duration");
            //    this.bitRate = mediaInfo.Get(StreamKind.Audio, 0, "BitRate");
            //    this.title = mediaInfo.Get(StreamKind.General, 0, "Title");
            //    this.album = mediaInfo.Get(StreamKind.General, 0, "Album");
            //    this.trackName = mediaInfo.Get(StreamKind.General, 0, "Track");
            //    this.performer = mediaInfo.Get(StreamKind.General, 0, "Performer");
            //    mediaInfo.Close();

            //}
        }
        public override string ToString()
        {
            



            string text = base.ToString();
            text += "duration: " + duration + "\r\n";
            text += "bitRate: " + bitRate + "\r\n";
            text += "title: " + title + "\r\n";
            text += "album: " + album + "\r\n";
            text += "trackName: " + trackName + "\r\n";
            text += "performer: " + performer + "\r\n";
            text += "thumbnailurl: " + thumbnailurl + "\r\n";
            return text;

        }
    }
}

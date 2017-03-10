using MediaInfo.DotNetWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.media
{
    class ImageItem:MediaItem
    {
        public string width { set; get; }
        public string height { set; get; }
        public ImageItem(string name, string path, string location, string uri, string fileExtension, string fileSize, string fileCreationDate) : base(name, path, location, uri, fileExtension, fileSize, fileCreationDate)
        {
            //this.name = name;
            //this.path = path;
        }
        public ImageItem(string fileName) : base(fileName)
        {
            using (var mediaInfo = new MediaInfo.DotNetWrapper.MediaInfo())
            {
                mediaInfo.Open(fileName);
                this.width = mediaInfo.Get(StreamKind.Image, 0, "Width");
                this.height = mediaInfo.Get(StreamKind.Image, 0, "Height");
                mediaInfo.Close();
            }
        }
        public override string ToString()
        {
            string text = base.ToString();
            text += "width: " + width + "\r\n";
            text += "height: " + height + "\r\n";
            return text;
        }
    }
}

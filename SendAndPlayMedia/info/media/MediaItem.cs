using MediaInfo.DotNetWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.media
{
    class MediaItem
    {
        public string name { set; get; }
        //public string path { set; get; }
        public string pathName { set; get; }
        public string location = "pc";
        public string url = "";
        //public string rootFolder = @"./mediasLibrary";
        public string fileExtension = "";
        public string fileSize = "";
        public string fileCreationDate = "";
        public string thumbnailurl = "";
        public MediaItem(string name,string path,string location,string url,string fileExtension,string fileSize,string fileCreationDate)
        {
            this.name = name;
            this.pathName = path;
            this.location = location;
            this.url = url;
            this.fileExtension = fileExtension;
            this.fileSize = fileSize;
            this.fileCreationDate = fileCreationDate;
        }
        public MediaItem(string fileName)
        {
            FileInfo f = new FileInfo(fileName);
            this.name = f.Name;
            this.pathName = f.FullName;
            this.location = "pc";
            this.url = pathName;
            this.fileExtension = f.Extension;
            this.fileSize = f.Length.ToString();
            this.fileCreationDate = f.CreationTime.ToString();
        }
        public override string ToString()
        {
            string text = "";
            text += "name: " + name + "\r\n";
            text+= "pathName: " + pathName + "\r\n";
            text += "location: " + location + "\r\n";
            text += "uri: " + url + "\r\n";
            text += "fileExtension: " + fileExtension + "\r\n";
            text += "fileSize: " + fileSize + "\r\n";
            text += "fileCreationDate: " + fileCreationDate + "\r\n";
            text += "thumbnailurl: " + thumbnailurl + "\r\n";
            return text;

        }
        protected string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

    }
    //class MediaList
    //{
    //    public string name { set; get; }
    //    public List<MediaItem> value{set;get;}
    //    public MediaList(string name,List<MediaItem> value)
    //    {
    //        this.name = name;
    //        this.value = value;
    //    }
    //}
}

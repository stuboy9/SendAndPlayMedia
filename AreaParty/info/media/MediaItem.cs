using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AreaParty.info.media
{
    class MediaItem
    {
        public string name { set; get; }//名字
        //public string path { set; get; }
        public string pathName { set; get; }//路径
        public string location = "pc";//表示位于pc
        public string url = "";//备用
        //public string rootFolder = @"./mediasLibrary";
        public string fileExtension = "";//扩展名
        public string fileSize = "";//文件大小
        public string fileCreationDate = "";//创建日期
        public string thumbnailurl = "";//缩略图路径
        public MediaItem(string name,string path,string location,string url,string fileExtension,string fileSize,string fileCreationDate)
        {
            this.name = name;
            this.pathName = path;
            this.location = location;
            this.url = url;
            this.fileExtension = fileExtension;
            this.fileSize = fileSize;
            this.fileCreationDate = fileCreationDate;
            this.thumbnailurl = "http://" + MyInfo.myIp + ":" + 8634 + "/" + System.Web.HttpUtility.UrlEncode(this.name) + this.fileSize + ".jpg";
        }
        public MediaItem(string fileName)
        {
            FileInfo f = new FileInfo(fileName);
            this.name = f.Name;
            this.pathName = f.FullName;
            this.location = "pc";
            //this.url = "http://" + SendAndPlayMedia.util.IPUtil.GetInternalIP() + ":" + 8634 + "/" + name.Replace(" ","");
            this.url = "http://" + MyInfo.myIp + ":" + 8634 + "/" + System.Web.HttpUtility.UrlEncode(this.name);
            this.fileExtension = f.Extension;
            this.fileSize = f.Length.ToString();
            this.fileCreationDate = f.CreationTime.ToString();
            this.thumbnailurl = "http://" + MyInfo.myIp + ":" + 8634 + "/" + System.Web.HttpUtility.UrlEncode(this.name) + this.fileSize + ".jpg";
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
        /// <summary>
        /// 获取本机ip，这段代码应该放在util中。
        /// </summary>
        /// <returns></returns>
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
        public  string GetMediaType()
        {
            string videoRegex = "(\\.mp4|\\.mkv|\\.rmvb|\\.mov|\\.flv|\\.avi|\\.mpg|\\.wmv|\\.mpeg)$";
            string audioRegex = "(\\.mp3|\\.wma)$";
            string pictureRegex = "(\\.jpg|\\.bmp|\\.png|\\.tiff|\\.tif|\\.gif)$";
            string re = "unkown";
            try
            {

                Regex r;
                r = new Regex(videoRegex, RegexOptions.IgnoreCase);
                if (r.IsMatch(this.fileExtension)) re = "video";
                r = new Regex(audioRegex, RegexOptions.IgnoreCase);
                if (r.IsMatch(this.fileExtension)) re = "audio";
                r = new Regex(pictureRegex, RegexOptions.IgnoreCase);
                if (r.IsMatch(this.fileExtension)) re = "image";
            }
            catch (System.IO.FileNotFoundException e)
            {
                //Console.WriteLine(e.Message);
                return "unkown";
            }
            return re;
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

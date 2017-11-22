using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;
using System.Web;

namespace node
{

	//using UpnpServiceProvider = dlna.UpnpServiceProvider;
	//using IPUtil = util.IPUtil;
	//using StartDLNA = yi.StartDLNA;

	public class Node
	{
		public string meta = null;
		public string path = null;
		public string id = null;
		public MediaType type;
		public string mimeType = null;
		public long size = 0;
        //public DateTime dateTime = new DateTime();
		public Random random = new Random();
		public string title = null;
		public int parentID = 1;
		public string url = null;
		public string mediaClass = null;
        private static string ip = AreaParty.Program.ip;
        public Node(string path)
		{
			this.path = path;
			analyzeMeta();
		}
		public virtual void analyzeMeta()
		{
            //IPAddress ipAddr = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            //String ip = ipAddr.ToString();
            //File f = new File(path);
            FileInfo fileInfo = new FileInfo(path);
            
			int index = path.LastIndexOf(Path.DirectorySeparatorChar);
			this.title = path.Substring(index + 1);
			this.id = title;
			this.url = "http:/" + ip + ":" + 8634 + "/" + HttpUtility.UrlEncode(id, System.Text.Encoding.UTF8);
            this.size = fileInfo.Length;
			string suffix = path.Substring(path.LastIndexOf(".", StringComparison.Ordinal));
            string suf = dealsuffix(suffix);
			this.type = getMediaType(suf);
			this.mediaClass = MediaClass;
			this.mimeType = getMimeType(suf);
			//<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/" xmlns:sec="http://www.sec.co.kr/"><item id="1522840861756309902" parentID="4210888465153540235" restricted="0"><dc:title>��������10.mkv</dc:title><upnp:class>object.item.videoItem.movie</upnp:class><dc:date>2016-12-05T18:22:25+0800</dc:date><res protocolInfo="http-get:*:video/x-matroska:*" size="1856556919">http://192.168.1.126:8085/content?id=1522840861756309902</res></item></DIDL-Lite>
			this.meta = "<DIDL-Lite xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dlna=\"urn:schemas-dlna-org:metadata-1-0/\" xmlns:sec=\"http://www.sec.co.kr/\"><item id=\"" + id + "\" parentID=\"" + this.parentID + "\" restricted=\"1\"><dc:title>" + this.title + "</dc:title><upnp:class>" + this.mediaClass + "</upnp:class><dc:date>2016-12-27T09:32:30+0800</dc:date><res protocolInfo=\"http-get:*:" + this.mimeType + ":*\" size=\"" + this.size + "\">" + this.url + "</res></item></DIDL-Lite>";
			//System.out.println(this.meta);
		}

        public virtual string dealsuffix(string suf)
        {
            string str = null;
            string[] videoTypes = new string[] { ".mp4", ".mkv", ".rmvb", ".ts", ".flv", ".mov", ".wmv", ".avi", ".mpg", ".mpeg" };
            string[] audioTypes = new string[] { ".mp3", ".wma" };
            string[] pictureTypes = new string[] { ".jpg", ".bmp", ".png", ".tiff", "tif", ".gif" };
            foreach(string s in videoTypes)
            {
                if (s.Equals(suf))
                {
                    str = suf;
                }
            }
            foreach (string s in audioTypes)
            {
                if (s.Equals(suf))
                {
                    str = suf;
                }
            }
            foreach (string s in pictureTypes)
            {
                if (s.Equals(suf))
                {
                    str = suf;
                }
            }
            if(str == null)
            {
                str = ".unkown";
            }
            return str;

        }

		public virtual string MediaClass
		{
			get
			{
				if (this.type.Equals(MediaType.video))
				{
					return "object.item.videoItem.movie";
				}
				else if (this.type.Equals(MediaType.audio))
				{
					return "object.item.audioItem.musicTrack";
				}
				else if (this.type.Equals(MediaType.picture))
				{
					return "object.item.imageItem.photo";
				}
                else
                {
                    return "object.item.unkownItem.unkown";
                }
			}
		}
		public virtual MediaType getMediaType(string suffix)
		{
			string[] videoTypes = new string[] {".mp4", ".mkv", ".rmvb", ".ts", ".flv", ".mov", ".wmv", ".avi", ".mpg", ".mpeg"};
			string[] audioTypes = new string[] {".mp3", ".wma"};
			string[] pictureTypes = new string[] {".jpg", ".bmp", ".png", ".tiff", "tif", ".gif"};
			foreach (string s in videoTypes)
			{
				if (s.Equals(suffix))
				{
					return MediaType.video;
				}
			}
			 foreach (string s in audioTypes)
			 {
				 if (s.Equals(suffix))
				 {
					 return MediaType.audio;
				 }
			 }
			 foreach (string s in pictureTypes)
			 {
				 if (s.Equals(suffix))
				 {
					 return MediaType.picture;
				 }
			 }
			 return MediaType.unkown;
		}
		public virtual string getMimeType(string suffix)
		{
			IDictionary<string, string> mime = new Dictionary<string, string>();
			mime[".mp4"] = "video/mp4";
			mime[".mkv"] = "video/x-mkv";
			mime[".rmvb"] = "video/vnd.rn-realvideo";
			mime[".ts"] = "video/MP2T";
			mime[".flv"] = "video/x-flv";
			mime[".mov"] = "video/quicktime";
			mime[".wmv"] = "video/x-ms-wmv wmv";
			mime[".avi"] = "video/x-msvideo";
			mime[".mpg"] = "video/mpeg";
			mime[".mpeg"] = "video/mpeg";

			mime[".mp3"] = "audio/mpeg";
			mime[".wma"] = "audio/x-ms-wma";

			mime[".jpg"] = "image/jpeg";
			mime[".bmp"] = "image/bmp";
			mime[".png"] = "image/png";
			mime[".tiff"] = "image/tiff";
			mime[".tif"] = "image/tif";
			mime[".gif"] = "image/gif";

            mime[".unkown"] = "unkown/unkown";

			return mime[suffix];
		}
    }
    

}
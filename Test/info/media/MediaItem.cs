using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.media
{
    class MediaItem
    {
        public string name { set; get; }
        public string path { set; get; }
    }
    class MediaList
    {
        public string name { set; get; }
        public List<MediaItem> value{set;get;}
        public MediaList(string name,List<MediaItem> value)
        {
            this.name = name;
            this.value = value;
        }
    }
}

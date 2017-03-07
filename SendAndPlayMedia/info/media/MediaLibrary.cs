using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.info.media;

namespace Test.info
{
    class MediaLibrary :Info
    {
        public string name { set; get; }
        public Dictionary<string, List<MediaItem>> value { set; get; }
        public MediaLibrary(Dictionary<string, List<MediaItem>> value)
        {
            this.name  = "mediaLibrary";
            this.value = value;
        }
    }
}

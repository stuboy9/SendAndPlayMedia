using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info.media
{
    class AudioItem:MediaItem
    {
        public AudioItem(string name,string path):base(name,path)
        {
            //base(name, path);
            //this.name = name;
            //this.pathName = path;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.windows
{
    class NASFolder
    {
        int fid;

        public int folder_id
        {
            get { return fid; }
            set { fid = value; }
        }
        string fname;

        public string folder_name
        {
            get { return fname; }
            set { fname = value; }
        }
        bool fs;

        public bool folder_statue
        {
            get { return fs; }
            set { fs = value; }
        }
    }
}

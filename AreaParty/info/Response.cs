using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info
{
    class Response
    {
        public string status { set; get; }
        public string message { set; get; }
        public string name { set; get; }
        public object data { set; get; }
        public Response(string status,string message,string name, object data)
        {
            this.status = status;
            this.message = message;
            this.name = name;
            this.data = data;
        }
    }
}

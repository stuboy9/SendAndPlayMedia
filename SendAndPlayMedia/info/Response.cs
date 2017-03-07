using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.info
{
    class Response
    {
        public string status { set; get; }
        public string message { set; get; }
        public string name { set; get; }
        public Info data { set; get; }
        public Response(string status,string message,string name,Info data)
        {
            this.status = status;
            this.message = message;
            this.name = name;
            this.data = data;
        }
    }
}

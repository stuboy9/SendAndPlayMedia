using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 移动端发送过来的请求格式
    /// </summary>
    class RequestMessageFormat
    {
        /// <summary>
        /// 请求名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 请求执行的动作(命令)
        /// </summary>
        public string command { get; set; }
        /// <summary>
        /// 附加参数
        /// </summary>
        public string param { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 返回给移动端的数据格式
    /// </summary>
    class ReturnMessageFormat
    {
        /// <summary>
        /// 执行状态
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 进程Bean
    /// </summary>
    class ProcessFormat
    {
        /// <summary>
        /// 进程ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 进程名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 进程使用的CPU百分比
        /// </summary>
        public int cpu { get; set; }
        /// <summary>
        /// 进程使用的内存(K)
        /// </summary>
        public long memory { get; set; }
        /// <summary>
        /// 进程主模块完整路径
        /// </summary>
        public string path { get; set; }
       
    }
}

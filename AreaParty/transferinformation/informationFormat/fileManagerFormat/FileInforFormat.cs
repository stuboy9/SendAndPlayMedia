using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 节点类用于存储当前文件名称、大小(KB)
    /// </summary>
    class FileInforFormat
    {
        /// <summary>
        /// 文件名称(不包含路径)
        /// </summary>
        public string name;
        /// <summary>
        /// 文件大小(KB)
        /// </summary>
        public int size;
        /// <summary>
        /// 文件最后修改时间
        /// </summary>
        public string lastChangeTime;
    }
}

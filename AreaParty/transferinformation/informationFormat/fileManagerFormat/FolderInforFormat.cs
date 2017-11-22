using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 节点类用于存储当前文件夹名称、子文件数目
    /// </summary>
    class FolderInforFormat
    {
        /// <summary>
        /// 文件夹名称(不包含路径)
        /// </summary>
        public string name;
        /// <summary>
        /// 文件夹下一层文件数和文件夹数
        /// </summary>
        public int subNum;
    }
}

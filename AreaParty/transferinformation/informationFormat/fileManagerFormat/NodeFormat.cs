using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 节点类用于存储当前节点名称(绝对路径)、文件信息、文件夹信息
    /// </summary>
    class NodeFormat
    {
        /// <summary>
        /// 当前节点名称(路径。如“H:\\”)
        /// </summary>
        public string path;
        /// <summary>
        /// 当前路径下的文件
        /// </summary>
        public List<FileInforFormat> files;
        /// <summary>
        /// 当前路径下的文件夹
        /// </summary>
        public List<FolderInforFormat> folders;
    }
}

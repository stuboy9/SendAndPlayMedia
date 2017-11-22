using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation.informationFormat.fileManagerFormat
{
    /// <summary>
    /// 存储分享的文件的路径信息的键值对
    /// </summary>
    class SharedFilePathFormat
    {
        /// <summary>
        /// 相当于键(用于存储点击分享那一刻的毫秒数)
        /// </summary>
        public string creatTime;
        /// <summary>
        /// 相当于值(用于存储分享的文件的完整路径)
        /// </summary>
        public string wholePath;
    }
}

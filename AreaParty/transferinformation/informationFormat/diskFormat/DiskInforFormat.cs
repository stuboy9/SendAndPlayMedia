using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation.informationFormat.diskFormat
{
    class DiskInforFormat
    {
        /// <summary>
        /// 指示驱动器上的可用空间（以G为单位）
        /// </summary>
        public long totalFreeSpace;
        /// <summary>
        /// 驱动器上存储空间的总大小（以G为单位）
        /// </summary>
        public long totalSize;
        /// <summary>
        /// 获取文件系统的名称，例如NTFS或者FAT32
        /// </summary>
        public string driveFormat;
        /// <summary>
        /// 获取驱动类型，如CD-ROM、可移动、网络或固定磁盘
        /// </summary>
        public string driveType;
        /// <summary>
        /// 获取驱动器名称，如C
        /// </summary>
        public string name;
        /// <summary>
        /// 驱动器卷标
        /// </summary>
        public string volumeLabel;
        /// <summary>
        /// 驱动器根目录，如C:\
        /// </summary>
        public string rootDirectory;
    }
}

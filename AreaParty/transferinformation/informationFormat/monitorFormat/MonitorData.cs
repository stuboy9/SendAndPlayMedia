using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    /// <summary>
    /// 监控信息封装类
    /// </summary>
    class MonitorData
    {
        /// <summary>
        /// 系统cpu百分比
        /// </summary>
        public int cpu { get; set; }
        /// <summary>
        /// 系统已用内存(GB)
        /// </summary>
        public double memory_used { get; set; }
        /// <summary>
        /// 系统剩余可用内存(GB)
        /// </summary>
        public double memory_available { get; set; }
        /// <summary>
        /// 系统总内存(GB)
        /// </summary>
        public double memory_total { get; set; }
        /// <summary>
        /// 系统上行网速(Kbps)
        /// </summary>
        public double net_up { get; set; }
        /// <summary>
        /// 系统下行网速
        /// </summary>
        public double net_down { get; set; }
        /// <summary>
        /// 系统
        /// </summary>
        public List<ProcessFormat> processes { get; set; }

    }
}

using System;
using System.Diagnostics;
using System.Threading;


namespace transferinformation.processesMonitor
{
    /// <summary>
    /// 进程动态信息类
    /// 动态获取的参数: 
    /// 参数：进程ID
    /// 参数：进程名称
    /// 参数：cpu占有率(%)
    /// 参数：物理内存占有率(K)
    /// 参数：进程主模块完整路径               
    /// </summary>
    class ProcessInfo : IDisposable
    {
        private int cpuPercent;            // 1s内该进程使用的CPU百分比        
        private long memory;               // 当前使用的物理内存大小(KB) 
        private Timer timer;               // 定时器
        private double cpuTime;            // 1S内该进程使用的cpu时间(ms)
        private const int interval = 3000; // 定时器时间间隔和延迟时间

        internal int id;             // 进程ID
        internal string name;        // 进程名称
        internal string fileName;    // 主模块完整路径
        internal int processorCount; // 处理器数量       
        internal TimeSpan cpuTimeNow, cpuTimeOld;    // 当前、前2S该进程的CPU时间        
        internal Process instance;


        /// <summary>
        /// 进程编号
        /// </summary>
        public int ID
        {
            get
            {
                return this.id;
            }
        }
        /// <summary>
        /// 进程名称
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }
        /// <summary>
        /// 进程主模块完整路径
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }
        /// <summary>
        /// 前2s内使用CPU的百分比
        /// </summary>
        public int CpuPercent
        {
            get
            {
                return this.cpuPercent;
            }
        }
        /// <summary>
        /// 占用的内存(KB)
        /// </summary>
        public long Memory
        {
            get
            {
                return this.memory;
            }
        }

        /// <summary>
        /// 内部构造函数
        /// </summary>
        /// <param name="ID">进程ID</param>
        /// <param name="name">进程名称</param>
        /// <param name="processorCount">处理器数量</param>        
        /// <param name="instance">进程实例引用</param>
        internal ProcessInfo(int ID, string name, int processorCount, Process instance)
        {
            this.id = ID;
            this.name = name;
            this.processorCount = processorCount;
            this.instance = instance;
        }


        /// <summary>
        /// 初始化：获取进程主模块完整路径、使用当前值初始化计数器计数值、初始化定时器(延迟2S, 间隔2S)
        /// </summary>
        internal void init()
        {
            try
            {
                fileName = instance.MainModule.FileName;
            } catch
            {
                fileName = "";
            }
            this.cpuTimeOld = instance.TotalProcessorTime;
            this.timer = new Timer(new TimerCallback(timer_Elapsed), null, 1000, interval);
        }
        /// <summary>
        /// 定时器时间到执行的事件
        /// </summary>
        /// <param name="sender"></param>
        private void timer_Elapsed(object sender)
        {
            // 获取当前使用的内存
            reGetMemory();
            // 更新当前CPU百分比
            try
            {
                this.cpuTimeNow = instance.TotalProcessorTime;
                // 计算cpu时间
                cpuTime = (this.cpuTimeNow - this.cpuTimeOld).TotalMilliseconds;
                // 四舍五入CPU百分比
                this.cpuPercent = (int)((cpuTime / 2000 / processorCount * 100) + 0.5);
                this.cpuTimeOld = this.cpuTimeNow;
            }
            catch { }           
        }             
      

        /// <summary>
        /// 更新当前占用的内存(KB)
        /// </summary>
        private void reGetMemory()
        {
            try
            {
                instance.Refresh();
                this.memory = instance.WorkingSet64 / 1024;
            }
            catch { }    
        }

        /// <summary>
        /// 释放占用的资源
        /// </summary>
        public void Dispose()
        {
            this.name = null;            
            timer.Dispose();
            this.fileName = null;
            this.instance = null;
            this.cpuTimeNow = TimeSpan.Zero;
            this.cpuTimeOld = TimeSpan.Zero;
            GC.SuppressFinalize(this);            
        }
    }
}

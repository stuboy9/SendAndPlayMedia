using System.Diagnostics;

namespace transferinformation.systemMonitor
{
    /// <summary>
    /// 代表一个安装在电脑上的网络适配器
    /// 使用本Class的某些属性可以获取当前网络速度
    /// </summary>
    public class NetworkAdapter
    {
        private long dlSpeed, ulSpeed;                      // 下载\上传速度(bytes/second)
        private long dlValue, ulValue;                      // 当前时刻下载\上传计数器(bytes)
        private long dlValueOld, ulValueOld;                // 前一秒的下载\上传计数值(bytes)
        internal string name;                               // 适配器名称
        internal PerformanceCounter dlCounter, ulCounter;   // 监控上传、下载速度的性能计数器

        /// <summary>
        /// 内部构造函数
        /// </summary>
        internal NetworkAdapter(string name)
        {
            this.name = name;
        }

        

        /// <summary>
        /// 使用当前值初始化(前一秒)计数器计数值
        /// </summary>
        internal void init()
        {
            // dlValueOld和ulValueOld要在refresh()中使用，所有需要先实现
            this.dlValueOld = this.dlCounter.NextSample().RawValue;
            this.ulValueOld = this.ulCounter.NextSample().RawValue;
        }

        /// <summary>
        /// 通过性能计数器获取新样例，并刷新dlSpeed和ulSpeed
        /// 本方法由NetworkMonitor每隔1S调用
        /// </summary>
        internal void refresh()
        {
            this.dlValue = this.dlCounter.NextSample().RawValue;
            this.ulValue = this.ulCounter.NextSample().RawValue;

            // 计算上传、下载网速
            this.dlSpeed = this.dlValue - this.dlValueOld;
            this.ulSpeed = this.ulValue - this.ulValueOld;

            this.dlValueOld = this.dlValue;
            this.ulValueOld = this.ulValue;
        }

        /// <summary>
        /// </summary>
        /// <returns>适配器名称</returns>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// 适配器名称
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }
        /// <summary>
        /// 当前下载速度(bytes/second)
        /// </summary>
        public long DownloadSpeed
        {
            get
            {
                return this.dlSpeed;
            }
        }
        /// <summary>
        /// 当前上传速度(bytes/second)
        /// </summary>
        public long UploadSpeed
        {
            get
            {
                return this.ulSpeed;
            }
        }
        /// <summary>
        /// 当前下载速度(Kbytes/second)
        /// </summary>
        public double DownloadSpeedKbps
        {
            get
            {
                return ((int)(this.dlSpeed / 1024 * 10)) / 10;
            }
        }
        /// <summary>
        /// 当前上传速度(Kbytes/second)
        /// </summary>
        public double UploadSpeedKbps
        {
            get
            {
                return ((int)(this.ulSpeed / 1024 * 10)) / 10;
            }
        }
    }
}

using System.Collections;
using System.Diagnostics;
using System.Timers;
using System.Management;

namespace transferinformation.systemMonitor
{
    /// <summary>
	/// 监控PC上信息(CPU、网络、内存)
	/// 使用了.Net库中的性能计数器
	/// </summary>
	public class SystemMonitor
    {
        private Timer timer;                        // timer事件执行每隔1S更新适配器的值
        private float physicalMemory;               // 总的物理内存大小       
        private ArrayList adapters;                 // 网络适配器列表
        private ArrayList monitoredAdapters;        // 当前监控的适配器列表
        private PerformanceCounter pcCpuLoadCounter;   //CPU计数器 
        
        public SystemMonitor()
        {
            this.adapters = new ArrayList();
            this.monitoredAdapters = new ArrayList();
            EnumerateNetworkAdapters();

            pcCpuLoadCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpuLoadCounter.MachineName = ".";
            pcCpuLoadCounter.NextValue();

            // 获取物理内存
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    physicalMemory = float.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
            mc.Dispose();
            moc.Dispose();

            timer = new Timer(3000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {    
            foreach (NetworkAdapter adapter in this.monitoredAdapters)
                adapter.refresh();
        }

        /// <summary>
        /// 总物理内存大小(GB)
        /// </summary>
        public double PhysicalMemory
        {
            get
            {
                return System.Math.Round(physicalMemory / 1024 / 1024 / 1024, 1);
                 
            }
        }

        /// <summary>
        /// 总CPU百分比
        /// </summary>
        public int CpuLoad
        {
            get
            {

                return (int)System.Math.Round(pcCpuLoadCounter.NextValue(), 2, System.MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// 可用物理内存大小(GB)
        /// </summary>
        public double MemoryAvailable
        {
            get
            {
                float availableG = 0;

                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availableG = float.Parse(mo["FreePhysicalMemory"].ToString())/1024/1024;
                    }
                }
                mos.Dispose();                
                return System.Math.Round(availableG, 1);
            }
        }
        /// <summary>
        /// 枚举出所有安装的网络适配器
        /// </summary>
        private void EnumerateNetworkAdapters()
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");

            foreach (string name in category.GetInstanceNames())
            {
                // 任何PC均有
                if (name == "MS TCP Loopback interface")
                    continue;
                // 创建网络适配器实例，并为其创建性能计数器
                NetworkAdapter adapter = new NetworkAdapter(name);
                adapter.dlCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
                adapter.ulCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);
                this.adapters.Add(adapter);
            }
        }
              

        /// <summary>
        /// 获取PC安装的网络适配器的实例列表
        /// </summary>
        public NetworkAdapter[] Adapters
        {
            get
            {
                return (NetworkAdapter[])this.adapters.ToArray(typeof(NetworkAdapter));
            }
        }
        
                
        /// <summary>
        /// 开启timer，并将所有网络适配器添加到正在监测的网络适配器列表中
        /// </summary>
        public void StartMonitoring()
        {
            if (this.adapters.Count > 0)
            {
                foreach (NetworkAdapter adapter in this.adapters)
                    if (!this.monitoredAdapters.Contains(adapter))
                    {
                        this.monitoredAdapters.Add(adapter);
                        adapter.init();
                    }
                timer.Enabled = true;
            }
        }       
       
        
        /// <summary>
        /// 关闭timer，并将正在监测的网络适配器列表中的适配器全部移除
        /// </summary>
        public void StopMonitoring()
        {
            this.monitoredAdapters.Clear();
            this.adapters.Clear();           
            timer.Dispose();
        }       

    }
}

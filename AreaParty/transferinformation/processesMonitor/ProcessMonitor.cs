using System;
using System.Collections;
using System.Diagnostics;

namespace transferinformation.processesMonitor
{
    class ProcessMonitor
    {
        private int processorCount;                     // 处理器数量
        private ArrayList processesList;                // 进程列表
        private ArrayList monitorProcessesList;         // 当前监控的进程列表

        /// <summary>
        /// 构造函数：初始化进程列表、监控列表、获取处理器数量、填充监控列表
        /// </summary>
        public ProcessMonitor()
        {
            this.processorCount = Environment.ProcessorCount;
            this.processesList = new ArrayList();
            monitorProcessesList = new ArrayList();
            EnumerateProcesses();            
        }
        
        /// <summary>
        /// 枚举进程并填充到进程列表中
        /// </summary>
        private void EnumerateProcesses()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process instance in processes)
            {
                try
                {
                    ProcessInfo process = new ProcessInfo(instance.Id,
                        instance.ProcessName,                        
                        processorCount,
                        instance);
                    process.init();
                    this.processesList.Add(process);
                }
                catch { }
            }
        }


        /// <summary>
        /// 获取所有监听的进程实例列表
        /// </summary>
        public ProcessInfo[] ProcessesList
        {
            get
            {
                return (ProcessInfo[])monitorProcessesList.ToArray(typeof(ProcessInfo));
            }
        }


        /// <summary>
        /// 开启timer，并将所有进程添加到正在监测的进程列表中
        /// </summary>
        public void StartMonitoring()
        {
            if (this.processesList.Count > 0)
            {
                foreach (ProcessInfo process in this.processesList)
                    if (!monitorProcessesList.Contains(process))
                    {
                        monitorProcessesList.Add(process);                       
                    }              
            }
        }
                
        /// <summary>
        /// 释放进程监控资源，并将正在监测的进程列表中的进程全部移除
        /// </summary>
        public void StopMonitoring()
        {
            foreach (ProcessInfo process in monitorProcessesList)
            {
                process.Dispose();
            }
            monitorProcessesList.Clear();
            this.processesList.Clear();            
        }
        
    }
}

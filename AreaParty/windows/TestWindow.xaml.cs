using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AreaParty.windows
{
    /// <summary>
    /// TestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }
        

        private void Label_More_Click(object sender, MouseButtonEventArgs e)
        {
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = "cmd.exe";
            rdcProcess.StartInfo.UseShellExecute = false;
            rdcProcess.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            rdcProcess.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            rdcProcess.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            rdcProcess.StartInfo.CreateNoWindow = true;//不显示程序窗口
            //rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            rdcProcess.Start();
            rdcProcess.StandardInput.WriteLine("\"C:/Users/Yi/Desktop/报告/配置文件说明PDF.pdf\"" + "&exit");
            rdcProcess.StandardInput.AutoFlush = true;
            rdcProcess.Close();
        }
    }
}

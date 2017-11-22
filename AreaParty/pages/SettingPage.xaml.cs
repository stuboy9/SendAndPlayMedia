using AreaParty.command;
using AreaParty.info;
using AreaParty.windows;
using client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AreaParty.pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : UserControl
    {
        public static SettingPage settingPage;
        public SettingPage()
        {
            InitializeComponent();
            string status = util.config.ConfigUtil.GetValue("longconnect");
            if (status.Equals("false")) { this.LoginCheckBox.IsChecked = false; }
            else
                this.LoginCheckBox.IsChecked = true;
        }
        private void MoreLabel_Click(object sender, MouseButtonEventArgs e)
        {
            int index = this.SettingTab.SelectedIndex;
            
            string pdf = "";
            switch (index)
            {
               
                case 0:
                     pdf = ConfigResource.GetRDP_PDF_PATH();
                     break;
                case 1:
                    pdf =ConfigResource.GetGAME_PDF_PATH();
                    break;
                //case 2:
                //    pdf = ConfigResource.GetSCREEN_PDF_PATH();
                //    break;
                case 2:
                    pdf = ConfigResource.GetLONGCENNECT_PDF_PATH();
                    break;
                case 3:
                    pdf = ConfigResource.GetNAS_PDF_PATH();
                    break;
                case 4:
                    pdf = ConfigResource.GetSOFTWAREUSE_PDF_PATH();
                    break;

            }
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = "cmd.exe";
            rdcProcess.StartInfo.UseShellExecute = false;
            rdcProcess.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            rdcProcess.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            rdcProcess.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            rdcProcess.StartInfo.CreateNoWindow = true;//不显示程序窗口
            //rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            rdcProcess.Start();
            rdcProcess.StandardInput.WriteLine("\"" + pdf + "\"" + "&exit");
            rdcProcess.StandardInput.AutoFlush = true;
            rdcProcess.Close();
        }

        private void Login_CheckBox_NAS_Click(object sender, RoutedEventArgs e)
        {
            if (this.LoginCheckBox_NAS.IsChecked == true)
            {
                NasConnectWindow nasConnectWindow = new NasConnectWindow();
                nasConnectWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                nasConnectWindow.Owner = AreaParty.MainWindow.main;
                nasConnectWindow.ShowDialog();
                string status = util.config.ConfigUtil.GetValue("naslongconnect");
                if (status.Equals("false")) { this.LoginCheckBox_NAS.IsChecked = false; }
                else
                    this.LoginCheckBox_NAS.IsChecked = true;
            }
            else
            {
                if (System.Windows.MessageBox.Show("确定断开NAS长连接", "NAS长连接", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    util.config.ConfigUtil.SetValue("naslongconnect", "false");
                    MainWindow.main.Status = "NAS长连接断开";
                }
                else
                {
                    this.LoginCheckBox_NAS.IsChecked = true;
                }
            }
        }

        private void Login_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (this.LoginCheckBox.IsChecked == true)
            {
                UserLoginWindow ulw = new UserLoginWindow();
                ulw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ulw.Owner = AreaParty.MainWindow.main;
                ulw.ShowDialog();
                string status = util.config.ConfigUtil.GetValue("longconnect");
                if (status.Equals("false")) { this.LoginCheckBox.IsChecked = false; }
                else
                    this.LoginCheckBox.IsChecked = true;
            }
            else
            {
                if (System.Windows.MessageBox.Show("确定断开长连接", "长连接", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    util.config.ConfigUtil.SetValue("longconnect", "false");
                    ClientPCNew2.StopService();
                    util.config.ConfigUtil.SetValue("longconnect", "false");
                    MainWindow.main.Status = "长连接断开";
                    
                }
                else
                {
                    this.LoginCheckBox.IsChecked = true;
                }
            }
        }
    }
}

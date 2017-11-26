using AreaParty.info;
using AreaParty.util.config;
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
    /// InitSetting.xaml 的交互逻辑
    /// </summary>
    public partial class InitSetting : Window
    {
        const int REMOTEDESKTOP = 1;
        const int GAME = 2;
        const int LONGIN = 3;
        const int SCREEN = 4;
        const int MORE = 6;
        const int NAS = 5;
        private int current = 1;
        public InitSetting()
        {
            InitializeComponent();
            string status = ConfigUtil.GetValue("longconnect");

            if (status.Equals("false")) { this.LoginCheckBox.IsChecked = false; }
            else
                this.LoginCheckBox.IsChecked = true;
            string status2 = ConfigUtil.GetValue("naslongconnect");
            if (status2.Equals("false")) { this.LoginCheckBox_NAS.IsChecked = false; }
            else
                this.LoginCheckBox_NAS.IsChecked = true;
        }
        /// <summary>
        /// 窗口移动事件
        /// </summary>
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Label_More_Click(object sender, MouseButtonEventArgs e)
        {
            string pdf = "";
            switch (current)
            {
                case REMOTEDESKTOP:
                    pdf = ConfigResource.GetRDP_PDF_PATH();
                    break;
                case GAME:
                    pdf = ConfigResource.GetGAME_PDF_PATH();
                    break;
                case LONGIN:
                    pdf = ConfigResource.GetLONGCENNECT_PDF_PATH();
                    break;
                case NAS:
                    pdf = ConfigResource.GetNAS_PDF_PATH();
                    break;
                case SCREEN:
                    pdf = ConfigResource.GetSCREEN_PDF_PATH();
                    //this.Close();
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

        private void Button_Finish_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Finish.UpdateLayout();
            this.Button_Finish.Visibility = Visibility.Collapsed;
            switch (current)
            {
                case REMOTEDESKTOP:
                    this.RemoteSetting.Visibility = Visibility.Hidden;
                    this.GameSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case GAME:
                    this.GameSetting.Visibility = Visibility.Hidden;
                    this.LoginSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case LONGIN:
                    this.LoginSetting.Visibility = Visibility.Hidden;
                    this.NasSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case NAS:
                    this.NasSetting.Visibility = Visibility.Hidden;
                    this.ScreenSetting.Visibility = Visibility.Visible;
                    this.Close();
                    current++;
                    break;
                case SCREEN:
                    //    Window1 w = new Window1();
                    //    w.finish += CallBack;
                    //    w.ShowDialog();
                    this.Close();
                    break;



            }
            this.Button_Finish.Visibility = Visibility.Visible;

        }
        private void CallBack()
        {
            this.Close();
        }

        private void Button_Skip_Click(object sender, RoutedEventArgs e)
        {
            //this.Focus();
            switch (current)
            {
                case REMOTEDESKTOP:
                    this.RemoteSetting.Visibility = Visibility.Hidden;
                    this.GameSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case GAME:
                    this.GameSetting.Visibility = Visibility.Hidden;
                    this.LoginSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case LONGIN:
                    this.LoginSetting.Visibility = Visibility.Hidden;
                    this.NasSetting.Visibility = Visibility.Visible;
                    current++;
                    break;
                case NAS:
                    this.NasSetting.Visibility = Visibility.Hidden;
                    this.ScreenSetting.Visibility = Visibility.Visible;
                    this.Close();
                    current++;
                    break;
                case SCREEN:
                    this.Close();
                    break;

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
        }

        private void Login_CheckBox_NAS_Click(object sender, RoutedEventArgs e)
        {
            if (this.LoginCheckBox_NAS.IsChecked == true)
            {
                NasConnectWindow nasConnectWindow = new NasConnectWindow();
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
                    ConfigUtil.SetValue("naslongconnect", "false");
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
                ulw.ShowDialog();
                string status = ConfigUtil.GetValue("longconnect");
                if (status.Equals("false")) { this.LoginCheckBox.IsChecked = false; }
                else
                    this.LoginCheckBox.IsChecked = true;
            }
            else
            {
                if (System.Windows.MessageBox.Show("确定断开长连接", "长连接", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    ConfigUtil.SetValue("longconnect", "false");
                }
                else
                {
                    this.LoginCheckBox.IsChecked = true;
                }
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void close_button_MouseMove(object sender, MouseEventArgs e)
        {
            //ImageBrush brush = new ImageBrush();
            //brush.ImageSource = new BitmapImage(new Uri("/styles/skin/close_bottun.png", UriKind.Relative));
            //close_button.Background = brush;
        }
    }
}


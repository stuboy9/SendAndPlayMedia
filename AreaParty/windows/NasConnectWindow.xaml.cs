using AreaParty.function.nas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AreaParty.windows
{
    /// <summary>
    /// NasConnectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NasConnectWindow : Window
    {
        public NasConnectWindow()
        {
            InitializeComponent();
            string checkbox = util.config.ConfigUtil.GetValue("nascheckbox");
            if (checkbox.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                NasCheckBox.IsChecked = true;
                this.UserNameTextBox.Text = util.config.ConfigUtil.GetValue("nasusername");
                this.PasswordTextBox.Password = util.config.ConfigUtil.GetValue("naspassword");
            }
            else
            {
                NasCheckBox.IsChecked = false;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            string username;
            string password;
            string remotepath;  //NAS将要映射的共享文件夹
            string localpath;   //映射为本地的盘符
            //string nasname = this.NasNameTextBox.Text;
            username = this.UserNameTextBox.Text;
            password = this.PasswordTextBox.Password;
            //string username = "admin";    //NAS的账户
            //string password = "abc123";    //NAS的密码
            //Device device = new Device(nasname, username, password);
            if((username == "")||(password == ""))
            {
                System.Windows.Forms.MessageBox.Show("账号密码为空");
                return;
            }
            string path = "";           //保存选择文件夹的名称
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            dilog.ShowDialog();
            remotepath = dilog.SelectedPath;
            localpath = NasFunction.FindDiskName();
            NasFunction.Get_Share(remotepath, localpath, username, password);

            if (NasCheckBox.IsChecked == true)
            {
                util.config.ConfigUtil.SetValue("nascheckbox", "true");
            }
            else
            {
                util.config.ConfigUtil.SetValue("nascheckbox", "false");
            }
            util.config.ConfigUtil.SetValue("nasusername", username);
            util.config.ConfigUtil.SetValue("naspassword", password);
            util.config.ConfigUtil.SetValue("naslongconnect", "true");
            //config.AppSettings.Settings["nasdir"].Value = nas_dir;
            MainWindow.main.Status_Nas = "NAS已连接";
            this.Close();
        }
        ///// <summary>
        ///// 映射共享文件
        ///// </summary>
        ///// <param name="remotepath"></param>
        ///// <param name="localpath"></param>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        //public static void Get_Share(string remotepath, string localpath, string username, string password)
        //{
        //    Process.Start("net", " use " + localpath + " " + remotepath + " " + password + " /user:" + username);
        //}

        /////断开共享文件的映射
        //public static void Break_Share(string localpath)
        //{
        //    Process.Start("net", " use " + localpath + " /delete");
        //}

        /////查找PC上的盘符的名称，从Z-A(未使用的字母)赋予给新映射的网络盘
        //public void FindDiskName()
        //{
        //    DriveInfo[] allDrives = DriveInfo.GetDrives();

        //    for (int i = 90; i > 64; i--)
        //    {
        //        var NewNasName = char.ConvertFromUtf32(i);
        //        var str = string.Empty + NewNasName;
        //        Boolean exist = true;
        //        for (int j = allDrives.Length - 1; j >= 0; j--)
        //        {
        //            string DiskName = allDrives[j].Name.Trim(':', '\\');
        //            Console.WriteLine(DiskName);
        //            if (DiskName != str)
        //            {
        //                exist = false;
        //            }
        //            else
        //            {
        //                exist = true;
        //                break;
        //            }

        //        }
        //        if (exist == false)
        //        {
        //            localpath = str + ":";
        //            break;
        //        }
        //    }
        //}


        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.Status_Nas = "未连接NAS";
            this.Close();
        }

        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string name = this.UserNameTextBox.Text;
            if (name == "NAS管理员账号")
            {
                this.UserNameTextBox.Text = "";
            }
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.UserNameTextBox.Text == "")
            {
                this.UserNameTextBox.Text = "NAS管理员账号";
            }
        }

        private void NasCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (NasCheckBox.IsChecked == true)
            {
                string user = util.config.ConfigUtil.GetValue("nasusername");
                string password = util.config.ConfigUtil.GetValue("naspassword");
                if ((this.UserNameTextBox.Text != "") && (this.PasswordTextBox.Password != ""))
                {
                    util.config.ConfigUtil.SetValue("nasusername", this.UserNameTextBox.Text);
                    util.config.ConfigUtil.SetValue("naspassword", this.PasswordTextBox.Password);
                }
                util.config.ConfigUtil.SetValue("nascheckbox", NasCheckBox.IsChecked.ToString());
            }
            else
            {
                util.config.ConfigUtil.SetValue("nascheckbox", NasCheckBox.IsChecked.ToString());
            }
            
        }
    }
    //class Device
    //{
    //    public string devicename = null;
    //    public string username = null;
    //    public string password = null;
    //    public Device(string devicename,string username, string password)
    //    {
    //        this.devicename = devicename;
    //        this.username = username;
    //        this.password = password;
    //    }
    //}
}

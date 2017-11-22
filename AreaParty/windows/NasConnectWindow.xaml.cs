﻿using System;
using System.Collections.Generic;
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
        string remotepath;  //NAS将要映射的共享文件夹
        string localpath;   //映射为本地的盘符
        public NasConnectWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            //string nasname = this.NasNameTextBox.Text;
            string username = this.UserNameTextBox.Text;
            string password = this.PasswordTextBox.Password;
            
                                //string username = "admin";    //NAS的账户
                                //string password = "abc123";    //NAS的密码
            //Device device = new Device(nasname, username, password);

            string path = "";           //保存选择文件夹的名称
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            dilog.ShowDialog();
            remotepath = dilog.SelectedPath;
            FindDiskName();
            Get_Share(remotepath, localpath, username, password);
            this.Close();
            //if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            //{

            /*System.Windows.MessageBox.Show(path);     */                       //显示该路径名
                                                                                 //}
                                                                                 //NASFoldersWindow nfw = new NASFoldersWindow();
                                                                                 //nfw.Show();
                                                                                 //this.Close();


            //Thread t = new Thread(Login);
            //User u = new User(name, password);
            //Thread t = new Thread(Login);
            ////t.SetApartmentState(ApartmentState.STA);
            //t.IsBackground = true;
            //t.Start(u);

        }

        public static void Get_Share(string remotepath, string localpath, string username, string password)
        {
            Process.Start("net", " use " + localpath + " " + remotepath + " " + password + " /user:" + username);
        }

        //断开共享文件的映射
        public static void Break_Share(string localpath)
        {
            Process.Start("net", " use " + localpath + " /delete");
        }



        //查找PC上的盘符的名称，从Z-A(未使用的字母)赋予给新映射的网络盘
        public void FindDiskName()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            for (int i = 90; i > 64; i--)
            {
                var NewNasName = char.ConvertFromUtf32(i);
                var str = string.Empty + NewNasName;
                Boolean exist = true;
                for (int j = allDrives.Length - 1; j >= 0; j--)
                {
                    string DiskName = allDrives[j].Name.Trim(':', '\\');
                    Console.WriteLine(DiskName);
                    if (DiskName != str)
                    {
                        exist = false;
                    }
                    else
                    {
                        exist = true;
                        break;
                    }

                }
                if (exist == false)
                {
                    localpath = str + ":";
                    break;
                }
            }
        }
            private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.Status_Nas = "未连接NAS";
            this.Close();
        }
        //private void NasNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    string name = this.NasNameTextBox.Text;
        //    if (name == "NAS设备名称")
        //    {
        //        this.NasNameTextBox.Text = "";
        //    }
        //}

        //private void NasNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (this.NasNameTextBox.Text == "")
        //    {
        //        this.NasNameTextBox.Text = "NAS设备名称";
        //    }
        //}

        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string name = this.UserNameTextBox.Text;
            if (name == "NAS用户账号")
            {
                this.UserNameTextBox.Text = "";
            }
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.UserNameTextBox.Text == "")
            {
                this.UserNameTextBox.Text = "NAS用户账号";
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
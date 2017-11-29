using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using transferinformation;

namespace AreaParty.function.nas
{
    class NasFunction
    {
        
        /// <summary>
        /// 封装映射函数
        /// </summary>
        /// <param name="remotepath"></param>
        /// <returns></returns>
        public static ReturnMessageFormat addNasFolder(string remotepath)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            string localpath;
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string username = config.AppSettings.Settings["nasusername"].Value;
            string password = config.AppSettings.Settings["naspassword"].Value;
            try
            {
                localpath = FindDiskName();
                Get_Share(remotepath, localpath, username, password);
                message.status = Order.success;
                message.message = "";
                message.data = null;
            }
            catch (Exception e)
            {
                message.status = Order.failure;
                message.message = e.Message;
                message.data = null;
            }
            config.Save(ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

            return message;

        }
        /// <summary>
        /// 封装解除映射函数
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ReturnMessageFormat deleteNasFolder(string path)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            path = path.Substring(0, 2);
            try
            {
                Break_Share(path);
                message.status = Order.success;
                message.message = "";
                message.data = null;
            }
            catch(Exception e)
            {
                message.status = Order.failure;
                message.message = e.Message;
                message.data = null;
            }
            return message;
        }

        /// <summary>
        /// 映射共享文件
        /// </summary>
        /// <param name="remotepath"></param>
        /// <param name="localpath"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void Get_Share(string remotepath, string localpath, string username, string password)
        {
            Process.Start("net", " use " + localpath + " " + remotepath + " " + password + " /user:" + username);
        }

        /// <summary>
        /// 断开共享文件的映射
        /// </summary>
        /// <param name="localpath"></param>
        public static void Break_Share(string localpath)
        {
            
            Process.Start("net", " use " + localpath + " /delete");
        }

        /// <summary>
        /// 查找PC上的盘符的名称，从Z-A(未使用的字母)赋予给新映射的网络盘
        /// </summary>
        public static string FindDiskName()
        {
            string localpath = null;
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
            return localpath;
        }

        //public static ReturnMessageFormat getNasFolder()
        //{
        //    ReturnMessageFormat message = new ReturnMessageFormat();
        //    Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        //    string nas_dir = config.AppSettings.Settings["nasdir"].Value;

        //    if (Directory.Exists(nas_dir))
        //    {
        //        try
        //        {

        //            message.status = Order.success;
        //            message.message = "";
        //            message.data = null;
        //        }
        //        catch(System.IO.IOException e)
        //        {
        //            message.status = Order.failure;
        //            message.message = e.Message;
        //            message.data = null;
        //        }

        //    }
        //    else
        //    {
        //        message.status = Order.failure;
        //        message.message = "";
        //        message.data = null;

        //    }

        //    config.Save(ConfigurationSaveMode.Modified);
        //    System.Configuration.ConfigurationManager.RefreshSection("appSettings");

        //    return message;
        //}
    }
}

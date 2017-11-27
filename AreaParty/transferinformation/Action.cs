using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic.Devices;
using System.Text;
using System.Threading.Tasks;
using transferinformation.informationFormat.diskFormat;
using System.IO;
using transferinformation.informationFormat.applicationFormat;
using System.Threading;
using transferinformation.informationFormat.fileManagerFormat;
using log4net;
using System.Reflection;
using System.Configuration;

namespace transferinformation
{
    class Action
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region ProcessHelper
        /// <summary>
        /// 关闭指定进程
        /// </summary>
        /// <param name="processId">进程ID</param>
        /// <returns>执行状态封装的信息类</returns>
        public static ReturnMessageFormat CloseProcess(int processId)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            try
            {
                Process process = Process.GetProcessById(processId);
                process.Kill();
                message.status = Order.success;
                message.message = "";
            }
            catch (Exception e)
            {
                message.status = Order.failure;
                message.message = e.Message;
            }
            return message;
        }

        #endregion

        #region RegistryHelper
        /// <summary>
        /// 读取指定路径下指定名称的注册表的值
        /// </summary>
        /// <param name="root">注册表中的顶级节点</param>
        /// <param name="subkey">要打开的子项的路径</param>
        /// <param name="name">注册表项名称</param>
        /// <returns>指定路径下指定名称对应的值</returns>
        public static string GetRegistryData(RegistryKey root, string subkey, string name)
        {
            string registData = string.Empty;
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey != null)
            {
                registData = myKey.GetValue(name).ToString();
            }
            return registData;
        }

        /// <summary>
        /// 向指定路径下指定名称的注册表项写数据
        /// </summary>
        /// <param name="root">注册表中的顶级节点</param>
        /// <param name="subkey">要打开的子项的路径</param>
        /// <param name="name">注册表项名称</param>
        /// <param name="value">数据</param>
        public static void SetRegistryData(RegistryKey root, string subkey, string name, string value)
        {
            RegistryKey aimdir = root.CreateSubKey(subkey);
            aimdir.SetValue(name, value);
            aimdir.Close();
        }

        /// <summary>
        /// 删除指定路径下指定名称的注册表项
        /// </summary>
        /// <param name="root">注册表中的顶级节点</param>
        /// <param name="subkey">要打开的子项路径</param>
        /// <param name="name">要删除的注册表项名称</param>
        public static void DeleteRegist(RegistryKey root, string subkey, string name)
        {            
            string[] subkeyNames;
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            subkeyNames = myKey.GetSubKeyNames();
            foreach (string aimKey in subkeyNames)
            {
                if (aimKey == name)
                    myKey.DeleteSubKeyTree(name);
            }
        }

        /// <summary>
        /// 判断指定路径下指定名称的注册表项是否存在
        /// </summary>
        /// <param name="root">注册表中的顶级节点</param>
        /// <param name="subkey">要打开的子项路径</param>
        /// <param name="name">注册表项名称</param>
        /// <returns>存在与否</returns>
        public static bool IsRegistryExist(RegistryKey root, string subkey, string name)
        {
            bool _exit = false;
            string[] subkeyNames;
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            subkeyNames = myKey.GetSubKeyNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == name)
                {
                    _exit = true;
                    break;
                }
            }
            return _exit;
        }

        /// <summary>
        /// 判断当前PC是否已经设置开机自动登录
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoLogonQualified()
        {
            bool _result = false;
            if (IsRegistryExist(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.autoAdminLogon) && 
                IsRegistryExist(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.defaultUserName) &&
                IsRegistryExist(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.defaultPassword))
            {
                if (GetRegistryData(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.autoAdminLogon) == "1" &&
                    GetRegistryData(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.defaultUserName).Length > 0 &&
                    GetRegistryData(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.defaultPassword).Length > 0)
                    _result = true;
                else _result = false;
            }
            else _result = false;
            return _result;
        }

        public static void SetAutoLogon()
        {
            if (IsRegistryExist(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.autoAdminLogon))
                SetRegistryData(Registry.LocalMachine, RegistryConst.AUTOLOGON, RegistryConst.autoAdminLogon, "1");
        }
        #endregion

        #region ComputerActionHelper
        /// <summary>
        /// 重启电脑
        /// </summary>
        public static void RebootComputer()
        {
            Process.Start("shutdown.exe", "-r -t 10");
        }

        public static void shutdownComputer()
        {
            Process.Start("shutdown.exe", "-s -t 0");
        }
        #endregion

        #region NodeHelper
        /// <summary>
        /// 递归遍历出当前根节点下的文件和文件夹
        /// </summary>
        /// <param name="root">根节点</param>
        /// <returns>封装好的节点信息类</returns>
        public static List<ReturnMessageFormat> OpenFolder(NodeFormat root)
        {
            List<ReturnMessageFormat> messageArray = new List<ReturnMessageFormat>();
            const int constNum = 10;

            string error = string.Empty;
            string path = root.path;
            root.files = new List<FileInforFormat>();
            root.folders = new List<FolderInforFormat>();

            string[] folders = System.IO.Directory.GetDirectories(path);
            string[] files = System.IO.Directory.GetFiles(path);

            foreach (string file in files)
            {
                FileInforFormat fileInfor = new FileInforFormat();
                try
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                    fileInfor.name = file.Split('\\').Last();
                    fileInfor.size = (int)(System.Math.Ceiling(fileInfo.Length / 1024.0));
                    fileInfor.lastChangeTime = fileInfo.LastWriteTime + "";
                    root.files.Add(fileInfor);
                } catch (Exception e)
                {
                    error += e.Message;
                }               
                
            }
            foreach (string folder in folders)
            {
                FolderInforFormat folderInfor = new FolderInforFormat();
                try
                {
                    folderInfor.subNum = System.IO.Directory.GetFileSystemEntries(folder).Length;
                    folderInfor.name = folder.Split('\\').Last();
                    root.folders.Add(folderInfor);
                } catch (Exception e)
                {
                    error += e.Message;
                }                
            }

            if ((root.folders.Count == 0 && root.files.Count == 0))
            {
                ReturnMessageFormat message = new ReturnMessageFormat();

                if (error.Equals(string.Empty))
                {
                    message.status = Order.success;
                    message.message = Order.folderAction_open_finish_message;
                    message.data = root;                    
                }
                else
                {
                    message.status = Order.failure;
                    message.message = error;
                    message.data = null;
                }               
                messageArray.Add(message);
            }
            else
            {
                if (root.folders.Count + root.files.Count <= constNum)
                {
                    ReturnMessageFormat message = new ReturnMessageFormat();
                    message.status = Order.success;
                    message.message = Order.folderAction_open_finish_message;
                    message.data = root;
                    messageArray.Add(message);
                }
                else
                {
                    while (root.folders.Count + root.files.Count > constNum)
                    {
                        ReturnMessageFormat message = new ReturnMessageFormat();
                        NodeFormat tempNode = new NodeFormat();
                        tempNode.folders = new List<FolderInforFormat>();
                        tempNode.files = new List<FileInforFormat>();
                        tempNode.path = path;
                        if (root.folders.Count >= constNum)
                        {
                            //tempNode.folders = new List<FolderInforFormat>();
                            //tempNode.files = new List<FileInforFormat>();
                            for (int i = 0; i < constNum; i++)
                            {
                                tempNode.folders.Add(root.folders[i]);
                            }
                            root.folders.RemoveRange(0, constNum);
                                                                           
                        }                        
                        else
                        {
                            
                            for (int i = 0; i < constNum - root.folders.Count; i++)
                            {
                                tempNode.files.Add(root.files[i]);
                            }
                            for (int i = 0; i < root.folders.Count; i++)
                            {
                                tempNode.folders.Add(root.folders[i]);
                            }
                            //tempNode.folders = root.folders;
                            int num1 = root.folders.Count;
                            int num2 = constNum - num1;
                            root.folders.RemoveRange(0, num1);                         
                            root.files.RemoveRange(0, num2);                          
                            
                        }
                        message.status = Order.success;
                        message.message = Order.folderAction_open_more_message;
                        message.data = tempNode;
                        messageArray.Add(message);
                    }

                    if (root.folders.Count + root.files.Count > 0)
                    {
                        ReturnMessageFormat message = new ReturnMessageFormat();
                        message.status = Order.success;
                        message.message = Order.folderAction_open_finish_message;
                        message.data = root;
                        messageArray.Add(message);
                    }
                }
                messageArray[messageArray.Count - 1].message = Order.folderAction_open_finish_message;
            }
           
            
            return messageArray;
        }

        /// <summary>
        /// 删除指定文件夹
        /// </summary>
        /// <param name="target">文件夹名称(绝对路径)(如："H:\\图片管理")</param>
        /// <returns>封装好的返回信息</returns>
        public static ReturnMessageFormat DeleteFolder(string target)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            if (System.IO.Directory.Exists(target))
            {
                try
                {
                    System.IO.Directory.Delete(target, true);
                    message.status = Order.success;
                    message.message = "";
                    message.data = null;
                }
                catch (System.IO.IOException e)
                {
                    message.status = Order.failure;
                    message.message = e.Message;
                    message.data = null;
                }
            }
            else
            {
                message.status = Order.failure;
                message.message = "";
                message.data = null;
            }
            return message;
        }

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="targetFile">文件名称(绝对路径)(如："H:\\图片管理\\haha.jpg")</param>
        /// <returns></returns>
        public static ReturnMessageFormat DeleteFile(string targetFile)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            if (System.IO.File.Exists(targetFile))
            {
                try
                {
                    System.IO.File.Delete(targetFile);
                    message.status = Order.success;
                    message.message = "";
                    message.data = null;
                }
                catch (System.IO.IOException e)
                {
                    message.status = Order.failure;
                    message.message = e.Message;
                    message.data = null;
                }
            }
            else
            {
                message.status = Order.failure;
                message.message = "";
                message.data = null;
            }
            return message;
        }

        /// <summary>
        /// 重命名指定文件
        /// </summary>
        /// <param name="targetFile">文件名称(绝对路径)(如："H:\\图片管理\\haha.jpg")</param>
        /// <param name="newName">haohao.jpg</param>
        /// <returns></returns>
        public static ReturnMessageFormat RenameFile(string targetFile, string newName)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            Computer myComputer = new Computer();
            try
            {
                myComputer.FileSystem.RenameFile(targetFile, newName);
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
            return message;
        }

        /// <summary>
        /// 重命名指定文件夹
        /// </summary>
        /// <param name="targetFile">文件名称(绝对路径)(如："H:\\图片管理")</param>
        /// <param name="newName">图片</param>
        /// <returns></returns>
        public static ReturnMessageFormat RenameFolder(string targetFolder, string newName)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            Computer myComputer = new Computer();
            try
            {
                myComputer.FileSystem.RenameDirectory(targetFolder, newName);
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
            return message;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="targetPath">要创建的文件夹路径(如："H:\\图片管理"，即表示在H盘创建图片管理文件夹)</param>
        /// <returns></returns>
        public static ReturnMessageFormat CreateFolder(string targetPath)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
                message.status = Order.success;
                message.message = "";
                message.data = null;
            }
            else
            {
                message.status = Order.failure;
                message.message = "要创建的文件夹已存在";
                message.data = null;
            }
            return message;
        }

        /// <summary>
        /// 复制文件到指定路径
        /// </summary>
        /// <param name="targetPath">目标路径(如："H:\\图片管理")</param>
        /// <param name="filePath">要复制的文件(如："D:\\图片管理\\bcb.jpg")</param>
        /// <returns></returns>
        public static ReturnMessageFormat CopyFile(string targetPath, string filePath)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            if (!System.IO.Directory.Exists(targetPath))
            {
                message.status = Order.failure;
                message.message = targetPath + "目标路径不存在";
                message.data = null;
            }
            else
            {
                try
                {
                    System.IO.File.Copy(filePath, targetPath + @"\" + filePath.Split('\\').Last(), false);
                    message.status = Order.success;
                    message.message = "";
                    message.data = null;
                }
                catch (Exception e)
                {
                    message.status = Order.failure;
                    message.message = e.Message;
                    message.data = null;
                    log.Info(string.Format("Copy文件异常：{0}", e.Message));
                    throw;
                }                
            }
            return message;
        }

        /// <summary>
        /// 复制文件夹到指定路径
        /// </summary>
        /// <param name="sourcePath">要复制的文件夹所在路径(如： "H:\\图片")</param>
        /// <param name="targetPath">目的路径(如： "D:\\新建文件夹")</param>
        /// <returns></returns>
        public static ReturnMessageFormat CopyFolder(string sourcePath, string targetPath)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            List<string> error = new List<string>();
            informationFormat.fileManagerFormat.TreeNode tree = new informationFormat.fileManagerFormat.TreeNode();
            tree.setFullName(sourcePath);
            tree.copyTo(targetPath, ref error);

            if (error.Count == 0)
            {
                message.status = Order.success;
                message.message = "";
                message.data = null;
            }
            else
            {
                string temp = string.Empty;
                foreach (string str in error)
                {
                    temp += ("错误信息" + str);
                }
                message.status = Order.failure;
                message.message = temp.Length > 2000 ? "复制文件夹出错" : temp;
                message.data = null;
            }
            return message;
        }

        /// <summary>
        /// 剪切文件到指定路径
        /// </summary>
        /// <param name="targetPath">目标路径(如："H:\\图片管理\")</param>
        /// <param name="filePath">要剪切的文件(如："D:\\图片管理\\bcb.jpg")</param>
        /// <returns></returns>
        public static ReturnMessageFormat CutFile(string targetPath, string filePath)
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            if (!System.IO.Directory.Exists(targetPath))
            {
                message.status = Order.failure;
                message.message = targetPath + "目标路径不存在";
                message.data = null;
            }
            else
            {
                try
                {                    
                    System.IO.File.Move(filePath, targetPath + filePath.Split('\\').Last());
                    message.status = Order.success;
                    message.message = "";
                    message.data = null;
                }
                catch (Exception e)
                {
                    message.status = Order.failure;
                    message.message = e.Message;
                    message.data = null;
                    throw;
                }
            }
            return message;
        }

        /// <summary>
        /// 剪切文件夹到指定路径
        /// </summary>
        /// <param name="targetPath">目标路径(如："H:\\图片管理")</param>
        /// <param name="folderPath">要剪切的文件夹(如："D:\\图片管理")</param>
        /// <returns></returns>
        public static ReturnMessageFormat CutFolder(string targetPath, string folderPath)
        {
            ReturnMessageFormat message = Action.CopyFolder(folderPath, targetPath);
            if (message.status == Order.success)
            {
                message = Action.DeleteFolder(folderPath);
            }            
            else
            {               
                message.status = Order.failure;
                message.message = folderPath + "不支持剪切";
                message.data = null;             
            }
            return message;
        }
        #endregion

        #region DiskHelper
        /// <summary>
        /// 获取当前PC除系统盘外的所有磁盘列表（仅支持固定磁盘、移动盘和网络磁盘）
        /// </summary>
        /// <returns></returns>
        public static ReturnMessageFormat GetDiskList()
        {
            ReturnMessageFormat message = new ReturnMessageFormat();
            List<DiskInforFormat> diskList = new List<DiskInforFormat>();

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo item in allDrives)
            {
                // !(item.Name.Contains(System.Environment.SystemDirectory.Split('\\').First()))
                //if (item.DriveType == DriveType.Fixed || item.DriveType == DriveType.Removable && item.IsReady)
                if (item.DriveType == DriveType.Fixed || item.DriveType == DriveType.Removable || item.DriveType == DriveType.Network && item.IsReady)
                {
                    DiskInforFormat disk = new DiskInforFormat();
                    disk.driveFormat = item.DriveFormat;
                    disk.driveType = item.DriveType.ToString();
                    disk.name = item.Name.Trim(':', '\\');
                    disk.rootDirectory = item.RootDirectory.ToString();
                    disk.totalSize = item.TotalSize / 1024 / 1024 / 1024;
                    disk.totalFreeSpace = item.TotalFreeSpace / 1024 / 1024 / 1024;
                    disk.volumeLabel = item.VolumeLabel;
                    if (item.Name.Contains(System.Environment.SystemDirectory.Split('\\').First()))
                    {
                        disk.driveType = "Fixed_SYS";
                    }
                    diskList.Add(disk);
                }
            }
            message.status = diskList.Count >= 1 ? Order.success : Order.failure;
            message.message = diskList.Count >= 1 ? "" : "当前PC除系统盘外无其他磁盘或移动盘";
            //Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //string naslongconnect = config.AppSettings.Settings["naslongconnect"].Value;
            //if (naslongconnect.Equals("ture")){
            //    message.message = config.AppSettings.Settings["nasdir"].Value;
            //}
            //else
            //{
            //    message.message = "";
            //}
            message.data = diskList;
            //config.Save(ConfigurationSaveMode.Modified);
            //System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            return message;
        }
        #endregion

        #region ApplicationHelper
        public static List<ReturnMessageFormat> GetApplicationList()
        {
            const int constNum = 10;
            List<ReturnMessageFormat> messageArray = new List<ReturnMessageFormat>();
            ReturnMessageFormat messageLast = new ReturnMessageFormat();
            List<ApplicationFormat> appList = new List<ApplicationFormat>();

            RegistryKey lmKey, uninstallKey, programKey;
            lmKey = Registry.LocalMachine;
            uninstallKey = lmKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] programKeys = uninstallKey.GetSubKeyNames();
           
            foreach (string keyName in programKeys)
            {
                ApplicationFormat app = new ApplicationFormat();           
                programKey = uninstallKey.OpenSubKey(keyName);
                if (programKey.GetValue("DisplayName") != null)
                {
                    app.displayName = programKey.GetValue("DisplayName").ToString();
                    if (programKey.GetValue("Publisher") != null)
                    {
                        if (programKey.GetValue("Publisher").ToString() == "Microsoft Corporation")
                            continue;
                        else
                            app.publisher = programKey.GetValue("Publisher").ToString();
                    }
                    else
                        app.publisher = "";
                    app.displayVersion = programKey.GetValue("DisplayVersion") != null ?
                       programKey.GetValue("DisplayVersion").ToString() : "";

                    appList.Add(app);                           
                }
            }

            while (appList.Count > constNum)
            {
                List<ApplicationFormat> temp = new List<ApplicationFormat>();
                for (int i = 0; i < constNum; i++)
                {
                    temp.Add(appList[i]);                    
                }
                ReturnMessageFormat messageTemp = new ReturnMessageFormat();
                messageTemp.status = Order.success;
                messageTemp.message = Order.appAction_get_more_message;
                messageTemp.data = temp;
                messageArray.Add(messageTemp);

                appList.RemoveRange(0, constNum);
            }
            messageLast.status = appList.Count >= 1 ? Order.success : Order.failure;
            messageLast.message = appList.Count >= 1 ? Order.appAction_get_finish_message : "当前PC系统除Microsoft Corporation应用外无其他应用";
            messageLast.data = appList;
            messageArray.Add(messageLast);

            return messageArray;
        }
        #endregion

        #region SharedFileHelper

        /// <summary>
        /// 将当前文件添加到分享文件的list中，并将此list进行json序列化后按二进制文件流的形式存到指定路径下
        /// </summary>
        /// <param name="file">要添加的文件</param>
        /// <param name="list">已有的文件列表</param>
        /// <param name="binaryPath">存储的二进制文件的完整路径</param>
        public static ReturnMessageFormat AddSharedFile(ref SharedFilePathFormat file, ref List<SharedFilePathFormat> list, ref string binaryPath)
        {
            if (list == null)
                list = new List<SharedFilePathFormat>();
            list.Add(file);            
            FileStream fs = new FileStream(binaryPath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            string str = JsonHelper.SerializeObject(list);
            byte[] byteStrToWrite = Encoding.UTF8.GetBytes(str);
            bw.Write(byteStrToWrite);
            bw.Flush();
            bw.Close();
            fs.Close();

            ReturnMessageFormat message = new ReturnMessageFormat();
            message.status = Order.success;
            message.message = "";
            message.data = null;

            return message;
        }
        /// <summary>
        /// 读取指定二进制文件(分享文件)的信息, 并解析到list中
        /// </summary>
        /// <param name="binaryPath">二进制文件的完整路径</param>
        /// <param name="list">已分享文件的列表</param>
        public static void readSharedFileAndParse(ref string binaryPath, ref List<SharedFilePathFormat> list)
        {
            if (File.Exists(binaryPath))
            {
                FileStream fs = new FileStream(binaryPath, FileMode.Open);
                int size = (int)fs.Length;                
                BinaryReader br = new BinaryReader(fs);
                byte[] array = br.ReadBytes(size);
                string fileListStr = Encoding.UTF8.GetString(array, 0, size);
                list = JsonHelper.DeserializeJsonToObject<List<SharedFilePathFormat>>(fileListStr);
                br.Close();
                fs.Close();
            }
        }
        #endregion

    }
}

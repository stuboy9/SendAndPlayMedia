
using AreaParty.command;
using log4net;
using Newtonsoft.Json;
using node;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.util
{
    class JAVAUtil
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 添加文件夹资源到http服务器
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="isDir"></param>
        public static void AddSourceToHTTP(string dirPath, Boolean isDir = true)
        {
            Dictionary<string, string> param = null;
            Command command = null;

            try{
                if (isDir == false)
                {
                    param = new Dictionary<string, string>();
                    param.Add("path", dirPath);
                    command = new Command("PC", "AddDirHTTP", param);
                }
                else
                {
                    param = new Dictionary<string, string>();
                    param.Add("path", dirPath);
                    command = new Command("PC", "AddDirHTTP", param);

                    if (File.Exists(dirPath) || Directory.Exists(dirPath))
                    {
                        if (Directory.Exists(dirPath))
                        {
                            DirectoryInfo Folder = new DirectoryInfo(dirPath);
                            foreach (FileInfo file in Folder.GetFiles())
                            {
                                Console.WriteLine("file fullname is:{0}", file.FullName);
                                Node node = new Node(file.FullName);
                                NodeContainer.addNode(node);
                                Console.WriteLine("node.id is:{0}", node.id);
                            }
                        }
                        else
                        {
                            FileInfo file = new FileInfo(dirPath);
                            Console.WriteLine("file fullname is:{0}", file.FullName);
                            Node node = new Node(file.FullName);

                            NodeContainer.addNode(node);
                            Console.WriteLine("node.title is:{0}", node.title);
                        }
                        Console.WriteLine("status:200");
                        log.Info(string.Format("路径{0}的资源添加成功", dirPath));

                    }
                    else
                    {
                        Console.WriteLine("status:404");
                        log.Info(string.Format("找不到目标路径"));
                    }
                }
            }
            catch (Exception e)
            {
                log.Info(string.Format("目标路径文件添加错误：{0}", e.Message));
            }
            
        }
        public static void AddAlltoHttp(string dstPath)
        {
            try
            {
                if (!System.IO.Directory.Exists(dstPath))
                {
                    return;
                }
                
                Stack<string> dirs = new Stack<string>(200);
                
                dirs.Push(dstPath);
                while (dirs.Count > 0)
                {
                    string currDir = dirs.Pop();
                    AddSourceToHTTP(currDir);
                    string[] subDirs;
                    try
                    {
                        subDirs = System.IO.Directory.GetDirectories(currDir);
                    }
                    catch (System.UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    
                    foreach (string str in subDirs)
                        dirs.Push(str);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                log.Info(string.Format("向服务器添加路径失败：{0}", e.Message));
            }
            catch (ArgumentException e)
            {
                return;
            }
        }
    }
}

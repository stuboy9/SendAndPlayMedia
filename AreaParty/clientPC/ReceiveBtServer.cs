using AreaParty.info;
using http;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace client
{

	public class ReceiveBtServer
	{
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string receiveFileDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\AreaParty\\BtFiles";
        private static Socket serversocket;
        private static string ip = AreaParty.Program.ip;
        private static Thread mythread, myfileservthread_thread, thread;
        private static FileServThread myfileservthread;
        public ReceiveBtServer()
        {
        }

        public void run()//启动文件存储服务端 
        {
            try
            {
                new FileServer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
        public class FileServer
        {
            public FileServer()
            {
                try
                {
                    s.op("服务端启动......");
                    server();
                }catch(Exception e)
                {
                    log.Info(string.Format("BT文件接收服务器启动失败：{0}", e.Message));
                }
            }
            public void server()
            {
                IPEndPoint hostEP = new IPEndPoint(IPAddress.Parse(ip), ConfigResource.BTRECEIVE_PORT);
                serversocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serversocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                serversocket.Bind(hostEP);
                serversocket.Listen(1);
                try
                {
                    thread = new Thread(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                try
                                {
                                    Socket socket = serversocket.Accept();
                                    mythread = new Thread(() =>
                                                                {
                                                                //Console.WriteLine("accept 等待连接");
                                                                myfileservthread = new FileServThread(socket);
                                                                //Console.WriteLine("accept 启动");
                                                                myfileservthread_thread = new Thread(myfileservthread.run)
                                                                    {
                                                                        IsBackground = true
                                                                    };
                                                                    myfileservthread_thread.Start();
                                                                });
                                    mythread.IsBackground = true;
                                    mythread.Start();
                                }
                                catch (IOException e)
                                {
                                    Console.WriteLine(e.Message);
                                    stop();

                                }
                            }
                        }catch(IOException e)
                        {
                            stop();
                        }
                    });
                    thread.IsBackground = true;
                    thread.Start();

                }catch(IOException e)
                {
                    stop();
                    //GC.Collect();
                }
                
                
            }
            public virtual void stop()
            {
                try
                {
                    serversocket.Close();
                    thread.Join();
                }
                catch (IOException)
                {
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        

        private class FileServThread
        {
            private Socket sock;
            public FileServThread(Socket sock)
            {
                this.sock = sock;
            }
            public virtual void run()
            {
                string ip = ((System.Net.IPEndPoint)sock.RemoteEndPoint).Address.ToString();
                s.op("开启新线程接收来自客户端IP: " + ip + " 的文件");
                try
                {
                    //Socket myClientSocket = sock as Socket;
                    int totalSize;//文件大小
                    long len = 0;
                    byte[] buffer = new byte[512];
                    string sendClientName;
                    string receiveFilePath;
                    
                    if (!Directory.Exists(receiveFileDir))
                    {
                        Directory.CreateDirectory(receiveFileDir);
                    }

                    string filename = getFileName(sock);

                    receiveFilePath = receiveFileDir +"\\"+ filename;

                    
                    if (File.Exists(receiveFilePath))
                    {
                        SendMessageToAPP(sock, "file exist");
                        sock.Close();
                        return;
                    }
                    FileStream fs = new FileStream(receiveFilePath, FileMode.Create, FileAccess.Write);
                    try
                    {
                        while ((totalSize = sock.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                        {
                            fs.Write(buffer, 0, totalSize);
                            len += totalSize;
                        }
                        //sleep(1000);
                        SendMessageToAPP(sock, "上传成功!");
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("异常！：{0}", e.Message);
                    }
                    Console.WriteLine("接收到文件大小：{0}", len);
                    fs.Dispose();
                    fs.Close();
                    sock.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}异常！错误：{1}", ip, e.Message);
                }

            }

            private static string getFileName(Socket mySocket)
            {
                string mes;
                int bufsize = 1024;
                byte[] buffer = new byte[bufsize];
                byte[] buf = new byte[bufsize];
                List<byte> head_mes = new List<byte>();
                int rlen = mySocket.Receive(buffer);
                if (buffer == null)
                {
                    return null;
                }
                try
                {
                    while (rlen > 0)
                    {
                        for (int j = 0; j < rlen; j++)
                        {
                            head_mes.Add(buffer[j]);
                        }
                        if (rlen < buffer.Length)
                        {
                            break;
                        }

                    }
                }
                catch
                {

                }
                if (head_mes.Count > 0)
                {
                    buf = head_mes.ToArray();
                }

                MemoryStream hbis = new MemoryStream(buf, 0, rlen);
                StreamReader hin = new StreamReader(hbis);


                string filename = HttpUtility.UrlDecode(hin.ReadLine(), Encoding.GetEncoding("UTF-8"));
                hin.Dispose();
                hbis.Dispose();
                string filepath = receiveFileDir + "\\" + filename;
                //FileInfo fileInfo = new FileInfo(filepath);
                if (File.Exists(filepath))
                {
                    mes = "存在同名文件或获取文件失败,服务端断开连接!";
                    SendMessageToAPP(mySocket, mes);
                    
                }
                else
                {
                    mes = "FileSendNow";
                    SendMessageToAPP(mySocket, mes);
                }

                //if (FileExist(filename))
                //{
                //    mes = "存在同名文件或获取文件失败,服务端断开连接!";
                //    SendMessageToAPP(mySocket, mes);
                //}
                //else
                //{
                //    mes = "FileSendNow";
                //    SendMessageToAPP(mySocket, mes);
                //}

                return filename;


            }
            /*判断目录下是否存在相同文件，暂时不用
            private static bool FileExist(string file)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(receiveFileDir);
                bool FileExist = FindFile(directoryInfo,file);
                return FileExist;
            }

            private static bool FindFile(DirectoryInfo di,string title)
            {
                FileInfo[] fis = di.GetFiles();
                for (int i = 0; i < fis.Length; i++)
                {
                    if (fis[i].Name.Equals(title))
                    {
                        return true;
                    }
                    Console.WriteLine("文件：" + fis[i].FullName);
                }
                DirectoryInfo[] dis = di.GetDirectories();
                for (int j = 0; j < dis.Length; j++)
                {
                    Console.WriteLine("目录：" + dis[j].FullName);
                    FindFile(dis[j],title);
                }
                return false;
            }
            */
            


        }
        public static void SendMessageToAPP(Socket socket, string mes)
        {
            NetworkStream networkStream = new NetworkStream(socket);
            byte[] mess = System.Text.Encoding.UTF8.GetBytes(mes);
            networkStream.Write(mess, 0, mess.Length);
        }
    }
    
}


internal class s
	{
		public static void op(object obj) //打印
		{
			Console.WriteLine(obj);
		}
	}


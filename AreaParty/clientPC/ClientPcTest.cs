using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace client
{
	public class ClientPcTest
	{
		public void MainStart()
		{
            try
            {
                BinaryReader br;
                BinaryWriter bw;
                //Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress remoteIP = IPAddress.Parse("127.0.0.1");//远程主机IP
                int remotePort = 13456;//远程主机端口
                TcpClient tcpClient = new TcpClient();
                string sendString = "{\"name\": \"JC\",\"command\": \"login\",\"param\": {\"name\": \"huguyong\", \"password\": \"123456\"}}";//获取要发送的字符串
                //byte[] sendData = Encoding.Default.GetBytes(sendString);//获取要发送的字节数组
                //byte[] receiveData = new byte[1024];
                //String str;
                try
                {
                    tcpClient.Connect(new IPEndPoint(remoteIP, remotePort)); //配置服务器IP与端口
                    //clientSocket.Send(Encoding.Default.GetBytes(sendString));
                    NetworkStream clientStream = tcpClient.GetStream();
                    bw = new BinaryWriter(clientStream);
                    bw.Write(sendString);
                    while (true)
                    {
                        //int len = clientSocket.Receive(receiveData);
                        //str = Encoding.Default.GetString(receiveData, 0,len);
                        br = new BinaryReader(clientStream);
                        string receive = br.ReadString();
                        Console.WriteLine("我的天");
                        Console.WriteLine(receive);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("连接超时，服务器没有响应！");//连接失败
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            //			(new Thread(() =>
            //		{

            //			// TODO Auto-generated method stub
            //			try
            //			{
            //				Socket socket = new Socket("localhost", 13456);
            //				PrintStream ps = new PrintStream(socket.OutputStream);
            //				ps.println("{\"name\": \"JC\",\"command\": \"login\",\"param\": {\"name\": \"huguyong\", \"password\": \"123456\"}}");

            ////					Socket socket2 = new Socket("localhost", 13456);
            ////					PrintStream ps2 = new PrintStream(socket2.getOutputStream());
            ////					ps2.println("{\"name\": \"JC\",\"command\": \"login\",\"param\": {\"name\": \"kirinna\", \"password\": \"aaaaaa\"}}");

            ////					while(true){
            ////						InputStream is = socket.getInputStream();
            ////						//BufferedReader br = new BufferedReader(new InputStreamReader(socket.getInputStream()));
            ////						String s = "";
            ////						try {
            ////							s = new String(readStream(is));
            ////						} catch (Exception e) {
            ////							// TODO Auto-generated catch block
            ////							e.printStackTrace();
            ////							//continue;
            ////						}
            ////						System.out.println(s + "--");
            ////					}
            //				socket.close();
            //			}
            //			catch (UnknownHostException e)
            //			{
            //				// TODO Auto-generated catch block
            //				Console.WriteLine(e.ToString());
            //				Console.Write(e.StackTrace);
            //			}
            //			catch (IOException e)
            //			{
            //				// TODO Auto-generated catch block
            //				Console.WriteLine(e.ToString());
            //				Console.Write(e.StackTrace);
            //			}
            //		})).Start();
        }
	//	public static class FileSize{
	//		private String fileSize;
	//		private String fileName;
	//	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] readStream(java.io.InputStream inStream) throws Exception
		//public static sbyte[] readStream(System.IO.Stream inStream)
		//{
		//	System.IO.MemoryStream outSteam = new System.IO.MemoryStream();
		//	sbyte[] buffer = new sbyte[1024];
		//	int len = -1;
		//	while ((len = inStream.Read(buffer, 0, buffer.Length)) != -1)
		//	{
		//		outSteam.Write(buffer, 0, len);
		//	}
		//	outSteam.Close();
		//	inStream.Close();
		//	Console.WriteLine(outSteam.toByteArray());
		//	return outSteam.toByteArray();
		//}
	}

}
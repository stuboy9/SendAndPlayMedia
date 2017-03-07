using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SendAndPlayMedia
{
    class DLNA
    {
        public string SendToJava(string msg)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(new IPEndPoint(ip, 9816));
            Console.WriteLine("连接成功");
            byte[] result = new byte[1024];
            serverSocket.Send(Encoding.UTF8.GetBytes(msg));
            int receiveNumber = serverSocket.Receive(result);
            string rev = Encoding.UTF8.GetString(result, 0, receiveNumber);
            Console.WriteLine(rev);
            return rev;
        } 
    }
}

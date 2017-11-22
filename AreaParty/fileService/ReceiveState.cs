using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.fileServece
{
    public class ReceiveState
    {
        private long haveReceiveBytes;// 已经接收到的字节数
        private int numReceivePackets;// 每一次接收到的数据包数
        private int numResendPackets;// 要求重传的数据包数
        private long havaReceivefileLength;// 已经接收的文件长度
        private long fileLength;// 文件长度
        private int numSendWindow;// 发送窗口大小
        private long existFileLength;// 断电续传接收方已经接收到的文件长度
        private String MD5Str;

        public ReceiveState()
        {
            this.HaveReceiveBytes = 0;
            this.NumReceivePackets = 0;
            this.NumResendPackets = 0;
            this.HavaReceivefileLength = 0;
            this.FileLength = 0;
            this.NumSendWindow = 50;
            this.ExistFileLength = 0;
        }

        public long HaveReceiveBytes { get => haveReceiveBytes; set => haveReceiveBytes = value; }
        public int NumReceivePackets { get => numReceivePackets; set => numReceivePackets = value; }
        public int NumResendPackets { get => numResendPackets; set => numResendPackets = value; }
        public long HavaReceivefileLength { get => havaReceivefileLength; set => havaReceivefileLength = value; }
        public long FileLength { get => fileLength; set => fileLength = value; }
        public int NumSendWindow { get => numSendWindow; set => numSendWindow = value; }
        public long ExistFileLength { get => existFileLength; set => existFileLength = value; }
        public string MD5Str1 { get => MD5Str; set => MD5Str = value; }

        public void AddHaveReceiveBytes(long receiveBytes)
        {
            haveReceiveBytes += receiveBytes;
        }

        public void AddNumResendPackets()
        {
            numResendPackets++;
        }
    }
}

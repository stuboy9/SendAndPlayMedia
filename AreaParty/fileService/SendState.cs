using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.fileServece
{
    public class SendState
    {
        private long haveSendBytes;// 已经发送的字节数
        private int numSendPackets;// 每一次发送到的数据包数
        private int numResendPackets;// 要求重传的数据包数
        private long havaSendfileLength;// 已经传输的文件长度
        private long fileLength;// 文件长度
        private int waitTime;// 延迟时间
        private int numSendWindow;// 发送窗口大小
        private long existFileLength;// 断点续传接收方已经接收到的文件长度
        private String MD5Str;

        public SendState()
        {
            HaveSendBytes = 0;
            NumSendPackets = 0;
            NumResendPackets = 0;
            HavaSendfileLength = 0;
            FileLength = 0;
            WaitTime = 1;
            NumSendWindow = 50;
            ExistFileLength = 0;
        }

        public long HaveSendBytes { get => haveSendBytes; set => haveSendBytes = value; }
        public int NumSendPackets { get => numSendPackets; set => numSendPackets = value; }
        public int NumResendPackets { get => numResendPackets; set => numResendPackets = value; }
        public long HavaSendfileLength { get => havaSendfileLength; set => havaSendfileLength = value; }
        public long FileLength { get => fileLength; set => fileLength = value; }
        public int WaitTime { get => waitTime; set => waitTime = value; }
        public int NumSendWindow { get => numSendWindow; set => numSendWindow = value; }
        public long ExistFileLength { get => existFileLength; set => existFileLength = value; }
        public string MD5Str1 { get => MD5Str; set => MD5Str = value; }

        public void AddNumResendPackets()
        {
            this.NumResendPackets++;
        }

        public void AddNumHaveSendBytes(long SendBytes)
        {
            haveSendBytes += SendBytes;
        }
    }
}

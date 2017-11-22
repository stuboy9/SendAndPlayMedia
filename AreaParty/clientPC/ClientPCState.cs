using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.clientPC
{
    /// <summary>
    /// 记录打洞状态及文件传输状态
    /// </summary>
    public class ClientPCState
    {
        private string id;
        private string senderId;
        private string receiverId;
        private string senderIp;
        private string receiverIp;
        private int senderPort;
        private int receiverPort;

        private string fileDate;
        private string filePath;
        private string fileSize;
        private string fileType;
        private string fileTempPath;
        private long fileOffset;

        private int transState;
        private int fileState;
        private int punchState;

        public ClientPCState(string id)
        {
            this.Id = id;
        }
        public string Id { get => id; set => id = value; }
        public string SenderId { get => senderId; set => senderId = value; }
        public string ReceiverId { get => receiverId; set => receiverId = value; }
        public string SenderIp { get => senderIp; set => senderIp = value; }
        public string ReceiverIp { get => receiverIp; set => receiverIp = value; }
        public int SenderPort { get => senderPort; set => senderPort = value; }
        public int ReceiverPort { get => receiverPort; set => receiverPort = value; }
        public string FileDate { get => fileDate; set => fileDate = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public string FileSize { get => fileSize; set => fileSize = value; }
        /// <summary>
        /// sender = 1 
        /// receiver = 2
        /// </summary>
        public int TransState { get => transState; set => transState = value; }
        /// <summary>
        /// noSuchFile = 1,
        ///  fileExist = 2,
        ///  tempExist = 3,
        ///  transing = 4,
        ///  transOver = 5,
        ///  fileException = 0,
        /// </summary>
        public int FileState { get => fileState; set => fileState = value; }
        /// <summary>
        /// stop = 1,
        /// readyStart = 2,
        /// Start = 3,
        /// buildUDP = 4,
        /// buildTCP = 5,
        /// stopTCP = 6,
        /// punchSuccess = 7,
        /// punchException = 0,
        /// </summary>
        public int PunchState { get => punchState; set => punchState = value; }
        public string FileType { get => fileType; set => fileType = value; }
        public string FileTempPath { get => fileTempPath; set => fileTempPath = value; }
        public long FileOffset { get => fileOffset; set => fileOffset = value; }

        //private int FileState
        //{
        //    noSuchFile = 1,
        //    fileExist = 2,
        //    tempExist = 3,
        //    transing = 4,
        //    transOver = 5,
        //    fileException = 0,
        //};

        //public enum PunchState
        //{
        //    stop = 1,
        //    readyStart = 2,
        //    Start = 3,
        //    buildUDP = 4,
        //    buildTCP = 5,
        //    stopTCP = 6,
        //    punchSuccess = 7,
        //    punchException = 0,
        //};

    }
}

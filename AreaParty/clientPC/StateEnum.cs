using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.clientPC
{
    class StateEnum
    {
        public enum State : int
        {
            /// <summary>
            /// 1-30位文件状态
            /// </summary>
            File_NoSuchFile = 1,
            File_FileExist = 2,
            File_TempExist = 3,
            File_Transing = 4,
            File_TransOver = 5,
            File_FileException = 0,
            /// <summary>
            /// 30-60为打洞状态
            /// </summary>
            Punch_Stop = 31,
            Punch_ReadyStart = 32,
            Punch_Start = 33,
            Punch_BuildUDP = 34,
            Punch_BuildTCP = 35,
            Punch_StopTCP = 36,
            Punch_PunchSuccess = 37,
            Punch_PunchException = 30,
            /// <summary>
            /// 91 92 为发送 接收状态
            /// </summary>
            Send = 91,
            Receive = 92,
        }
    }
}

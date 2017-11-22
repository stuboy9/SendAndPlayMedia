using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AreaParty.info
{
    class ConfigResource
    {
        public const int MYSERVER_PORT = 18888;//本程序接受命令端口
        public const int BROADCAST_PORT = 19321;//接受广播端口
        public const int HTTP_PORT = 8634;//9816;//http服务器端口
        public const int BTRECEIVE_PORT = 10003;
        public const int HOLE_PORT = 13456;//clientPC端口
        public const int PCINFO_PORT = 8888;//transferinformation
        public const int KEYBOARD = 23568;//mouseandserver端口
        public const string SERVER_IP = "119.23.12.116"; //请求软件白名单
        public const int SERVER_PORT = 3333;//请求软件白名单
        public const string SERVER_2_IP = "119.23.74.220";//报告统计信息地址和请求版本跟新
        public const int SERVER_2_PORT = 10000;//报告统计信息地址
        public const int UPDATE_PORT = 10001;//请求版本更新端口

        public bool THREAD_END = false;

        public static string GetRDP_PDF_PATH()
        {
            return Application.StartupPath+"\\PDF\\RDP配置.pdf";
        }
        public static string GetGAME_PDF_PATH()
        {
            return Application.StartupPath + "\\PDF\\游戏串流配置.pdf";
        }
        public static string GetSCREEN_PDF_PATH()
        {
            return Application.StartupPath + "\\PDF\\屏幕配置.pdf";
        }
        public static string GetLONGCENNECT_PDF_PATH()
        {
            return Application.StartupPath + "\\PDF\\长连接登录设置.pdf";
        }
        public static string GetNAS_PDF_PATH()
        {
            return Application.StartupPath + "\\PDF\\NAS连接登录设置.pdf";
        }
        public static string GetSOFTWAREUSE_PDF_PATH()
        {
            return Application.StartupPath + "\\PDF\\软件使用设置.pdf";
        }
    }
}

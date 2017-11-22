using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    class RegistryConst
    {
        // 设置自启动时的注册表路径
        public const string REGISTRY_RUN = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        // 设置开机自动登录注册表路径
        public const string AUTOLOGON = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        public const string autoAdminLogon = "AutoAdminLogon";
        public const string defaultUserName = "DefaultUserName";
        public const string defaultPassword = "DefaultPassword";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.info.tv
{
    class DeviceItem
    {
        public string uuid;  //非dlna的uuid
        public string name;  //手机上显示的设备名
        public string type;    //“online” or “offline”  “offline”指未安装或打开tv端软件的设备
        public string ip;      //用于与tv端交互及指定rdp设备
        public Boolean dlnaOk;//dlna是否正常工作
        public Boolean miracastOk;//miracast是否正常工作
        public Boolean rdpOk;//rdp是否正常工作
        public string deviceName;  //用于指定dlna设备
        public string screen;  //用于指定miracast设备
        public DeviceItem(string uuid, string name, string type, string ip, Boolean dlnaOk, Boolean miracastOk, Boolean rdpOk,string deviceName,string screen)
        {
            this.uuid = uuid;
            this.name = name;
            this.type = type;
            this.ip = ip;
            this.dlnaOk = dlnaOk;
            this.miracastOk = miracastOk;
            this.rdpOk = rdpOk;
            this.deviceName = deviceName;
            this.screen = screen;
        }
        public DeviceItem(TVInfo tv,string deviceName,string screen)
        {
            this.uuid = tv.uuid;
            this.name = tv.name;
            this.type = tv.type;
            this.ip = tv.ip;
            this.dlnaOk = tv.dlnaOk;
            this.miracastOk = tv.miracastOk;
            this.rdpOk = tv.rdpOk;
            this.deviceName = deviceName;
            this.screen = screen;
        }

    }
    class DeviceLibrary
    {
        public string name = "deviceLibrary";
        public List<DeviceItem> value = new List<DeviceItem>();
    }
}

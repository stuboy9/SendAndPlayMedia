using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using Test.info.miracast;
using System.IO.Pipes;
using System.Security.Principal;
using System.Security.AccessControl;
using SendAndPlayMedia.util;
using Windows.System;
using System.Threading.Tasks;

namespace SendAndPlayMedia
{
    enum DisplayMode
    {
        Internal,
        External,
        Extend,
        Duplicate
    }
    struct CurrentScreen
    {
        public string name;
        public int index;
        public System.Windows.Forms.Screen screen;

        public CurrentScreen(string name,int index, System.Windows.Forms.Screen screen) : this()
        {
            this.name = name;
            this.index = index;
            this.screen = screen;
        }
    }
    class UWPCommand
    {
        public string name = "UWP";
        public string command = "";
        public Dictionary<string, string> param = new Dictionary<string, string>();
        
    }
    class UWPResponse
    {
        public string status = "200";
        public string type = "data";
        public List<string> value = new List<string>();
    }
        class Projection
    {
        private static readonly Object obj = new Object();//锁对象,同一时间只能查询或者投影操作。
        //用户视频文件夹目录
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos);
        static string[] device = null;
        public static CurrentScreen current = new CurrentScreen (null,0,null );
        public Boolean flag = true;
        public Dictionary<string, int> mapScreen = new Dictionary<string, int>();
        static readonly Uri uri = new Uri("com.yi.myprojection:DoSomething?With=This");
        const string TargetPackageFamilyName = "06e0587e-3eeb-433c-a44a-b5145c6efc47_yrvdh1zv4g43y";
        public string CommunicateToUWP(string command)
        {
            string message = "";
            try
            {
                using (var pipe = new NamedPipeServerStream("mypipe", PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, HandleInheritability.None, PipeAccessRights.ChangePermissions))
                {
                    PipeSecurity ps = pipe.GetAccessControl();
                    PipeAccessRule clientRule = new PipeAccessRule(
                        new SecurityIdentifier("S-1-15-2-1"), // All application packages
                        PipeAccessRights.ReadWrite,
                        AccessControlType.Allow);
                    PipeAccessRule ownerRule = new PipeAccessRule(
                        WindowsIdentity.GetCurrent().Owner,
                        PipeAccessRights.FullControl,
                        AccessControlType.Allow);
                    ps.AddAccessRule(clientRule);
                    ps.AddAccessRule(ownerRule);
                    pipe.SetAccessControl(ps);
                    pipe.WaitForConnection();
                    using (var sr = new StreamReader(pipe, Encoding.UTF8))
                    {

                        StreamWriter sw = new StreamWriter(pipe, Encoding.UTF8);
                        sw.WriteLine(command);
                        sw.Flush();

                         message = sr.ReadLine();
                        //在此处处理App写入命名管道的内容
                        Console.WriteLine(message);
                        pipe.WaitForPipeDrain();
                    }
                }
            }
            catch (Exception e)
            {
                message = "";
            }

            return message;
        }
        public string GetDeviceList()
        {
            lock (obj)
            {
                try
                {
                    //String str = "";
                    //lock (obj)
                    //{
                    //    if (!File.Exists(path + "\\name.txt"))
                    //    {
                    //        FileStream fs = new FileStream(path + "\\name.txt", FileMode.Create);
                    //        StreamWriter sw = new StreamWriter(fs);
                    //        sw.WriteLine("screen");
                    //        sw.Close();
                    //        fs.Close();
                    //    }
                    //    string filepath = path + "\\server.txt";
                    //    int times = 0;
                    //    Console.WriteLine(path + "\\server.txt");
                    //    while (!System.IO.File.Exists(filepath))
                    //    {
                    //        Thread.Sleep(1000);
                    //        if (times++ > 10)
                    //            throw new Exception("超时。。。");
                    //    }
                    //    //if(File.Exists("C:\\Users\\HOME\\Videos\\name.txt"))
                    //    //    File.Delete("C:\\Users\\HOME\\Videos\\name.txt");
                    //    Thread.Sleep(1000);//等待数据写入，防止读写冲突
                    //    str = File.ReadAllText(filepath);
                    //    File.Delete(filepath);
                    //}
                    ////str = "本地桌面," + str;
                    //str = str.TrimEnd(',');
                    //string[] strs = str.Split(',');
                    //for (int i = 0; i < strs.Length; i++)
                    //{
                    //    mapScreen.Clear();
                    //    mapScreen.Add(strs[i], i);
                    //}
                    UWPCommand c = new UWPCommand();
                    c.name = "UWP";
                    c.command = "GET";
                    c.param = new Dictionary<string, string>();
                    //Console.WriteLine(JsonConvert.SerializeObject(c));
                    UWPResponse msg = JsonConvert.DeserializeObject<UWPResponse>(CommunicateToUWP(JsonConvert.SerializeObject(c)));
                    device = new string[msg.value.Count];
                    MiracastLibrary library = new MiracastLibrary(new List<Test.info.miracast.Screen>());
                    for (int i = 0; i < msg.value.Count; i++)
                    {
                        //Console.WriteLine(strs[i]);
                        Test.info.miracast.Screen screen = new Test.info.miracast.Screen(msg.value.ElementAt(i), i);
                        device[i] = msg.value.ElementAt(i);
                        Console.WriteLine(device[i]);
                        library.value.Add(screen);
                    }
                    string json = JsonConvert.SerializeObject(library);
                    //StringWriter stringWriter = new StringWriter();
                    //JsonWriter writer = new JsonTextWriter(stringWriter);
                    //writer.WriteStartObject();
                    //writer.WritePropertyName("miracast");
                    //writer.WriteStartArray();
                    //foreach(string t in strs){
                    //    writer.WriteValue(t);
                    //}
                    //writer.WriteEndArray();
                    //writer.WriteEndObject();

                    //str = str + "\n";
                    return json;
                }
                catch (Exception e)
                {
                    Console.WriteLine("读取miracast设备出错");
                    Console.WriteLine(e);
                    return JsonConvert.SerializeObject(new MiracastLibrary(new List<Test.info.miracast.Screen>()));
                }

            }
        }
        public Boolean TestConnection()
        {
                System.Windows.Forms.Screen[] scr = System.Windows.Forms.Screen.AllScreens;
                
                foreach(System.Windows.Forms.Screen s in scr)
                {
                    if (current.screen!=null&&current.screen.Equals(s)) return true;
                }
                    current.name = null;
                    current.index = 0;
                    current.screen = null;
            return false;
        }
        public Boolean TestConnection(int index)
        {
            System.Windows.Forms.Screen[] scr = System.Windows.Forms.Screen.AllScreens;
            //Console.WriteLine("当前屏幕{0},选择投影屏幕{1}", current.screen.DeviceName,);
            //foreach (System.Windows.Forms.Screen s in scr)
            //{
            //    if (current.screen != null && current.screen.Equals(s)) return true;
            //}
            if (current.screen != null && current.screen.Equals(scr.Last()) && scr.Count() > 0) return true;
            current.name = "";
            current.index = 0;
            current.screen = null;
            return false;
        }
        public void SelectProjectionDevice(int index)
        {
            if (device == null) GetDeviceList();
            if (TestConnection(index)) return;
            FileStream  fs = new FileStream(path + "\\client.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(index);
            sw.Close();
            fs.Close();

            Thread.Sleep(2000);
            ClearMiracastPopup();
            Thread.Sleep(10000);
            SetDisplayMode(DisplayMode.Extend);
            //Console.WriteLine(device.Last());
            current.name = device[index];
            current.index = index;
            current.screen = System.Windows.Forms.Screen.AllScreens.Last();
        }
        public void SelectProjectionDevice(string name)
        {
            lock (obj)
            {
                if (device == null) GetDeviceList();
                if (TestConnection(0)) return;
                //FileStream fs = new FileStream(path + "\\client.txt", FileMode.Create);
                //StreamWriter sw = new StreamWriter(fs);
                //sw.WriteLine(mapScreen[name]);
                //sw.Close();
                //fs.Close();

                UWPCommand c = new UWPCommand();
                c.name = "UWP";
                c.command = "PLAY";
                c.param = new Dictionary<string, string>();
                c.param.Add("render", name);
                if (!device.Contains(name)) GetDeviceList();
                UWPResponse r = JsonConvert.DeserializeObject<UWPResponse>(CommunicateToUWP(JsonConvert.SerializeObject(c)));
                if (r.status == "404") return;
                else
                {
                    ClearMiracastPopup();
                    Thread.Sleep(2000);
                    SetDisplayMode(DisplayMode.Extend);
                }

                //Console.WriteLine(device.Last());
                current.name = name;
                current.screen = System.Windows.Forms.Screen.AllScreens.Last();
            }
            
        }
        public void CloseProjection()
        {
            SetDisplayMode(DisplayMode.External);
            current.name = null;
            current.index = 0;
            current.screen = null;
        }
        //设置投影模式，external:仅第二屏幕; internal:仅电脑屏幕; extend:扩展; clone:复制;
        public  void SetDisplayMode(DisplayMode mode)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "DisplaySwitch.exe";
            switch (mode)
            {
                case DisplayMode.External:
                    proc.StartInfo.Arguments = "/external";
                    break;
                case DisplayMode.Internal:
                    proc.StartInfo.Arguments = "/internal";
                    break;
                case DisplayMode.Extend:
                    proc.StartInfo.Arguments = "/extend";
                    break;
                case DisplayMode.Duplicate:
                    proc.StartInfo.Arguments = "/clone";
                    break;
            }
            proc.Start();
        }
        public static async Task<bool> IsInstallUWP()
        {
            var supportStatus = await Launcher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri, TargetPackageFamilyName);
            if (supportStatus != LaunchQuerySupportStatus.Available)
            {
                return false;
            }
            else return true;

        }

        public static async void launchuwp()
        {

            var options = new LauncherOptions { TargetApplicationPackageFamilyName = TargetPackageFamilyName };
            bool success = await Launcher.LaunchUriAsync(uri, options);

        }
        public String GetUWPUri()
        {
            return ConfigUtil.GetValue("uwpUri");
        }
        private String GetUWPFamilyName()
        {
            return ConfigUtil.GetValue("TargetPackageFamilyName");
        }



        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private void MiracastEnter()
        {
            keybd_event(Keys.Left, 0, 0, 0);
            keybd_event(Keys.Left, 0, 2, 0);
            keybd_event(Keys.Enter, 0, 0, 0);
            keybd_event(Keys.Enter, 0, 2, 0);
        }
        public void ClearMiracastPopup()
        {

            WindowControl wc = new WindowControl();
            wc.ActivateWindow("Shell_Dialog", null);
            Thread.Sleep(1000);
            MiracastEnter();

        }

    }
}

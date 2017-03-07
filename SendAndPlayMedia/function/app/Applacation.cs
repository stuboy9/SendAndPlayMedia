using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.info.applacation;

namespace SendAndPlayMedia
{
    class Applacation
    {
        private string data = "";
        private Object objApp = new Object();
        public Applacation()
        {
            lock (objApp)
            {
                Init();
            }
        }
        private void Init()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            if(File.Exists(path + "\\data.txt")){
                data = File.ReadAllText(path+"\\data.txt");
            }else
            {
                FileStream fs = new FileStream(path + "\\data.txt", FileMode.Create);
                StringWriter sw = new StringWriter();
                AppLibrary library = new AppLibrary(new List<ApplacationItem>());

                string[] appName = { "PotPlayer", "网易云音乐", "NotePad++", "WORLD", "POWERPNT", "有道词典", "EXCEL" };
                string[] appPath = { @"E:\software\PotPlayer\PotPlayerMini.exe" , @"E:\software\CloudMusic\cloudmusic.exe", @"E:\software\Notepad++\notepad++.exe" , @"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE" , @"C:\Program Files (x86)\Microsoft Office\root\Office16\POWERPNT.EXE" , @"E:\software\YOUDAODict\YoudaoDict.exe", @"C:\Program Files (x86)\Microsoft Office\root\Office16\EXCEL.EXE" };
                for(int i = 0; i < appName.Length; i++)
                {
                    ApplacationItem item = new ApplacationItem(appName[i],appPath[i]);
                    library.value.Add(item);
                }
                string json = JsonConvert.SerializeObject(library);
                //JsonWriter writer = new JsonTextWriter(sw);
                //writer.WriteStartObject();
                //writer.WritePropertyName("applacation");

                //writer.WriteStartArray();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("QQ");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"E:\software\QQ\Bin\QQ.exe");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("PotPlayer");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"E:\software\PotPlayer\PotPlayerMini.exe");
                //writer.WriteEndObject();
                

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("网易云音乐");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"E:\software\CloudMusic\cloudmusic.exe");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("NotePad++");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"E:\software\Notepad++\notepad++.exe");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("WORLD");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("POWERPNT");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"C:\Program Files (x86)\Microsoft Office\root\Office16\POWERPNT.EXE");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("有道词典");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"E:\software\YOUDAODict\YoudaoDict.exe");
                //writer.WriteEndObject();

                //writer.WriteStartObject();
                //writer.WritePropertyName("name");
                //writer.WriteValue("EXCEL");
                //writer.WritePropertyName("path");
                //writer.WriteValue(@"C:\Program Files (x86)\Microsoft Office\root\Office16\EXCEL.EXE");
                //writer.WriteEndObject();

                //writer.WriteEndArray();
                //writer.WriteEndObject();

                //writer.Flush();
                
                StreamWriter s = new StreamWriter(fs);
                data = json;
                s.WriteLine(data);
                s.Close();
                fs.Close();
            }

        }

        public string GetAppList()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            path += "\\data.txt";
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            line = sr.ReadToEnd();
            return line;
        }
         
        public Process OpenApp(string name,string param)
        {
            try
            {
                CloseAll();
                Process rdcProcess = new Process();
                rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(name);
                rdcProcess.StartInfo.Arguments = param;
                rdcProcess.Start();
                return rdcProcess;
            }
            catch (Exception e)
            {
                throw new Exception("打开程序异常异常");
            }
        }
        public void CloseAll()
        {
            AppLibrary library = JsonConvert.DeserializeObject<AppLibrary>(GetAppList());
            foreach(ApplacationItem item in library.value)
            {
                CloseApp(item.packageName.Split('\\').Last().Split('.').First());
            }
        }
        public void CloseApp(string name)
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process item in ps)
            {
                if (item.ProcessName == name)
                {
                    item.Kill();
                }
            }
        }



    }
}

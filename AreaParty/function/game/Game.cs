using AreaParty.info.applacation;
using AreaParty.info.game;
using AreaParty.util.config;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.function.game
{
    class Game
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Game()
        {
            
        }
        public void ProduceGameIcon(string apppath, string name, string iconpath)
        {
            util.IconUtil.GetIconFromFile(apppath, 2, iconpath + "\\" + name + ".png");
        }
        public List<GameItem> GetGameList()
        {
            return GameConfig.GetAllGame(); 
        }

        public Process OpenGame(string path, string param)
        {
            try
            {

                CloseAllGame();
                new app.Applacation().CloseAll();
                Process rdcProcess = new Process();
                FileInfo fi = new FileInfo(path);
                rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(path);
                rdcProcess.StartInfo.Arguments = param;
                rdcProcess.StartInfo.WorkingDirectory = fi.Directory.ToString();
                rdcProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                rdcProcess.Start();
                return rdcProcess;
            }
            catch (Exception e)
            {
                log.Error("打开软件错误", e);
                return null;
            }
        }
        public void CloseAllGame()
        {
            try
            {
                List<GameItem> list = GetGameList();
                foreach (GameItem item in list)
                {
                    CloseApp(item.packageName.Split('\\').Last().Split('.').First());
                }

            }
            catch (Exception e)
            {
                log.Error("调用CloseAll函数报错:", e);
            }
        }
        public void CloseApp(string name)
        {
            try
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
            catch (Exception e)
            {
                log.Error("调用CloseApp函数报错:", e);
            }

        }
    }
}

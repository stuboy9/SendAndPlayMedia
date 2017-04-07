using CustomMediaTransportControls;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace myProjection
{
    public sealed partial class MainPage : Page
    {
        public Windows.Storage.StorageFile file;
        private MainPage rootPage;
        //private DevicePicker picker;
        ProjectionViewBroker pvb ;
        //DeviceInformation activeDevice = null;
        //主页面ID号
        int thisViewId;
        public static MainPage Current;
        public ViewLifetimeControl ProjectionViewPageControl;
        //多个搜索到的设备名 字符串
        private static String names = "";
        //查询语句(通过ProjectionManager.GetDeviceSelector()方法获得)
        private static string aqsFilter = "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:= \"{0407d24e-53de-4c9a-9ba1-9ced54641188}\"AND System.Devices.WiFiDirect.Services:~= \"Miracast\"";
        private static List<DeviceInformation> deviceList = new List<DeviceInformation>();

        private static int viewId = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Id;
        private static CoreDispatcher disPa;
     
        private static readonly StorageFolder sf = KnownFolders.VideosLibrary;//视频库
        private DeviceInformationCollection deviceInfoColl = null;//设备信息集合
        private static int requestTime = 0;//请求设备列表次数
        public static int viewID
        {
            get
            {
                return viewId;
            }
        }

        //public static CoreDispatcher DisPatcher
        //{
        //    get
        //    {
        //        return disPa;
        //    }
        //}

        public MainPage()
        {
            try
            {
                this.InitializeComponent();
                ApplicationView.PreferredLaunchViewSize = new Size(250,200);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

                Current = this;
                rootPage = MainPage.Current;
                thisViewId = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Id;

                CommunicateWithPc();
                FileOper();//等待读取投影请求文件，投影桌面
                ScanCurrentName();//等待读取设备名请求文件，写回设备名称文件
            }
            catch (Exception ex4321)
            {
                rootPage.NotifyUser("MainPage:" + ex4321.ToString(), NotifyType.ErrorMessage);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern SafeFileHandle CreateFile(
           string lpFileName,
           [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
           [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
           [MarshalAs(UnmanagedType.U4)] System.IO.FileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);
        private async void CommunicateWithPc()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(1500);

                    var handle = CreateFile(@"\\.\pipe\mypipe", FileAccess.ReadWrite, FileShare.None, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                    if (handle.IsInvalid)
                    {
                        var err = Marshal.GetLastWin32Error();
                        //rootPage.NotifyUser("ScanCurrentName:" + "error", NotifyType.ErrorMessage);
                        continue;
                    }

                    StreamReader sr = new StreamReader(new FileStream(handle, FileAccess.Read), Encoding.UTF8);

                    string read = sr.ReadLine();


                    StreamWriter sw = new StreamWriter(new FileStream(handle, FileAccess.Write), Encoding.UTF8);
                   
                    Command y = Command.ToCommandFromJson(read);
                    string re = "";
                    if (y.command == "GET")
                    {
                        deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
                        lock (deviceList)
                        {
                            deviceList.Clear();
                            foreach (DeviceInformation di in deviceInfoColl)
                            {
                                deviceList.Add(di);
                            }
                        }

                        Response r = new Response();
                        r.status = "200";
                        r.name = "UWP";
                        r.type = "data";
                        foreach(DeviceInformation di in deviceList)
                            r.value.Add(di.Name);
                        re = r.ToJson();
                        sw.WriteLine(re);
                        sw.Flush();
                        handle.Dispose();
                    }
                    else if (y.command == "PLAY")
                    {
                        Response r = new Response();
                        r.status = "200";
                        r.name = "UWP";
                        r.type = "execteMessage";

                        DeviceInformation device = null;
                        if (!(deviceList.Count > 0))
                        {
                            deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
                        }
                        lock (deviceList)
                        {
                            foreach (DeviceInformation di in deviceList)
                                if (di.Name == y.param["render"])
                                    device = di;
                        } 
                        if (device == null)
                        {
                            //deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
                            //lock (deviceList)
                            //{
                            //    deviceList.Clear();
                            //    foreach (DeviceInformation di in deviceInfoColl)
                            //    {
                            //        deviceList.Add(di);
                            //        if (di.Name == y.param["render"])
                            //            device = di;
                            //    }
                            //}
                            deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);

                            throw new Exception("没有匹配的设备或当前设备搜索不到，请重新选择");
                        }
                            
                        thisViewId = MainPage.viewID;

                        if (rootPage.ProjectionViewPageControl == null)
                        {
                            try
                            {
                                //创建新的空白页面
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    disPa = Window.Current.Dispatcher;
                                    rootPage.NotifyUser("创建页面: ", NotifyType.StatusMessage);
                                });

                                await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    //生命周期控制
                                    rootPage.ProjectionViewPageControl = ViewLifetimeControl.CreateForCurrentView();
                                    pvb = new ProjectionViewBroker();
                                    pvb.ProjectionViewPageControl = rootPage.ProjectionViewPageControl;
                                    pvb.MainViewId = thisViewId;
                                    pvb.MainPageDispatcher = disPa;
                                    //新建页面存入视图，当调用StartProjectingAsync时显示这个视图
                                    var rootFrame = new Frame();
                                    rootFrame.Navigate(typeof(ProjectionViewPage), pvb);
                                    Window.Current.Content = rootFrame;
                                    Window.Current.Activate();
                                });
                            }
                            catch (Exception e444)
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    rootPage.NotifyUser("create view:" + e444.ToString(), NotifyType.ErrorMessage);
                                });
                            }
                        }
                        try
                        {
                            rootPage.ProjectionViewPageControl.StartViewInUse();
                            await Task.Delay(200);//闪退...
                            IAsyncAction projection = ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, device);
                            //await ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, device);

                            projection.Completed += async delegate (IAsyncAction asyncInfo, AsyncStatus asyncStatus)
                            {
                                try
                                {
                                    // asyncInfo.AsTask();
                                    switch (asyncStatus)
                                    {
                                        case AsyncStatus.Completed:
                                            if (ProjectionManager.ProjectionDisplayAvailable && pvb.ProjectedPage != null)
                                            {
                                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                                {
                                                    await ApplicationViewSwitcher.SwitchAsync(thisViewId, rootPage.ProjectionViewPageControl.Id, ApplicationViewSwitchingOptions.ConsolidateViews);
                                                });

                                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                                {
                                                    rootPage.NotifyUser(" 投影成功", NotifyType.StatusMessage);
                                                });
                                                re = r.ToJson();
                                                sw.WriteLine(re);
                                                sw.Flush();
                                                handle.Dispose();
                                            }
                                            re = r.ToJson();
                                            sw.WriteLine(re);
                                            sw.Flush();
                                            handle.Dispose();
                                            break;
                                        case AsyncStatus.Error:
                                            if (ProjectionManager.ProjectionDisplayAvailable && pvb.ProjectedPage != null)
                                            {

                                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                                {
                                                    rootPage.NotifyUser(" 投影成功...", NotifyType.StatusMessage);
                                                });
                                            }
                                            re = r.ToJson();
                                            sw.WriteLine(re);
                                            sw.Flush();
                                            handle.Dispose();
                                            break;
                                        //throw new Exception("投影出错");
                                        default:
                                            r.status = "404";
                                            re = r.ToJson();
                                            sw.WriteLine(re);
                                            sw.Flush();
                                            handle.Dispose();
                                            throw new Exception("投影出现未知错误");
                                    }
                                }
                                catch (Exception e)
                                {
                                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        rootPage.NotifyUser("isProjectionCompleted:" + e.ToString(), NotifyType.ErrorMessage);
                                    });
                                }
                            };

                            await Task.Delay(200);//闪退...
                            rootPage.ProjectionViewPageControl.StopViewInUse();
                            //rootPage.ProjectionViewPageControl = null;
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                rootPage.NotifyUser("ProjectionManager:" + ex.ToString(), NotifyType.ErrorMessage);
                            });

                        }

                    }

                    
                }
                catch (Exception e)
                {

                }
            }

        }
        private void parseCommand(string json)
        {
            //JObject json = new JObject()
        }
        //扫描显示设备
        
        //private async Task<bool> Scaned()
        //{
        //    try
        //    {
        //        deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
        //        if (canChangeList)
        //        {
        //            lock(deviceList)
        //            {
        //                deviceList.Clear();
        //                foreach (DeviceInformation di in deviceInfoColl)
        //                {
        //                    deviceList.Add(di);
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //        {
        //            rootPage.NotifyUser("Scaned:" + e.ToString(), NotifyType.ErrorMessage);
        //        });
        //        return false;
        //    }
        //}

        //读取屏幕请求文件
        private async void ScanCurrentName()
        {
            string fileName = "name.txt";
            while (true)
            {
                try
                {
                    //读取请求文件
                    await Task.Delay(500);
                    bool isOk = false;// await IsFileFound("name.txt");
                    while (!isOk)
                    {
                        await Task.Delay(1500);
                        //Monitor.Enter(sf);
                        var files = await sf.GetFilesAsync();
                        //Monitor.Exit(sf);
                        foreach (StorageFile f in files)
                        {
                            if (f.Name == fileName)
                            {
                                isOk = true;
                                break;
                            }
                        }
                        //isOk = await IsFileFound("name.txt");
                    }
                    String receive = await ReadFile("name.txt");
                    if (receive == "screen")
                        await WriteFileNames();
                }
                catch (Exception e)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        rootPage.NotifyUser("ScanCurrentName:" + e.ToString(), NotifyType.ErrorMessage);
                    });
                }
                //finally
                //{
                //    if (Monitor.IsEntered(sf) == true)
                //        Monitor.Exit(sf);
                //}
            }
        }

        //服务监听
        private async void FileOper()
        {
            string fileName = "client.txt";
            while (true)
            {
                try
                {
                    //读取请求文件
                    bool isOk = false;// await IsFileFound("client.txt");
                    while (!isOk)
                    {
                        await Task.Delay(1500);
                        //Monitor.Enter(sf);
                        var files = await sf.GetFilesAsync();
                        //Monitor.Exit(sf);
                        foreach (StorageFile f in files)
                        {
                            if (f.Name == fileName)
                            {
                                isOk = true;
                                break;
                            }
                        }
                        //isOk = await IsFileFound("client.txt");
                    }
                    String receive = await ReadFile("client.txt");
                    if (0 <= Int32.Parse(receive))
                    {
                        int number = Int32.Parse(receive);
                        try
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                rootPage.NotifyUser("数组下标：index= " + number + " list.size= " + deviceList.Count, NotifyType.StatusMessage);
                            });
                            DeviceInformation device = null;
                            lock (deviceList)
                            {
                                device = deviceList.ElementAt(number);
                            }
                             
                            if (device == null)
                                throw new Exception("没有匹配的设备或当前设备搜索不到，请重新选择");
                            thisViewId = MainPage.viewID;


                            //如果投影存在，可以再次显示；若不存在，重新创建一个新的页面显示内容
                            if (rootPage.ProjectionViewPageControl == null)
                            {
                                try
                                {
                                    //创建新的空白页面
                                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        disPa = Window.Current.Dispatcher;
                                        rootPage.NotifyUser("创建页面: ", NotifyType.StatusMessage);
                                    });
                                    
                                    await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        //生命周期控制
                                        rootPage.ProjectionViewPageControl = ViewLifetimeControl.CreateForCurrentView();
                                        pvb = new ProjectionViewBroker();
                                        pvb.ProjectionViewPageControl = rootPage.ProjectionViewPageControl;
                                        pvb.MainViewId = thisViewId;
                                        pvb.MainPageDispatcher = disPa;
                                        //新建页面存入视图，当调用StartProjectingAsync时显示这个视图
                                        var rootFrame = new Frame();
                                        rootFrame.Navigate(typeof(ProjectionViewPage), pvb);
                                        Window.Current.Content = rootFrame;
                                        Window.Current.Activate();
                                    });
                                }
                                catch (Exception e444)
                                {
                                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        rootPage.NotifyUser("create view:" + e444.ToString(), NotifyType.ErrorMessage);
                                    });
                                }
                            }
                            try
                            {
                                rootPage.ProjectionViewPageControl.StartViewInUse();
                                await Task.Delay(200);//闪退...
                                IAsyncAction projection = ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, device);
                                //await ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, device);
                                projection.Completed += isProjectionCompleted;
                                await Task.Delay(200);//闪退...
                                rootPage.ProjectionViewPageControl.StopViewInUse();
                                //rootPage.ProjectionViewPageControl = null;
                            }
                            catch (Exception ex)
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    rootPage.NotifyUser("ProjectionManager:" + ex.ToString(), NotifyType.ErrorMessage);
                                });

                            }
                        }
                        catch (Exception e32)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                rootPage.NotifyUser("ProjectionManager all:" + e32.ToString() + "index= " + number + ",list.size= " + deviceList.Count, NotifyType.ErrorMessage);
                            });
                            requestTime = 10;//强制要求下次扫描设备...
                        }
                    }
                }
                catch (Exception eee)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        rootPage.NotifyUser(eee.ToString(), NotifyType.ErrorMessage);
                    });
                }
                //finally
                //{
                //    if (Monitor.IsEntered(sf) == true)
                //        Monitor.Exit(sf);
                //}
            }
        }
        //投影完成事件 
        private async void isProjectionCompleted(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            try
            {
                // asyncInfo.AsTask();
                switch (asyncStatus)
                {
                    case AsyncStatus.Completed:
                        if (ProjectionManager.ProjectionDisplayAvailable && pvb.ProjectedPage != null)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                await ApplicationViewSwitcher.SwitchAsync(thisViewId, rootPage.ProjectionViewPageControl.Id, ApplicationViewSwitchingOptions.ConsolidateViews);
                            });

                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                rootPage.NotifyUser(" 投影成功", NotifyType.StatusMessage);
                            });
                        }
                        break;
                    case AsyncStatus.Error:
                        if (ProjectionManager.ProjectionDisplayAvailable && pvb.ProjectedPage != null)
                        {

                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                rootPage.NotifyUser(" 投影成功...", NotifyType.StatusMessage);
                            });
                        }
                        break;
                    //throw new Exception("投影出错");
                    default:
                        throw new Exception("投影出现未知错误");
                }
            }
            catch (Exception e)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser("isProjectionCompleted:" + e.ToString(), NotifyType.ErrorMessage);
                });
            }
        }

        private async Task<bool> WriteFileNames()
        {
            try
            {
                //扫描设备信息....
                if (requestTime >= 10 || deviceList.Count <= 0)//如果请求次数>10，或者deviceList为空，扫描..
                {
                    deviceInfoColl = await DeviceInformation.FindAllAsync(aqsFilter);
                    String devicesName = "";
                    lock (deviceList)
                    {
                        deviceList.Clear();
                        foreach (DeviceInformation di in deviceInfoColl)
                        {
                            deviceList.Add(di);
                            devicesName += di.Name + ",";
                        }
                    }
                    names = devicesName.TrimEnd(',');
                    requestTime = 1;
                }
                else//计数+1
                    requestTime++;
                
                //创建文件，写入 
                //Monitor.Enter(sf);
                StorageFile storageFile = await sf.CreateFileAsync("server.txt", CreationCollisionOption.ReplaceExisting);
                //Monitor.Exit(sf);
                using (Stream file = await storageFile.OpenStreamForWriteAsync())
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.Write(names);
                    }
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser(String.Format("创建文件:'{0}'", names), NotifyType.StatusMessage);
                });
                storageFile = null;
                return true;
            }
            catch (Exception ex22)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser("WriteFileNames:" + ex22.ToString(), NotifyType.ErrorMessage);
                });
                return false;
            }
            //finally
            //{
            //    if (Monitor.IsEntered(sf) == true)
            //        Monitor.Exit(sf);
            //}
        }
        
        private async Task<string> ReadFile(String name)
        {
            //读取文件
            try
            {
                StorageFile readFile = await sf.GetFileAsync(name);
                String receive = "";

                using (Stream file = await readFile.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        receive = reader.ReadLine();
                    }
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser(String.Format("读取文件:'{0}'", receive), NotifyType.StatusMessage);
                });
                await readFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return receive;
            }
            catch (Exception e33)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser("ReadFile:" + e33.ToString(), NotifyType.ErrorMessage);
                });
                return null;
            }
            //finally
            //{
            //    if (Monitor.IsEntered(sf) == true)
            //        Monitor.Exit(sf);
            //}
        }

        //判断文件是否存在
        private async Task<bool> IsFileFound(String fileName)
        {
            try
            {
                bool fileExist = false;
                //Monitor.Enter(sf);
                var files = await sf.GetFilesAsync();
                //Monitor.Exit(sf);
                foreach (StorageFile f in files)
                {
                    if (f.Name == fileName)
                    {
                        fileExist = true;
                        break;
                    }
                }
                return fileExist;
            }
            catch (Exception exc)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser("IsFileFound:" + exc.ToString(), NotifyType.ErrorMessage);
                });
                return false;
            }
            //finally
            //{
            //    if (Monitor.IsEntered(sf) == true)
            //        Monitor.Exit(sf);
            //}
        }
        
        //页面上显示程序运行状态
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            status.Text = strMessage;
            StatusBorder.Visibility = (status.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };
    }
}

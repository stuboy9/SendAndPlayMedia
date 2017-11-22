using AreaParty.function.dlna;
using AreaParty.function.miracast;
using AreaParty.info;
using AreaParty.info.miracast;
using AreaParty.info.tv;
using AreaParty.pages.setting;
using AreaParty.util.config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AreaParty.windows
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public delegate void Finish();
        public Finish finish =null;
        pages.setting.TVScreen tvscreen = new pages.setting.TVScreen();
        pages.setting.Game game= new pages.setting.Game();
        pages.setting.Miracast miracast = new pages.setting.Miracast();

        private  ObservableCollection<MyListBoxItem> ScreenTV = new ObservableCollection<MyListBoxItem>(); 
        private  ObservableCollection<MyListBoxItem> Miracast = pages.setting.Miracast.MiracastTV;

        private bool ScreenTVServer = true;
        private bool ScreenMiracast = true;
        UserControl current = null;
        public Window1()
        {
            InitializeComponent();
            ScreenTV=tvscreen.ScreenTV;

            tvscreen.screen_cancel_click += Cancel;
            tvscreen.screen_ensure_click += Next;
            game.game_cancel_click += Cancel;
            game.game_next_click += Next;
            miracast.miracast_cancel_click += Cancel;
            miracast.miracast_next_click+=Next;
            miracast.miracast_skip_click += Next;

            this.pTransitionControl_3.ShowPage(tvscreen);
            current = tvscreen;
            InitGetSource();
        }
        private void Cancel()
        {
            CloseSource();
            this.Close();
        }
        private void Next()
        {
            if(current is pages.setting.TVScreen)
            {
                this.pTransitionControl_3.ShowPage(game);
                current = game;
            }
            else if(current is pages.setting.Game)
            {
                this.pTransitionControl_3.ShowPage(miracast);
                info.tv.TVCommand c = info.tv.TVCommand.GetInstance("GAME_PAIR", info.MyInfo.myIp, false, info.MyInfo.myMAC, "",null,null);
                function.tv.TVFunction.sendCommand(MyInfo.tvLibrary.value.Find(x=>x.name.Equals(this.tvscreen.TVName)).ip, c);
                current = miracast;
            }
            else
            {
                if (UserScreenConfig.IsExistsScreen(this.tvscreen.TVName))
                {
                    UserScreenConfig.RemoveScreen(this.tvscreen.TVName);

                }
                Console.WriteLine("配置完成"+this.tvscreen.TVName+ this.miracast.miracastname);
                UserScreenConfig.AddScreen(this.tvscreen.TVName, null, this.miracast.miracastname);
                CloseSource();
                if (finish != null)
                {
                    finish();
                }
                this.Close();
            }
        }
        public void InitGetSource()
        {
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)GetScreenTV);
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)GetDlna);
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)GetMiracast);
            Console.WriteLine("线程开始");
            Thread ThreadScreenTV = new Thread(GetScreenTV);
            ThreadScreenTV.IsBackground = true;
            ThreadScreenTV.Start();
            

            Thread ThreadScreenMiracast = new Thread(GetMiracast);
            ThreadScreenMiracast.IsBackground = true;
            ThreadScreenMiracast.Start();

        }
        public void CloseSource()
        {
            this.ScreenTVServer = false;
            this.ScreenMiracast = false;
        }
        public void GetScreenTV()
        {
            while (this.ScreenTVServer)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()

                {

                    foreach (TVInfo item in MyInfo.tvLibrary.value)
                    {
                        MyListBoxItem temp = new MyListBoxItem { Name = item.name, ImagePath = "/styles/skin/item/item_tv.png" };
                        if (!ScreenTV.Contains(temp))
                            ScreenTV.Add(temp);

                        //Console.WriteLine(item.name);
                    }

                });
                Thread.Sleep(1000);
            }
        }
        public async void GetMiracast()
        {
            Projection p = new Projection();
            while (this.ScreenMiracast)
            {
                MiracastLibrary s = await p.GetDeviceList();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {

                    foreach (AreaParty.info.miracast.Screen sc in s.value)
                    {
                        MyListBoxItem temp = new MyListBoxItem { Name = sc.name, ImagePath = "/styles/skin/item/item_tv.png" };
                        if (!Miracast.Contains(temp))
                            Miracast.Add(new MyListBoxItem { Name = sc.name, ImagePath = "/styles/skin/item/item_tv.png" });
                    }

                });
                Thread.Sleep(1000);
            }
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSource();
            this.Close();
        }

        private void mniButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //拖动
            this.DragMove();
        }
    }

}

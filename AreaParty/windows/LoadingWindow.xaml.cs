using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AreaParty.windows
{
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : Window
    {
        int time = 60;
        public LoadingWindow()
        {
            InitializeComponent();
        }
        public void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Time_Trick;  //你的事件
            timer.Start();
        }
        public void Time_Trick(object sender, EventArgs e)
        {
            time -= 1;
            this.Timer.Text = time + "s";
            if (time <= 0) this.Close();
        }

    }
}

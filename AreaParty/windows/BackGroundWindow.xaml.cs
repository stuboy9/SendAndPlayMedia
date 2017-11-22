using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// BackGroundWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BackGroundWindow : Window
    {

        private string BkwPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "//backgroundwindow";

        //private Queue<string> background = new Queue<string>();

        //private void init()
        //{
        //    getFileName();
        //}

        //private Queue<string> getFileName()
        //{
        //    DirectoryInfo TheFolder = new DirectoryInfo(BkwPath);
        //    foreach (FileInfo NextFile in TheFolder.GetFiles())
        //    {
        //        background.Enqueue(BkwPath + "//" + NextFile.Name + ".jpg");
        //    }
        //    return background;       
        //}
        //单件实例 
        #region 
        private static BackGroundWindow _instance;
        private static readonly object ObjLok = new object();
        public static BackGroundWindow Instance()
        {
            lock (ObjLok)
            {
                return _instance ?? (_instance = new BackGroundWindow());
            }
        }
        private BackGroundWindow()
        {
            string path = BkwPath + "//backgroundwindow1.jpg";
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(path));
            imageBrush.Stretch = Stretch.Fill;
            this.Background = imageBrush;
            InitializeComponent();
            
        }
        //public static void close()
        //{
        //    _instance.Close();
        //}
        #endregion  
        //private void closeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}
        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    Hide();
        //    e.Cancel = true;
        //}

    }
}

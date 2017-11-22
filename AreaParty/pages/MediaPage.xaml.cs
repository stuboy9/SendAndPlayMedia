using AreaParty.routedcommand;
using AreaParty.util;
using AreaParty.util.config;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AreaParty.pages
{
    
    public static class ListName
    {
        public const string VIDIO = "VIDEO";
        public const string AUDIO = "AUDIO";
        public const string IMAGE = "IMAGE";
    }
    /// <summary>
    /// MediaPage.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPage : UserControl
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        Dictionary<string, string> dictVideo = new Dictionary<string, string>();
        Dictionary<string, string> dictAudio = new Dictionary<string, string>();
        Dictionary<string, string> dictImage = new Dictionary<string, string>();
        ObservableCollection<ListBoxMediaItem> video;
        public MediaPage()
        {
            
            InitializeComponent();
            ObservableCollection<ListBoxMediaItem> screen = new ObservableCollection<ListBoxMediaItem>();
            ObservableCollection<ListBoxMediaItem> videoList = initvideoList();
            ObservableCollection<ListBoxMediaItem> audioList = initaudeoList();
            ObservableCollection<ListBoxMediaItem> imageList = initimageList();

            List<string> list = MediaConfig.GetMyVideoLibrary();
            foreach (string d in list)
            {
                FileInfo Info = new FileInfo(d);
                videoList.Add(new ListBoxMediaItem { Name = Info.Name, ImagePath = "/styles/skin/item/item_video.png" });
                dictVideo.Add(Info.Name, d);
            }

            list = MediaConfig.GetMyAudioLibrary();
            foreach (string d in list)
            {
                FileInfo Info = new FileInfo(d);
                audioList.Add(new ListBoxMediaItem { Name = Info.Name, ImagePath = "/styles/skin/item/item_audio.png" });
                dictAudio.Add(Info.Name, d);
            }
            list = MediaConfig.GetMyImageLibrary();
            foreach (string d in list)
            {
                FileInfo Info = new FileInfo(d);
                imageList.Add(new ListBoxMediaItem { Name = Info.Name, ImagePath = "/styles/skin/item/item_image.png" });
                dictImage.Add(Info.Name, d);
            }
            //this.DataContext = screen;
            this.listbox.ItemsSource = videoList;
            this.listbox1.ItemsSource = audioList;
            this.listbox2.ItemsSource = imageList;
            this.listbox.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.listbox.RaiseEvent(eventArg);
            };
            this.listbox1.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.listbox1.RaiseEvent(eventArg);
            };
            this.listbox2.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.listbox2.RaiseEvent(eventArg);
            };


        }
        
        private ObservableCollection<ListBoxMediaItem> initvideoList()
        {
            ObservableCollection<ListBoxMediaItem> videoList = new ObservableCollection<ListBoxMediaItem>();
            videoList.Add(new ListBoxMediaItem { Name = "我的视频", ImagePath = "/styles/skin/item/item_video.png" });
            videoList.Add(new ListBoxMediaItem { Name = "下载视频", ImagePath = "/styles/skin/item/item_video.png" });
            return videoList;
        }
        private ObservableCollection<ListBoxMediaItem> initaudeoList()
        {
            ObservableCollection<ListBoxMediaItem> audioList = new ObservableCollection<ListBoxMediaItem>();
            audioList.Add(new ListBoxMediaItem { Name = "我的音频", ImagePath = "/styles/skin/item/item_audio.png" });
            return audioList;
        }
        private ObservableCollection<ListBoxMediaItem> initimageList()
        {
            ObservableCollection<ListBoxMediaItem> imageList = new ObservableCollection<ListBoxMediaItem>();
            imageList.Add(new ListBoxMediaItem { Name = "我的图片", ImagePath = "/styles/skin/item/item_image.png" });
            return imageList;
        }

        private void Menu_Remove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ListBoxMediaItem> video = this.listbox.ItemsSource as ObservableCollection<ListBoxMediaItem>;
            ObservableCollection<ListBoxMediaItem> audio = this.listbox1.ItemsSource as ObservableCollection<ListBoxMediaItem>;
            ObservableCollection<ListBoxMediaItem> image = this.listbox2.ItemsSource as ObservableCollection<ListBoxMediaItem>;

            object ob = this.listbox.SelectedItem;
            object ob1 = this.listbox1.SelectedItem;
            object ob2 = this.listbox2.SelectedItem;
            string type = null;
            if (ob == null&&ob1==null&&ob2==null)
            {
                MessageBox.Show("请选择一个媒体库");
                return;
            }
            ListBox lb = null;
            if (ob != null)
            {
                lb = this.listbox;
                type = "VIDEO";
            }
            else if (ob1 != null)
            {
                lb = this.listbox1;
                ob = ob1;
                type = "AUDIO";
            }
            else if (ob2 != null) {
                lb = this.listbox2;
                ob = ob2;
                type = "IMAGE";
            }
            else return;
            log.Info(string.Format("选择媒体列表{0}：", type));
            //Countries.countries.Add(new Country { Name = "YI", ImagePath = @"C:\Users\Yi\Desktop\WPFPicture\icon\tv.png" });
            
            ListBoxItem item = (ListBoxItem)(lb.ItemContainerGenerator.ContainerFromItem(ob));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            TextBlock myTextBlock = (TextBlock)myDataTemplate.FindName("textBlock", myContentPresenter);

            try
            {
                switch (type)
                {
                    case ListName.VIDIO:
                        if (video.Contains(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" }))
                        {
                            video.Remove(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" });
                            MediaConfig.RemoveMyVideoLibrary(this.dictVideo[myTextBlock.Text]);
                            this.dictVideo.Remove(myTextBlock.Text);
                        }
                        break;
                    case ListName.AUDIO:
                        if (audio.Contains(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" }))
                        {
                            audio.Remove(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" });
                            MediaConfig.RemoveMyAudioLibrary(this.dictAudio[myTextBlock.Text]);
                            this.dictAudio.Remove(myTextBlock.Text);

                        }
                        break;
                    case ListName.IMAGE:
                        if (image.Contains(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" }))
                        {
                            image.Remove(new ListBoxMediaItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" });
                            MediaConfig.RemoveMyImageLibrary(this.dictImage[myTextBlock.Text]);
                            this.dictImage.Remove(myTextBlock.Text);
                        }
                        break;
                }
                log.Info(string.Format("删除{0}成功！", type));
            }
            catch(Exception error)
            {
                log.Info(string.Format("删除失败{0}：",error.Message));
            }
                    
        }
   


        private void Video_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog fbr = new System.Windows.Forms.FolderBrowserDialog();
                if (fbr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fbr.SelectedPath;
                    FileInfo fi = new FileInfo(path);
                    if (!this.dictVideo.ContainsKey(fi.Name))
                    {
                        this.dictVideo.Add(fi.Name, path);
                        video = this.listbox.ItemsSource as ObservableCollection<ListBoxMediaItem>;
                        video.Add(new ListBoxMediaItem { Name = fi.Name, ImagePath = "/styles/skin/item/item_video.png" });
                        //ObservableCollection<ListBoxMediaItem> test = this.listbox.ItemsSource as ObservableCollection<ListBoxMediaItem>;
                        //test.Add(new ListBoxMediaItem { Name = fi.Name, ImagePath = "/styles/skin/item/item_video.png" });
                        //MediaConfig.AddMediaLibrary(path);
                        MediaConfig.AddMyVideoLibrary(path);
                        //JAVAUtil.AddSourceToHTTP(path);
                        //new Thread(new ThreadStart(delegate ()
                        //{
                        new function.media.MediaFunction().GetThumbnail(path);
                        util.JAVAUtil.AddAlltoHttp(path);
                        //})).Start();
                    }
                }
            }
            catch (Exception error)
            {
                log.Info(string.Format("视频添加失败：{0}：", error.Message));
            }
            
        }
        
        private void Audio_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog fbr = new System.Windows.Forms.FolderBrowserDialog();
                if (fbr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fbr.SelectedPath;
                    FileInfo fi = new FileInfo(path);
                    if (!this.dictAudio.ContainsKey(fi.Name))
                    {
                        this.dictAudio.Add(fi.Name, path);
                        ObservableCollection<ListBoxMediaItem> test = this.listbox1.ItemsSource as ObservableCollection<ListBoxMediaItem>;
                        test.Add(new ListBoxMediaItem { Name = fi.Name, ImagePath = "/styles/skin/item/item_audio.png" });
                        //MediaConfig.AddMediaLibrary(path);
                        MediaConfig.AddMyAudioLibrary(path);
                        //JAVAUtil.AddSourceToHTTP(path);
                        //new Thread(new ThreadStart(delegate ()
                        //{
                        new function.media.MediaFunction().GetThumbnail(path);
                        util.JAVAUtil.AddAlltoHttp(path);
                        //})).Start();

                    }
                }
            }
            catch(Exception error)
            {
                log.Info(string.Format("音频添加失败：{0}：", error.Message));
            }    
        }


        
        private void Image_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog fbr = new System.Windows.Forms.FolderBrowserDialog();
                if (fbr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fbr.SelectedPath;
                    FileInfo fi = new FileInfo(path);
                    if (!this.dictImage.ContainsKey(fi.Name))
                    {
                        this.dictImage.Add(fi.Name, path);
                        ObservableCollection<ListBoxMediaItem> test = this.listbox2.ItemsSource as ObservableCollection<ListBoxMediaItem>;
                        test.Add(new ListBoxMediaItem { Name = fi.Name, ImagePath = "/styles/skin/item/item_image.png" });
                        //MediaConfig.AddMediaLibrary(path);
                        MediaConfig.AddMyImageLibrary(path);
                        //JAVAUtil.AddSourceToHTTP(path);
                        //new Thread(new ThreadStart(delegate ()
                        //{
                        new function.media.MediaFunction().GetThumbnail(path);
                        util.JAVAUtil.AddAlltoHttp(path);
                        //})).Start();

                    }
                }
            }catch(Exception error)
            {
                log.Info(string.Format("图片添加失败：{0}：", error.Message));
            }
            
        }




        private childItem FindVisualChild<childItem>(DependencyObject obj)where childItem : DependencyObject
        {

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        private class ListBoxMediaItem
        {
            public string Name { get; set; }
            public string ImagePath { get; set; }

            //public string Type { get; set; }
            //public override string GetType()
            //{
            //    return Type;
            //}

            public override string ToString()
            {
                return Name;
            }
            public override bool Equals(object obj)
            {
                if (obj is ListBoxMediaItem)
                {
                    return this.Name.Equals(((ListBoxMediaItem)obj).Name);
                }
                else
                    return false;

            }
            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }
        }
    }
}

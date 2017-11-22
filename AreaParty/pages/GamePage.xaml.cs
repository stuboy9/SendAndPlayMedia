using AreaParty.info;
using AreaParty.info.game;
using AreaParty.util.config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AreaParty.pages
{
    /// <summary>
    /// GamePage.xaml 的交互逻辑
    /// </summary>
    public partial class GamePage : UserControl
    {
        public GamePage()
        {
            InitializeComponent();
            ObservableCollection<ListBoxGameItem> screen = new ObservableCollection<ListBoxGameItem>();
            List<GameItem> list = GameConfig.GetAllGame();
            foreach (GameItem d in list)
            {
                screen.Add(new ListBoxGameItem { Name = d.appName, ImagePath = MyInfo.iconFolder + "\\" + d.appName + ".png" });
            }
            this.DataContext = screen;
        }
        private void Menu_Remove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ListBoxGameItem> test = this.DataContext as ObservableCollection<ListBoxGameItem>;

            object ob = this.listbox.SelectedItem;
            if (ob == null)
            {
                MessageBox.Show("请选择一个游戏");
                return;
            }
            //Countries.countries.Add(new Country { Name = "YI", ImagePath = @"C:\Users\Yi\Desktop\WPFPicture\icon\tv.png" });

            ListBoxItem item = (ListBoxItem)(this.listbox.ItemContainerGenerator.ContainerFromItem(ob));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            TextBlock myTextBlock = (TextBlock)myDataTemplate.FindName("textBlock", myContentPresenter);
            if (test.Contains(new ListBoxGameItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" }))
            {
                test.Remove(new ListBoxGameItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_media.png" });
                GameConfig.RemoveGame(myTextBlock.Text);
            }
        }
        private void Menu_Add_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Excel Files (*.exe)|*.exe"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string path = openFileDialog.FileName;
                FileInfo fi = new FileInfo(path);
                
                string name = fi.Name.Substring(0, fi.Name.LastIndexOf("."));
                if (!GameConfig.IsExistsGame(fi.Name))
                {
                    ObservableCollection<ListBoxGameItem> test = this.DataContext as ObservableCollection<ListBoxGameItem>;
                    test.Add(new ListBoxGameItem { Name = name, ImagePath = MyInfo.iconFolder + "\\" + name + ".png" });
                    GameConfig.AddGame(name, path);
                    util.IconUtil.GetIconFromFile(path, 2, MyInfo.iconFolder + "\\" + name + ".png");
                    util.JAVAUtil.AddSourceToHTTP(MyInfo.iconFolder + "\\" + name + ".png",false);
                }
            }
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
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
        private class ListBoxGameItem
        {
            public string Name { get; set; }
            public string ImagePath { get; set; }

            public override string ToString()
            {
                return Name;
            }
            public override bool Equals(object obj)
            {
                if (obj is ListBoxGameItem)
                {
                    return this.Name.Equals(((ListBoxGameItem)obj).Name);
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

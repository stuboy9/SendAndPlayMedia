using AreaParty.info.tv;
using AreaParty.routedcommand;
using AreaParty.util;
using AreaParty.util.config;
using AreaParty.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Screen.xaml 的交互逻辑
    /// </summary>
    public partial class Screen : UserControl
    {
        public Screen()
        {
            InitializeComponent();

            List<DeviceItem> list = UserScreenConfig.GetScreen();
            ObservableCollection<ScreenItem> screen = new ObservableCollection<ScreenItem>();
            foreach (DeviceItem d in list)
            {
                screen.Add(new ScreenItem { Name = d.name, ImagePath = "/styles/skin/item/item_tv.png" });
            }
            this.DataContext = screen;

            CommandBinding binding = new CommandBinding(MyRoutedCommand.EditCommand);
            binding.Executed += new ExecutedRoutedEventHandler(ListBoxItem_Edit_Click);
            CommandBinding cmd_Open = new CommandBinding(MyRoutedCommand.RemoveCommand);
            cmd_Open.Executed += new ExecutedRoutedEventHandler(ListBoxItem_Remove_Click);
            CommandBinding cmd_Save = new CommandBinding(MyRoutedCommand.AddCommand);
            cmd_Save.Executed += new ExecutedRoutedEventHandler(ListBox_Add_Click);
            this.CommandBindings.Add(binding);
            this.CommandBindings.Add(cmd_Open);
            this.CommandBindings.Add(cmd_Save);
        }
        private void ListBoxItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            Window1 sc = new Window1();
            sc.finish += UpScreen;
            sc.ShowDialog();
            //MessageBox.Show("Add");
            //Console.WriteLine("Add");
            //MessageBox.Show("Edit");
            Console.WriteLine("Edit");
        }
        private void ListBoxItem_Remove_Click(object sender,RoutedEventArgs e)
        {
            //MessageBox.Show("Remove");
            ObservableCollection<ScreenItem> test = this.DataContext as ObservableCollection<ScreenItem>;

            object ob = this.listbox.SelectedItem;
            if (ob == null)
            {
                MessageBox.Show("请选择一个屏幕");
                return;
            }
            //Countries.countries.Add(new Country { Name = "YI", ImagePath = @"C:\Users\Yi\Desktop\WPFPicture\icon\tv.png" });

            ListBoxItem item = (ListBoxItem)(this.listbox.ItemContainerGenerator.ContainerFromItem(ob));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            TextBlock myTextBlock = (TextBlock)myDataTemplate.FindName("textBlock", myContentPresenter);
            if (test.Contains(new ScreenItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_tv.png" }))
            {
                test.Remove(new ScreenItem { Name = myTextBlock.Text, ImagePath = "/styles/skin/item/item_tv.png" });
                UserScreenConfig.RemoveScreen(myTextBlock.Text);
            }
            //test.Remove();
            Console.WriteLine("Remove");
        }
        private void ListBox_Add_Click(object sender,RoutedEventArgs e)
        {
            Window1 w1 = new Window1();
            //ScreenConfigure sc = new ScreenConfigure();
            //sc.Finish += UpScreen;
            w1.finish += UpScreen;
            w1.Show();
            //sc.ShowDialog();
            //MessageBox.Show("Add");
            Console.WriteLine("Add");
        }
        private void UpScreen()
        {
            List<DeviceItem> list = UserScreenConfig.GetScreen();
            ObservableCollection<ScreenItem> screen = new ObservableCollection<ScreenItem>();
            foreach(DeviceItem d in list)
            {
                screen.Add(new ScreenItem { Name = d.name, ImagePath = "/styles/skin/item/item_tv.png" });
            }
            this.DataContext = screen;
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
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
    }
    public class ScreenItem
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            if (obj is ScreenItem)
            {
                return this.Name.Equals(((ScreenItem)obj).Name);
            }
            else
                return false;

        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }


    public class Countries
    {
        public static ObservableCollection<ScreenItem> GetCountry()
        {

            return new ObservableCollection<ScreenItem> {
                new ScreenItem {   Name = "Austri1Austria1Austria1",ImagePath = "/styles/skin/item/item_tv.png"},
                new ScreenItem {   Name = "Austria2",ImagePath = "/styles/skin/item/item_tv.png"},
                new ScreenItem {   Name = "Austria3",ImagePath = "/styles/skin/item/item_tv.png"},
                new ScreenItem {   Name = "Austria4",ImagePath = "/styles/skin/item/item_tv.png"},
                new ScreenItem {   Name = "Austria4",ImagePath = "/styles/skin/item/item_tv.png"},
                new ScreenItem {   Name = "Austria4",ImagePath = "/styles/skin/item/item_tv.png"},
            };
        }
    }
}

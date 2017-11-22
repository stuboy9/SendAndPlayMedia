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

namespace AreaParty.pages.setting
{
    /// <summary>
    /// TVScreen.xaml 的交互逻辑
    /// </summary>
    public partial class TVScreen : UserControl
    {
        public delegate void Screen_Cancel_Click();
        public delegate void Screen_Ensure_Click();
        
        
        public Screen_Cancel_Click screen_cancel_click = null;
        public Screen_Ensure_Click screen_ensure_click = null;

        public string TVName = null;
        public  ObservableCollection<MyListBoxItem> ScreenTV = new ObservableCollection<MyListBoxItem>();
        public TVScreen()
        {
            InitializeComponent();
            this.TVlistbox.ItemsSource = ScreenTV;
        }

        private void TVScreen_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (screen_cancel_click != null) screen_cancel_click();
        }

        private void TVScreen_Ensure_Click(object sender, RoutedEventArgs e)
        {
            object ob = this.TVlistbox.SelectedItem;
            if (ob == null)
            {
                MessageBox.Show("请选择一个屏幕");
                return;
            }
            TVName = GetSelectedItem();
            if (screen_ensure_click != null) screen_ensure_click();
        }

        private string GetSelectedItem()
        {
            object ob = this.TVlistbox.SelectedItem;
            if (ob == null)
            {
                return null;
            }
            //Countries.countries.Add(new Country { Name = "YI", ImagePath = @"C:\Users\Yi\Desktop\WPFPicture\icon\tv.png" });

            ListBoxItem item = (ListBoxItem)(this.TVlistbox.ItemContainerGenerator.ContainerFromItem(ob));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            Label myTextBlock = (Label)myDataTemplate.FindName("tvname", myContentPresenter);
            return myTextBlock.Content.ToString();
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
    }
    public class MyListBoxItem
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            if (obj is MyListBoxItem)
            {
                return this.Name.Equals(((MyListBoxItem)obj).Name);
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

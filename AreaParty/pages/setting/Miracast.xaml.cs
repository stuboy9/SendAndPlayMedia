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
    /// Miracastt.xaml 的交互逻辑
    /// </summary>
    public partial class Miracast : UserControl
    {
        public delegate void MIRACAST_Cancel_Click();
        public delegate void MIRACAST_Skip_Click();
        public delegate void MIRACAST_Next_Click();
        

        public MIRACAST_Cancel_Click miracast_cancel_click = null;
        public MIRACAST_Skip_Click miracast_skip_click = null;
        public MIRACAST_Next_Click miracast_next_click = null;
        public static ObservableCollection<MyListBoxItem> MiracastTV = new ObservableCollection<MyListBoxItem>();
        public string miracastname = null;
        public Miracast()
        {
            InitializeComponent();
            this.TVlistbox.ItemsSource = MiracastTV;
        }

        private void Miracast_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (miracast_cancel_click != null) miracast_cancel_click();
        }

        private void Miracast_Ensure_Click(object sender, RoutedEventArgs e)
        {
            object ob = this.TVlistbox.SelectedItem;
            if (ob == null)
            {
                MessageBox.Show("请选择一个接收端");
                return;
            }
            this.miracastname = GetSelectedItem();
            if (miracast_next_click != null) miracast_next_click();
        }

        private void Miracast_Skip_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("您将无法在手机上使用M模式将电脑应用投影到电视上，如需使用此功能请重新开启");
            this.miracastname = GetSelectedItem();
            miracast_skip_click();
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

}

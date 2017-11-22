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
    /// Dlna.xaml 的交互逻辑
    /// </summary>
    public partial class Dlna : UserControl
    {
        public delegate void DLNA_Cancel_Click();
        public delegate void DLNA_Skip_Click();
        public delegate void DLNA_Next_Click();


        public DLNA_Cancel_Click dlna_cancel_click = null;
        public DLNA_Skip_Click dlna_skip_click = null;
        public DLNA_Next_Click dlna_next_click = null;
        public static ObservableCollection<MyListBoxItem> DlnaTV = new ObservableCollection<MyListBoxItem>();
        public string dlnaname = null;

        public Dlna()
        {
            InitializeComponent();
            this.TVlistbox.ItemsSource = DlnaTV;
        }

        private void Dlna_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (dlna_cancel_click != null) dlna_cancel_click();

        }

        private void Dlna_Ensure_Click(object sender, RoutedEventArgs e)
        {
            object ob = this.TVlistbox.SelectedItem;
            if (ob == null)
            {
                MessageBox.Show("请选择一个接收端");
                return;
            }
            dlnaname = GetSelectedItem();
            if (dlna_next_click != null) dlna_next_click();
        }

        private void Dlna_Skip_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("如需使用此功能请重新开启");
            dlnaname = GetSelectedItem();
            dlna_next_click();
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

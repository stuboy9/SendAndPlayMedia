using AreaParty.util.config;
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
    /// PairCodeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PairCodeWindow : Window
    {
        public PairCodeWindow()
        {
            InitializeComponent();
        }
        //public string ResponseTexT
        //{
        //    get { return ResponseTextBox.Text; }
        //    set { ResponseTextBox.Text = value; }
        //}

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            String s = this.UserNameTextBox.Text;
            if((System.Text.RegularExpressions.Regex.IsMatch(s, "^(?![0-9]+$)(?![a-zA-Z]+$)[0-9a-zA-Z]+$")) & (s.Length == 8))
            {
                PairCodeConfig.SetPairCode(s);
                DialogResult = true;
            }else
            {
                this.UserNameTextBox.Text = "配对码";
            }
            
        }

        private void CannelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(this.UserNameTextBox.Text == "配对码")
            {
                this.UserNameTextBox.Text = "";
            }
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(this.UserNameTextBox.Text == "")
            {
                this.UserNameTextBox.Text = "配对码";
            }
        }
    }
}

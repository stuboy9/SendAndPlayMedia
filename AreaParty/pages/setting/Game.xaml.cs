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
    /// Game.xaml 的交互逻辑
    /// </summary>
    public partial class Game : UserControl
    {
        public delegate void GAME_Cancel_Click();
        public delegate void GAME_Next_Click();
        public delegate void GAME_CHECKBOX_Click();

        public GAME_Cancel_Click game_cancel_click = null;
        public GAME_Next_Click game_next_click = null;
        public GAME_CHECKBOX_Click game_checkbox_click=null;
        private static ObservableCollection<MyListBoxItem> DlnaTV = new ObservableCollection<MyListBoxItem>();
        public Game()
        {
            InitializeComponent();
        }

        private void Game_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (game_cancel_click != null) game_cancel_click();
        }

        private void Game_Ensure_Click(object sender, RoutedEventArgs e)
        {
            if (GameCheckBox.IsChecked == true)
            {
                MessageBox.Show("稍后电脑会弹出配对框，请用户在弹框中输入TV上所显示的验证码");
            }
            if (game_next_click != null) game_next_click();
        }
        private void Game_CheckBox_Click(object sender,RoutedEventArgs e)
        {
            
        }

    }
}

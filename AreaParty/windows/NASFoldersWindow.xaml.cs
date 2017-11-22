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
    /// NASFoldersWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NASFoldersWindow : Window
    {
        private List<int> selectFid = new List<int>();//保存多选文件夹ID  
        private List<int> allFid = new List<int>();//保存全选文件夹ID  
        private List<NASFolder> list = new List<NASFolder>();//文件夹列表源数据  

        public NASFoldersWindow()
        {
            //List<Folder> folders = null;
            

            InitializeComponent();
            this.initlist();
            //this.InitList(folders);
        }

        private void initlist()
        {
            for (int i = 0; i < 5; i++)
            {
                NASFolder user = new NASFolder()
                {
                    folder_id = i,
                    folder_name = "foldername" + i,
                    folder_statue = false
                };
                list.Add(user);
            }
            this.foldersview.ItemsSource = list;
        }

        //private void InitList(List<Folder> folders)
        //{
        //    int i = 0;
        //    foreach (Folder folder in folders)
        //    {
        //        folder.folder_id = i;
        //        list.Add(folder);
        //        i++;
        //    }
        //    this.foldersview.ItemsSource = list;
        //}
        /// <summary>
        /// 复选框映射文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_Click(object sender, RoutedEventArgs e)
        {
            //this.DeleteUsers(selectUid);
            this.MapFolders(selectFid);
            this.foldersview.Items.Refresh();//刷新数据  
        }

        /// <summary>
        /// 批量映射文件夹
        /// </summary>
        /// <param name="selectFid"></param>
        private void MapFolders(List<int> selectFid)
        {
            if (selectFid.Count > 0)
            {
                foreach (var fid in selectFid)
                {
                    list[fid].folder_statue = true;
                }
            }
        }
        /// <summary>
        /// 由checkbox的click事件来记录选中的行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            int folder_id = Convert.ToInt32(cb.Tag.ToString()); //获取该行id  
            if (cb.IsChecked == true)
            {
                selectFid.Add(folder_id);  //如果选中就保存id  
            }
            else
            {
                selectFid.Remove(folder_id);   //如果选中取消就删除里面的id  
            }
        }
        /// <summary>
        /// 单个文件夹映射
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int folder_id = Convert.ToInt32(b.CommandParameter);
            this.MapFolder(folder_id);
            this.foldersview.Items.Refresh();
        }
        private void MapFolder(int folder_id)
        {
            list[folder_id].folder_statue = true;
        }
        /// <summary>
        /// 全选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == true)
            {

                allFid = list.Select(l => l.folder_id).ToList();
            }
            else
            {
                allFid.Clear();
            }
        }
    }
   
}

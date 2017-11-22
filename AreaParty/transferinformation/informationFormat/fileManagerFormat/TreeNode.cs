using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation.informationFormat.fileManagerFormat
{
    // 指定路径的目录结构
    class TreeNode
    {
        private string fullName;
        private string name;
        private List<TreeNode> folders;
        private List<string> files;

        public void setFullName(string fullName)
        {
            this.fullName = fullName;
            this.name = fullName.Split('\\').Last();
            folders = new List<TreeNode>();
            files = new List<string>();
        }
        public void fillingList()
        {
            string[] folders = System.IO.Directory.GetDirectories(fullName);
            string[] files = System.IO.Directory.GetFiles(fullName);

            foreach (string file in files)
            {
                this.files.Add(file);
            }
            foreach (string folder in folders)
            {
                TreeNode tree = new TreeNode();
                tree.setFullName(folder);
                tree.fillingList();
                this.folders.Add(tree);
            }
        }

        public void copyTo(string targetPath, ref List<string> errorMessage)
        {
            targetPath = targetPath + @"\" + this.name;
            if (folders.Count == 0)
            {
                this.fillingList();
            }
            if (!System.IO.Directory.Exists(targetPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                    foreach (string file in this.files)
                    {
                        try
                        {
                            System.IO.File.Copy(file, targetPath + @"\" + file.Split('\\').Last(), false);
                        }
                        catch (Exception e1)
                        {
                            errorMessage.Add(e1.Message);
                        }
                    }
                    foreach (TreeNode tree in this.folders)
                    {
                        tree.copyTo(targetPath, ref errorMessage);
                    }
                }
                catch (Exception e)
                {
                    errorMessage.Add(e.Message);
                }
            }
            else
            {
                errorMessage.Add("要复制的文件已存在指定路径中");
            }

        }





    }
}

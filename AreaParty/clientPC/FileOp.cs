using System;
using System.IO;
using System.Text;

namespace client
{

	public class FileOp
	{
        /// <summary>
        /// 创建文件 </summary>
        /// <param name="fileName"> </param>
        /// <returns>  </returns>
        public static bool CreateFile(string fileName)
        {
            bool flag = false;
            try
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName).Close();//释放File句柄
                    flag = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                
            }
            return flag;
        }

        /// <summary>
        /// 读TXT文件内容 </summary>
        /// <param name="fileName"> </param>
        /// <returns>  </returns>
        public static string ReadTxtFile(string fileName)
        {
            string result = "";
            //System.IO.StreamReader fileReader = null;
            //System.IO.StreamReader bufferedReader = null;
            StreamReader streamReader = null;
            try
            {
                //fileReader = new System.IO.StreamReader(fileName);
                //bufferedReader = new System.IO.StreamReader(fileReader);
                streamReader = new StreamReader(fileName);
                try
                {
                    string read = null;
                    while ((read = streamReader.ReadLine()) != null)
                    {
                        result = result + read;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                //if (bufferedReader != null)
                //{
                //    bufferedReader.Close();
                //}
                //if (fileReader != null)
                //{
                //    fileReader.Close();
                //}
                streamReader.Close();
            }
            Console.WriteLine("读取出来的文件内容是：" + result);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool WriteTxtFile(string content, string fileName)
        {
            //RandomAccessFile mm = null;
            bool flag = false;
            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.Create);
                //fileStream.Write(content.GetBytes("GBK"), 0, content.GetBytes("GBK").length);
                streamWriter = new StreamWriter(fileStream);
                //fileStream.Seek(0, SeekOrigin.Begin);
                //开始写入
                streamWriter.Write(content);
                //清空缓冲区
                streamWriter.Flush();
                flag = true;
            }
            catch (Exception e)
            {
                // TODO: handle exception  
                Console.WriteLine(e.ToString());
            }
            finally
            {
                streamWriter.Close();
                fileStream.Close();
            }
            return flag;
        }
    }
}
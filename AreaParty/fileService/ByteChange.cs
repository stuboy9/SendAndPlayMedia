using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.fileServece
{
    public class ByteChange
    {
        
        //将long存入byte数组,使用小端模式
        public static void LongToByte(byte[] b,int start,long num)
        {
            for (int i = start; i < start + 8; i++)
            {
                b[i] = (byte)num;
                num >>= 8;
            }
        }

        public static long ByteToLong(byte[] b, int start)
        {
            long num = 0;
            for (int i = 7 + start; i >= start; i--)
            {
                num = num << 8;
                num += (0xff & (int)b[i]);
            }
            return num;
        }

        public static void IntToByte(byte[] b,int start, int num)
        {
            for (int i = start; i < start + 4; i++)
            {
                b[i] = (byte)num;
                num >>= 8;
            }
        }

        public static int ByteToInt(byte[] b, int start)
        {
            int num = 0;
            for (int i = start + 3 ; i >= start; i--)
            {
                num = num << 8;
                num += (0xff & (int)b[i]);
            }
            return num;
        }

        //将short存入byte数组,使用小端模式
        public static void ShortToByte(byte[] b, int start, short num)
        {
            for (int i = start; i < start + 2; i++)
            {
                b[i] = (byte)num;
                num >>= 8;
            }
        }
        //从byte数组中提取short,使用小端模式
        public static short ByteToShort(byte[] b, int start)
        {
            short num = 0;
            for (int i = 1 + start; i >= start; i--)
            {
                num = (short)(num << 8);
                num += (short)(0xff & b[i]);
            }
            return num;
        }
        //将String填充入数组
        public static void StringToByte(String str, byte[] b, int start, int length)
        {
            byte[] c = System.Text.Encoding.Default.GetBytes(str);
            if (length < c.Length)
                return;
            for (int i = 0; i < c.Length; i++)
            {
                b[start + i] = c[i];
            }
        }
        //将String填充入数组
        public static String ByteToString(byte[] b, int start, int length)
        {
            return System.Text.Encoding.Default.GetString(b,0,length);
        }
        //清空数组
        public static void CleanByte(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = 0;
            }
        }


    }
}

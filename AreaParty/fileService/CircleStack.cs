using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaParty.fileServece
{
    public class CircleStack
    {
        private int[] buffer;
        private int index;
        private int size;
        private bool full;

        public CircleStack(int size)
        {
            buffer = new int[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = 0;
            }
            index = 0;
            this.size = size;
            full = false;
        }

        public void Add(int value)
        {
            buffer[index++] = value;
            if (index == size)
            {
                index = 0;
                full = true;
            }
        }

        public int GetLast()
        {
            int index1;
            if (index > 0)
            {
                index1 = index - 1;
                return buffer[index1];
            }
            else if (full)
            {
                index1 = size - 1;
                return buffer[index1];
            }
            return -1;
        }

        public bool InIt(int value)
        {
            if (full)
            {
                for (int i = 0; i < size; i++)
                {
                    if (buffer[i] == value)
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < index; i++)
                {
                    if (buffer[i] == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

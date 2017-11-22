using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaPartyTests.fileService
{
    /// <summary>
    /// 获取一个随机数
    /// </summary>
    class MyRandom
    {
        int randomValue;
        public MyRandom(int start)
        {
            randomValue = start;
        }
        public int GetNextRandom() => randomValue++;
    }
}

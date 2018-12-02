using System;
using System.Collections.Generic;
using System.Text;

namespace SDFS
{
    public class UtilityMethods
    {
        public static int CopyByteToByte(byte[] Data, int DataIndex, byte[] Array, int ArrayIndex, int many)
        {
            int i = 0;
            int j = ArrayIndex;
            for (i = DataIndex; i < many + DataIndex && i < Data.Length; i++)
            {
                Array[j++] = Data[i];
            }
            while (j < many + ArrayIndex)
            {
                Array[j++] = 0;
            }
            return i;
        }
    }
}

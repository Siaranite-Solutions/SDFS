using System;
using System.Collections.Generic;
using System.Text;

namespace SDFS
{
    public class UtilityMethods
    {
        /// <summary>
        /// Converts a byte array into a hexadecimal string
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        public static String byteArrayToByteString(Byte[] inp)
        {
            Byte ch = 0x00;
            String ret = "";

            String[] pseudo = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "A", "B", "C", "D", "E", "F" };

            for (int i = 0; i < inp.Length; i++)
            {
                ret += "0x";
                ch = (byte)(inp[i] & 0xF0);
                ch = (byte)(ch >> 4);
                ch = (byte)(ch & 0x0F);

                ret += pseudo[(int)ch].ToString();

                ch = (byte)(inp[i] & 0x0F);
                ret += pseudo[(int)ch].ToString();

                ret += " ";
            }
            return ret;
        }

        /// <summary>
        /// Checks the specified string if it contains the specified char array
        /// </summary>
        /// <param name="instr"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static bool StringContains(String instr, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (Contains(instr, chars[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks the specified string if it contains the specified char
        /// </summary>
        /// <param name="instr"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool Contains(string instr, char p)
        {
            for (int i = 0; i < instr.Length; i++)
            {
                if (instr[i] == p)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copies the contents of one byte array into another starting at the specified index
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="DataIndex"></param>
        /// <param name="Array"></param>
        /// <param name="ArrayIndex"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static int CopyByteToByte(byte[] Data, int DataIndex, byte[] Array, int ArrayIndex, int Length)
        {
            int i = 0;
            int j = ArrayIndex;
            for (i = DataIndex; i < Length + DataIndex && i < Data.Length; i++)
            {
                Array[j++] = Data[i];
            }
            while (j < Length + ArrayIndex)
            {
                Array[j++] = 0;
            }
            return i;
        }

        /// <summary>
        /// Copies the contents of one byte array into another starting at the specified index
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="DataIndex"></param>
        /// <param name="Array"></param>
        /// <param name="arrind"></param>
        /// <param name="Length"></param>
        /// <param name="writeallzero"></param>
        /// <returns></returns>
        public static int CopyByteToByte(byte[] Data, int DataIndex, byte[] Array, int ArrayIndex, int Length, bool writeallzero)
        {
            int i = 0;
            int j = ArrayIndex;
            for (i = DataIndex; i < Length + DataIndex && i < Data.Length; i++)
            {
                Array[j++] = Data[i];
            }
            while (writeallzero && j < Length + ArrayIndex)
            {
                Array[j++] = 0;
            }
            return i;
        }

        /// <summary>
        /// Copies the contents of a char array into a byte array
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="dind"></param>
        /// <param name="arr"></param>
        /// <param name="arrind"></param>
        /// <param name="many"></param>
        /// <returns></returns>
        public static int CopyCharToByte(Char[] Data, int dind, byte[] arr, int arrind, int many)
        {
            int i = 0;
            int j = arrind;
            for (i = dind; i < many && i < Data.Length; i++)
            {
                arr[j++] = (byte)Data[i];
            }
            while (j < many)
            {
                arr[j++] = 0;
            }
            return i;
        }

        /// <summary>
        /// Copies the contents of a byte array into a char array
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="dind"></param>
        /// <param name="arr"></param>
        /// <param name="arrind"></param>
        /// <param name="many"></param>
        /// <returns></returns>
        public static int CopyByteToChar(byte[] Data, int dind, Char[] arr, int arrind, int many)
        {
            int i = 0;
            int j = arrind;
            for (i = dind; i < many && i < Data.Length; i++)
            {
                arr[j++] = (Char)Data[i];
            }
            while (j < many)
            {
                arr[j++] = '\0';
            }
            return i;
        }

        /// <summary>
        /// Converts a char[] array into a string and returns it
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CharToString(char[] text)
        {
            String ret = "";
            for (int i = 0; i < text.Length; i++)
            {
                ret += text[i];
            }
            return ret;
        }

        /// <summary>
        /// Currently date and time represented in a UNIX timestamp
        /// </summary>
        public static long UNIXTimeStamp
        {
            get
            {
                long ret = 0;
                long secondsinyear = 31536000;
                long secondsinday = 86400;
                for (int i = 1970; i < DateTime.Now.Year - 1; i++)
                {
                    ret += secondsinyear;
                    if ((i % 400 == 0 || (i % 4 == 0 && i % 100 != 0)))
                    {
                        ret += secondsinday;
                    }
                }
                for (int i = 1; i < DateTime.Now.Month - 1; i++)
                {
                    ret += MonthToDays[i] * secondsinday;
                    if (i == 2 && (DateTime.Now.Year % 400 == 0 || (DateTime.Now.Year % 4 == 0 && DateTime.Now.Year % 100 != 0)))
                    {
                        ret += secondsinday;
                    }
                }
                ret += DateTime.Now.Day * secondsinday;
                ret += DateTime.Now.Hour * 3600;
                ret += DateTime.Now.Minute * 60;
                ret += DateTime.Now.Second;
                return ret;
            }
        }

        /// <summary>
        /// Retrieves the number of days in each month
        /// </summary>
        private int[] MonthToDays
        {
            get
            {
                List<int> l = new List<int>();
                l.Add(31);
                l.Add(28);
                l.Add(31);
                l.Add(30);
                l.Add(31);
                l.Add(30);
                l.Add(31);
                l.Add(31);
                l.Add(30);
                l.Add(31);
                l.Add(30);
                l.Add(31);
                return l.ToArray();
            }
        }
    }
}

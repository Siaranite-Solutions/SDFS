using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace SDFS.Logical.Entries
{
    public class File : Entry
    {
        /// <summary>
        /// The full path (path + filename) of the current file
        /// </summary>
        public String Filename
        {
            get
            {
                return Filesystem.ConcatDirectory(_path, Name);
            }
        }

        /// <summary>
        /// Writes the specified byte array into the file
        /// </summary>
        /// <param name="Data"></param>
        public void WriteAllBytes(Byte[] Data)
        {
            if (sBlock.NextBlock != 0)
            {
                Filesystem.Clean(sBlock);
                sBlock.NextBlock = 0;
                Block.Write(Filesystem.cFS.mPartition, sBlock);
            }
            int index = 0;
            Block edge = Block.GetFreeBlock(Filesystem.cFS.mPartition);
            sBlock.NextBlock = edge.BlockNumber;
            Block.Write(_partition, sBlock);
            do
            {
                Byte[] arr = new byte[Block.MaxBlockContentSize];
                index = UtilityMethods.CopyByteToByte(Data, index, arr, 0, arr.Length);
                edge.Used = true;
                edge.Content = arr;
                if (index != Data.Length)
                {
                    Block fB = Block.GetFreeBlock(Filesystem.cFS.mPartition);
                    edge.NextBlock = fB.BlockNumber;
                    edge.ContentSize = (uint)arr.Length;
                    Block.Write(Filesystem.cFS.mPartition, edge);
                    edge = fB;
                }
                else
                {
                    edge.ContentSize = (uint)(Data.Length % arr.Length);
                    Block.Write(Filesystem.cFS.mPartition, edge);
                }
            }
            while (index != Data.Length);
            EditAttributes(EntryAttribute.DtM, UtilityMethods.UNIXTimeStamp);
            EditAttributes(EntryAttribute.DtA, UtilityMethods.UNIXTimeStamp);
        }

        /// <summary>
        /// Converts the specified text into a byte[] array, and writes the bytes to the file entry (WriteAllBytes())
        /// </summary>
        /// <param name="txt"></param>
        public void WriteAllText(String txt)
        {
            Byte[] tB = new byte[txt.Length];
            UtilityMethods.CopyCharToByte(txt.ToCharArray(), 0, tB, 0, txt.Length);
            WriteAllBytes(tB);
        }

        /// <summary>
        /// Returns the bytes stored in the file entry
        /// </summary>
        /// <returns></returns>
        public Byte[] ReadAllBytes()
        {
            if (sBlock.NextBlock == 0)
            {
                return new byte[0];
            }
            Block b = sBlock;
            List<Byte> bL = new List<Byte>();
            while (b.NextBlock != 0)
            {
                b = Block.Read(b.mPartition, b.NextBlock);
                for (int i = 0; i < b.ContentSize; i++)
                {
                    bL.Add(b.Content[i]);
                }
            }
            EditAttributes(EntryAttribute.DtA, UtilityMethods.UNIXTimeStamp);
            return bL.ToArray();
        }

        /// <summary>
        /// Converts the file entry's bytes into a string, and returns it
        /// </summary>
        /// <returns></returns>
        public string ReadAllText()
        {
            Byte[] b = ReadAllBytes();
            Char[] txt = new char[b.Length];
            UtilityMethods.CopyByteToChar(b, 0, txt, 0, b.Length);
            return UtilityMethods.CharToString(txt);
        }

        /// <summary>
        /// FS File object constructor
        /// </summary>
        /// <param name="part"></param>
        /// <param name="blockNumber"></param>
        /// <param name="path"></param>
        public File(Partition part, ulong blockNumber, String path)
        {
            _path = path;
            _partition = part;
            sBlock = Block.Read(part, blockNumber);
            if (!sBlock.Used)
            {
                sBlock.Used = true;
                String name = "New File";
                CreateEntry(_partition, sBlock, name);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SDFS.Physical;
using Cosmos.HAL.BlockDevice;

namespace SDFS.Logical
{
    public class Block
    {
        /// <summary>
        /// Maxmimum size of content which a Block can contain
        /// </summary>
        public static uint MaxBlockContentSize = 491;

        /// <summary>
        /// This block's filesystem partition (private)
        /// </summary>
        private Partition _Partition;

        /// <summary>
        /// Block number (private)
        /// </summary>
        private ulong _blockNo = 0;

        /// <summary>
        /// Boolean value - is this block in use?
        /// </summary>
        private bool _Used = false;

        /// <summary>
        /// Content size of the block
        /// </summary>
        private uint _cSize = 0;

        /// <summary>
        /// Total size of the block (private)
        /// </summary>
        private ulong _tSize = 0;
        
        /// <summary>
        /// Next available block allocation
        /// </summary>
        private ulong _nBlock = 0;

        /// <summary>
        /// The partition holding the block
        /// </summary>
        public Partition mPartition
        {
            get
            {
                return _Partition;
            }
        }

        /// <summary>
        /// The number of the current block
        /// </summary>
        public ulong BlockNumber
        {
            get
            {
                return _blockNo;
            }
        }

        /// <summary>
        /// Bool whether block used or not used
        /// </summary>
        public bool Used
        {
            get
            {
                return _Used;
            }
            set
            {
                _Used = value;
            }
        }

        /// <summary>
        /// The current content size
        /// </summary>
        public uint ContentSize
        {
            get
            {
                return _cSize;
            }
            set
            {
                _cSize = value;
            }
        }

        /// <summary>
        /// Total size of the Entry
        /// </summary>
        public ulong TotalSize
        {
            get
            {
                return _tSize;
            }
            set
            {
                _tSize = value;
            }
        }

        /// <summary>
        /// The next block number to read al the content
        /// </summary>
        public ulong NextBlock
        {
            get
            {
                return _nBlock;
            }
            set
            {
                _nBlock = value;
            }
        }

        /// <summary>
        /// The Content Byte Array. TO-DO: code this better
        /// </summary>
        public Byte[] Content;

        /// <summary>
        /// Creates a new virtual Block.
        /// </summary>
        /// <param name="Data">The Byte data</param>
        /// <param name="p">The partition to use</param>
        /// <param name="bn">The block number</param>
        public Block(Byte[] Data, Partition p, ulong bn)
        {
            _blockNo = bn;
            _Partition = p;
            Content = new Byte[Data.Length - 21];
            if (Data[0] == 0x00)
            {
                _Used = false;
                for (int i = 0; i < Content.Length; i++)
                {
                    Content[i] = 0;
                }
            }
            else
            {
                _Used = true;
                _cSize = BitConverter.ToUInt32(Data, 1);
                _tSize = BitConverter.ToUInt64(Data, 5);
                _nBlock = BitConverter.ToUInt64(Data, 13);
                for (int i = 21; i < Data.Length; i++)
                {
                    Content[i - 21] = Data[i];
                }
            }
        }

        /// <summary>
        /// The
        /// </summary>
        /// <param name="p">The partition to read</param>
        /// <param name="bn">The blocknumber to read</param>
        public static Block Read(Partition p, ulong bn)
        {
            Byte[] data = p.NewBlockArray(1);
            p.ReadBlock(bn, 1, data);
            return new Block(data, p, bn);
        }

        /// <summary>
        /// Writes a block into a specific partition
        /// </summary>
        /// <param name="p">The partition to write to</param>
        /// <param name="b">The block to write</param>
        public static void Write(Partition p, Block b)
        {
            Byte[] data = new Byte[p.BlockSize];
            int index = 0;
            if (b.Used)
            {
                data[index++] = 0x01;
            }
            else
            {
                data[index++] = 0x00;
            }
            Byte[] x = BitConverter.GetBytes(b.ContentSize);
            for (int i = 0; i < x.Length; i++)
            {
                data[index++] = x[i];
            }
            x = BitConverter.GetBytes(b.TotalSize);
            for (int i = 0; i < x.Length; i++)
            {
                data[index++] = x[i];
            }
            x = BitConverter.GetBytes(b.NextBlock);
            for (int i = 0; i < x.Length; i++)
            {
                data[index++] = x[i];
            }
            x = b.Content;
            for (int i = 0; i < x.Length; i++)
            {
                data[index++] = x[i];
            }
            p.WriteBlock(b.BlockNumber, 1, data);
        }

        /// <summary>
        /// Get the next free block from the selected partition (TO-DO: Implement something that runs faster)
        /// </summary>
        /// <param name="p">The partition to get the block from</param>
        public static Block GetFreeBlock(Partition p)
        {
            for (ulong i = 1; i < p.BlockCount; i++)
            {
                Block b = Read(p, i);
                if (!b.Used)
                {
                    return b;
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;

namespace SDFS.Physical
{
    public class PrimaryPartition : Partition
    {
        /// <summary>
        /// The primary partition's block device
        /// </summary>
        BlockDevice HostDevice;

        /// <summary>
        /// The primary partition's starting sector
        /// </summary>
        ulong StartingSector;

        /// <summary>
        /// Partition Information instance for this partition
        /// </summary>
        private PartitionObj _Partition;

        /// <summary>
        /// Readonly public property for partition info instance
        /// </summary>
        public PartitionObj Info
        {
            get
            {
                return _Partition;
            }
        }

        /// <summary>
        /// Constructor for a primary partition
        /// </summary>
        /// <param name="aHost"></param>
        /// <param name="aStartingSector"></param>
        /// <param name="aSectorCount"></param>
        /// <param name="info"></param>
        public PrimaryPartition(BlockDevice aHost, ulong aStartingSector, ulong aSectorCount, PartitionObj info)
            : base(aHost, aStartingSector, aSectorCount)
        {
            HostDevice = aHost;
            StartingSector = aStartingSector;
            mBlockCount = aSectorCount;
            mBlockSize = aHost.BlockSize;
            _Partition = info;
        }


        /// <summary>
        /// Reads the specified number of blocks from this partition
        /// </summary>
        /// <param name="aBlockNo">Starting block number</param>
        /// <param name="aBlockCount">Number of blocks to read</param>
        /// <param name="aData">byte[] array to read to</param>
        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            UInt64 HostBlockNumber = StartingSector + aBlockNo;
            CheckBlockNo(HostBlockNumber, aBlockCount);
            HostDevice.ReadBlock(HostBlockNumber, aBlockCount, aData);
        }

        /// <summary>
        /// Writes the specified byte[] array to the specified block and number of blocks
        /// </summary>
        /// <param name="aBlockNo">Starting block number</param>
        /// <param name="aBlockCount">Number of blocks to write to</param>
        /// <param name="aData">Byte[] array containing data to be written</param>
        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            UInt64 HostBlockNumber = StartingSector + aBlockNo;
            CheckBlockNo(HostBlockNumber, aBlockCount);
            HostDevice.WriteBlock(HostBlockNumber, aBlockCount, aData);
        }
    }
}

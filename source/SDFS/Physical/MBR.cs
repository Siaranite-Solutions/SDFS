using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;

namespace SDFS.Physical
{
    public class MBR
    {
        /// <summary>
        /// An array containing all patitions
        /// </summary>
        public PartitionObj[] Partitions
        {
            get
            {
                return mPartitions.ToArray();
            }
        }

        /// <summary>
        /// List of partitions found on the Master Boot Record
        /// </summary>
        private List<PartitionObj> mPartitions = new List<PartitionObj>();

        /// <summary>
        /// The block device on which this MBR resides
        /// </summary>
        private BlockDevice mBlockDevice;

        /// <summary>
        /// Constructor for a Master Boot Record
        /// </summary>
        /// <param name="mbr"></param>
        /// <param name="aBlockDevice"></param>
        public MBR (Byte[] aMBR, BlockDevice aBlockDevice)
        {
            mBlockDevice = aBlockDevice;
            UtilityMethods.CopyByteToByte(aMBR, 0, BootArray, 0, 440);
            Signature = BitConverter.ToUInt32(aMBR, 440);
            // Partition entries are located at MBR offsets 446, 462, 478 and 494
            // MBR only consists of 512 bytes
            ReadPartitions(aMBR, 446);
            ReadPartitions(aMBR, 462);
            ReadPartitions(aMBR, 478);
            ReadPartitions(aMBR, 494);
        }

        /// <summary>
		/// The Byte Array containing the bootable code and partition information of the MBR
		/// </summary>
		public readonly Byte[] BootArray = new Byte[440];

        /// <summary>
		/// The Disk signature
		/// </summary>
		public readonly UInt32 Signature = 0;

        /// <summary>
        /// Reads the Master Boot Record for partition entries, and parses information such as sector count, block size and partition ID
        /// </summary>
        /// <param name="MBR"></param>
        /// <param name="aLocation"></param>
        private void ReadPartitions(byte[] MBR, UInt32 aLocation)
        {
            byte xPartitionID = MBR[aLocation + 4];
            // A partition ID of 0 identifies an empty (null) partition
            if (xPartitionID != 0)
            {
                UInt32 xStartSector = BitConverter.ToUInt32(MBR, (int)aLocation + 8);
                UInt32 xSectorCount = BitConverter.ToUInt32(MBR, (int)aLocation + 12);

                var xPartitionObject = new PartitionObj(xPartitionID, xStartSector, xSectorCount, mBlockDevice.BlockSize);
                mPartitions.Add(xPartitionObject);
            }
        }
    }
}

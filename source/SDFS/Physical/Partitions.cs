using System;
using System.Collections.Generic;
using System.Text;

namespace SDFS.Physical
{
    public class PartitionObj
    {
        /// <summary>
		/// Constructor for a new partition
		/// </summary>
		/// <param name="SysID"></param>
		/// <param name="startSector"></param>
		/// <param name="sectCount"></param>
		public PartitionObj(byte SysID, UInt32 startSector, UInt32 sectCount, UInt64 blockSize)
        {
            SystemID = SysID;
            StartSector = startSector;
            SectorCount = sectCount;
            BlockSize = blockSize;
        }

        /// <summary>
        /// Returns the total size of this partition in megabytes
        /// </summary>
        public UInt32 TotalSize
        {
            get
            {
                return (uint)(((BlockSize * SectorCount) / 1024) / 1024);
            }
        }

        /// <summary>
        /// Block size of this partition
        /// </summary>
        public readonly UInt64 BlockSize;

        /// <summary>
        /// The partition's FS ID
        /// </summary>
        public readonly byte SystemID;
        /// <summary>
        /// The partition's starting sector
        /// </summary>
        public readonly UInt32 StartSector;
        /// <summary>
        /// The number of sectors in the partition
        /// </summary>
        public readonly UInt32 SectorCount;
    }
}

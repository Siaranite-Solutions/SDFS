using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;

namespace SDFS.Physical
{
    public class MBR
    {
        /// <summary>
        /// The block device on which this MBR resides
        /// </summary>
        private BlockDevice mBlockDevice;

        /// <summary>
        /// Constructor for a Master Boot Record
        /// </summary>
        /// <param name="mbr"></param>
        /// <param name="aBlockDevice"></param>
        public MBR (Byte[] mbr, BlockDevice aBlockDevice)
        {
            this.mBlockDevice = aBlockDevice;
        }

        /// <summary>
		/// The Byte Array containing the bootable code and partition information of the MBR
		/// </summary>
		public readonly Byte[] Bootable = new Byte[440];

        /// <summary>
		/// The Disk signature
		/// </summary>
		public readonly UInt32 Signature = 0;


    }
}

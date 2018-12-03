using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL.BlockDevice;
using SDFS.Physical;

namespace SDFS_Demo
{
    class DiskHandler
    {
        /// <summary>
        /// Partitioning function for devices that present a choise of the device to partition.
        /// </summary>
        /// <param name="list">The list of the devices to choose from</param>
        public static void CreatePartitions(SDFS.Physical.IDE[] list)
        {
            SDFS.Physical.IDE Device = null;
            int partnum = 0;
            ulong DispCount = 0;
            Console.WriteLine("SDFS Partitioning Tool");
            do
            {
                Console.WriteLine("Which device do you want to use?");
                for (int i = 0; i < list.Length; i++)
                {
                    Console.WriteLine(" --- Device N." + (i + 1) + " Size: approximately " + (uint)((((list[i].BlockSize * list[i].BlockCount) / 1024) / 1024) + 1) + " MB");
                }
                Console.Write("Insert Number: ");
                String nums = Console.ReadLine();
                int num = int.Parse(nums) - 1;
                if (num >= 0 && num < list.Length)
                {
                    Device = list[num];
                    DispCount = list[num].BlockCount - 1;
                }
            } while (Device == null);
            Console.WriteLine("How many primary partitions? (Max. 4)");
            do
            {
                Console.Write("Insert Number: ");
                String nums = Console.ReadLine();
                partnum = int.Parse(nums);
            } while (partnum == 0 || partnum > 4);
            uint mbrpos = 446;
            Byte[] type = new Byte[] { 0x00, 0x00, 0x00, 0x00 };
            uint[] StartBlock = new uint[] { 1, 0, 0, 0 };
            uint[] BlockNum = new uint[] { 0, 0, 0, 0 };
            for (int i = 0; i < partnum; i++)
            {
                type[i] = 0xFA;
                Console.Write("How many blocks for Partition N. " + (i + 1) + "? (Max: " + ((uint)(DispCount - (uint)(partnum - (i + 1)))).ToString() + "): ");
                String nums = Console.ReadLine();
                uint num = (uint)int.Parse(nums);
                if (num >= 0 && num <= DispCount - (uint)(partnum - (i + 1)))
                {
                    BlockNum[i] = num;
                    if (i < partnum - 1)
                    {
                        StartBlock[i + 1] = (num) + StartBlock[i];
                    }
                    DispCount = DispCount - (num);
                }
                else
                {
                    i--;
                }
            }
            Byte[] data = Device.NewBlockArray(1);
            Device.ReadBlock(0, 1, data);
            for (int i = 0; i < 4; i++)
            {
                mbrpos += 4;
                data[mbrpos] = type[i];
                mbrpos += 4;
                Byte[] b = BitConverter.GetBytes(StartBlock[i]);
                SDFS.UtilityMethods.CopyByteToByte(b, 0, data, (int)mbrpos, b.Length);
                mbrpos += 4;
                b = BitConverter.GetBytes(BlockNum[i]);
                SDFS.UtilityMethods.CopyByteToByte(b, 0, data, (int)mbrpos, b.Length);
                mbrpos += 4;
                Device.WriteBlock(StartBlock[i], 1, Device.NewBlockArray(1));
                Console.WriteLine("Partition N. " + (i + 1) + " Start: " + (StartBlock[i]).ToString() + " BlockCount: " + (BlockNum[i]).ToString());
            }
            Device.WriteBlock(0, 1, data);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using SDFS;
using SDFS.Logical;
using SDFS.Physical;

namespace SDFS_Demo
{
    public class Kernel : Sys.Kernel
    {
        /// <summary>
        /// Partition containing a valid SDFS partition
        /// </summary>
        private static PrimaryPartition workPartition = null;

        protected override void BeforeRun()
        {
            IDE[] IDEs = IDE.Devices.ToArray();
            Console.WriteLine("Number of IDE disks: " + IDEs.Length);
            Console.WriteLine("Looking for valid partitions...");
            for (int i = 0; i < IDEs.Length; i++)
            {
                PrimaryPartition[] parts = IDEs[i].PrimaryPartitions;
                for (int j = 0; j < parts.Length; j++)
                {
                    if (parts[j].Info.SystemID == 0xFA)
                    {
                        workPartition = parts[j];
                    }
                }
            }
            //#warning Revert to == null!!!
            if (workPartition == null)
            {
                DiskHandler.CreatePartitions(IDEs);
                Console.WriteLine("The machine needs to be restarted.");
                Console.ReadKey(true);
                Sys.Power.Reboot();
            }

            Console.Write("Checking FileSystem... ");
            Filesystem fs;
            try
            {
                fs = new Filesystem(workPartition);
                Filesystem.MapFilesystem(fs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!" + ex.Message);
            }
        }

        protected override void Run()
        {
            Console.WriteLine(workPartition.Info.TotalSize + "MBs");
            Console.WriteLine("Type: " + workPartition.Info.SystemID.ToString());
            Console.ReadKey(true);
        }
    }
}
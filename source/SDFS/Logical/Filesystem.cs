using System;
using SDFS.Physical;
using Cosmos.HAL.BlockDevice;
using SDFS.Logical.Entries;

namespace SDFS.Logical
{
    public class Filesystem
    {
        public Directory Root
        {
            get
            {
                return new Directory(_Partition, 1, Separator);
            }
        }

        /// <summary>
        /// Concatenates a file and it's path location, resulting in full path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fname"></param>
        /// <returns>Full path location of file</returns>
        public static string ConcatFile(string path, string fname)
        {
            String file = "";
            file = path.TrimEnd(Separator.ToCharArray());
            if (file == null)
            {
                file = "";
            }
            if (fname != Separator)
            {
                file += file += Separator + fname;
            }
            else
            {
                file = Separator;
            }
            return file;
        }

        /// <summary>
        /// Concatenates a directory and it's path location, resulting in full path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dir"></param>
        /// <returns>Full path location of directory</returns>
        public static string ConcatDirectory(string path, string dir)
        {
            String directory = "";
            directory = path.TrimEnd(Separator.ToCharArray());
            if (directory == null)
            {
                directory = "";
            }
            if (dir != Separator)
            {
                directory += Separator + dir + Separator;
            }
            else
            {
                directory = Separator;
            }
            return directory;
        }

        /// <summary>
        /// The filesystem that is currently in use
        /// </summary>
        public static Filesystem cFS = null;

        /// <summary>
        /// Constructor for a filesystem object
        /// Runs test for valid FS on specified partition
        /// </summary>
        public Filesystem(Partition aPartition)
        {
            _Partition = aPartition;
            if (!IsValidFS())
            {
                // If unable to detect a valid partition
                if (!GenerateFS())
                {
                    // Error - unable to create a new filesystem on specified partition
                    throw new Exception("Unable to create a new filesystem on the specified partition");
                }
            }
        }

        /// <summary>
        /// Separator character that splits files/directories from their parent directory.
        /// </summary>
        public static string Separator = "/";

        /// <summary>
        /// Property of the partition upon which this filesystem resides
        /// </summary>
        private Partition _Partition;

        private byte[] fsSignature = new byte[]
        {
            0x4D,
            0x65,
            0x64,
            0x6C,
            0x69,
            0x44,
            0x46,
            0x53
        };

        /// <summary>
        /// Public property of the partition upon which this filesystem resides
        /// </summary>
        public Partition mPartition
        {
            get
            {
                return _Partition;
            }
        }

        /// <summary>
        /// Returns block size of this FileSystem's partition
        /// </summary>
        public ulong BlockSize
        {
            get
            {
                return _Partition.BlockSize;
            }
        }

        /// <summary>
        /// Returns block count of this FileSystem's partition
        /// </summary>
        public ulong BlockCount
        {
            get
            {
                return _Partition.BlockCount;
            }
        }

        /// <summary>
        /// Map a specified filesystem to the methods and properties of SDFS
        /// </summary>
        /// <param name="fs"></param>
        public static void MapFilesystem (Filesystem fs)
        {
            cFS = fs;
        }


        /// <summary>
        /// Generates a new FS block structure on specified filesystem
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private bool GenerateFS(Partition part)
        {
            Refresh(10000);
            Byte[] data = part.NewBlockArray(1);
            fsSignature.CopyTo(data, 0);
            for (int i = fsSignature.Length; i < fsSignature.Length; i++)
            {
                data[i] = fsSignature[i];
            }
            part.WriteBlock(0, 1, data);
            return true;
        }

        /// <summary>
        /// Generates a new FS block structure on pre-set filesystem
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private bool GenerateFS()
        {
            Refresh(10000);
            Byte[] data = _Partition.NewBlockArray(1);
            fsSignature.CopyTo(data, 0);
            for (int i = fsSignature.Length; i < fsSignature.Length; i++)
            {
                data[i] = fsSignature[i];
            }
            _Partition.WriteBlock(0, 1, data);
            return true;
        }

        /// <summary>
        /// Tests to see if partition object (_partition) contains a valid MDFS filesystem block structure (does the partition signature match the ASCII string 'MedliDFS')
        /// </summary>
        /// <returns></returns>
        public bool IsValidFS()
        {
            Byte[] data = _Partition.NewBlockArray(1);
            for (int i = 0; i < fsSignature.Length; i++)
            {
                if (fsSignature[i] != data[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Cleans a filesystem by a specified amount of blocks
        /// (default is BlockCount)
        /// </summary>
        /// <param name="sBlock"></param>
        public void Refresh(ulong sBlock = 0)
        {
            Byte[] data = _Partition.NewBlockArray(1);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
            // Proceeding to refresh the filesystem...
            ulong lBlock = _Partition.BlockCount;
            if (sBlock != 0)
            {
                lBlock = sBlock;
            }
            ulong rate = lBlock / 100;
            uint perc = 0;
            // perc + "% refreshed. " + (uint)lBlock + " blocks remaining.
            for (ulong i = 0; i < lBlock; i++)
            {
                mPartition.WriteBlock(i, 1, data);
                if (i % rate == 0)
                {
                    perc++;
                }
                if (i % 32 == 0)
                {
                    // perc + "% refreshed. " + ((uint)(lBlock - i)) + " blocks remaining.
                }
            }
            // Successfully refreshed filesystem.
        }

        public static void Clean(Block sBlock)
        {
            Block block = sBlock;
            while (block.NextBlock != 0)
            {
                block = Block.Read(block.mPartition, block.NextBlock);
                block.Used = false;
                Block.Write(cFS.mPartition, block);
            }
        }
    }
}

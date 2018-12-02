using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;
using SDFS.Physical;

namespace SDFS.Logical.Entries
{
    public class Directory : Entry
    {
        /// <summary>
        /// The full path (path+Name) of the current Directory
        /// </summary>
        public String DirectoryName
        {
            get
            {
                return Filesystem.ConcatDirectory(_path, Name);
            }
        }

        /// <summary>
        /// Creates a new Directory Object
        /// </summary>
        /// <param name="partition">The partition to use</param>
        /// <param name="blockNumber">The block number we want to use</param>
        /// <param name="path">The path of the new directory</param>
        public Directory(Partition partition, ulong blockNumber, String path)
        {
            _path = path;
            _partition = partition;
            sBlock = Block.Read(partition, blockNumber);
            if (blockNumber == 1 && path == "/" && sBlock.Content[0] != '/')
            {
                Char[] nm = "/".ToCharArray();
                for (int i = 0; i < nm.Length; i++)
                {
                    sBlock.Content[i] = (byte)nm[i];
                }
                sBlock.Used = true;
                sBlock.NextBlock = 0;
                Block.Write(partition, sBlock);
            }
            if (!sBlock.Used)
            {
                sBlock.Used = true;
                String n = "New Directory";
                if (path == Filesystem.Separator)
                {
                    _path = "";
                    n = path;
                }
                CreateEntry(partition, sBlock, n);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Directory[] RetrieveDirectories()
        {
            Block curb = sBlock;
            List<Directory> d = new List<Directory>();
            while (curb.NextBlock != 0)
            {
                int index = 0;
                curb = Block.Read(sBlock.mPartition, sBlock.NextBlock);
                while (index < curb.ContentSize)
                {
                    ulong a = BitConverter.ToUInt64(curb.Content, index);
                    index += 8;
                    uint sep = BitConverter.ToUInt32(curb.Content, index);
                    index += 4;
                    if (sep == 1)
                    {
                        d.Add(new Directory(_partition, a, Filesystem.ConcatDirectory(_path, Name)));
                    }
                }
            }
            return d.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public File[] RetrieveFiles()
        {
            Block curb = sBlock;
            List<File> d = new List<File>();
            while (curb.NextBlock != 0)
            {
                int index = 0;
                curb = Block.Read(sBlock.mPartition, sBlock.NextBlock);
                while (index < curb.ContentSize)
                {
                    ulong a = BitConverter.ToUInt64(curb.Content, index);
                    index += 8;
                    uint sep = BitConverter.ToUInt32(curb.Content, index);
                    index += 4;
                    if (sep == 1)
                    {
                        d.Add(new File(_partition, a, Filesystem.ConcatDirectory(_path, Name)));
                    }
                }
            }
            return d.ToArray();
        }

        /// <summary>
        /// Retrieves the entries stored in the directory
        /// </summary>
        /// <returns></returns>
        public Entry[] RetrieveEntries()
        {
            Block edge = sBlock;
            List<Entry> entries = new List<Entry>();
            while (edge.NextBlock != 0)
            {
                int index = 0;
                edge = Block.Read(sBlock.mPartition, sBlock.NextBlock);
                while (index < edge.ContentSize)
                {
                    ulong a = BitConverter.ToUInt64(edge.Content, index);
                    index += 8;
                    uint sep = BitConverter.ToUInt32(edge.Content, index);
                    index += 4;
                    if (sep == 1)
                    {
                        entries.Add(new Directory(_partition, a, Filesystem.ConcatDirectory(_path, Name)));
                    }
                    else if (sep == 2)
                    {
                        entries.Add(new File(_partition, a, Filesystem.ConcatDirectory(_path, Name)));
                    }
                }
            }
            return entries.ToArray();
        }

        public void AddDirectory(String Name)
        {
            Entry[] directories = RetrieveEntries();
            for (int i = 0; i < directories.Length; i++)
            {
                if (directories[i].Name == Name)
                {
                    // The directory already exists!
                    return;
                }
            }
            Block edge = EditBlock();
            Block nDirB = CreateEntry(_partition, Name);
            BitConverter.GetBytes(nDirB.BlockNumber).CopyTo(edge.Content, edge.ContentSize);
            BitConverter.GetBytes((uint)1).CopyTo(edge.Content, edge.ContentSize + 8);
            edge.ContentSize += 12;
            Block.Write(_partition, edge);
            EditAttributes(EntryAttribute.DtM, UtilityMethods.UNIXTimeStamp);
            EditAttributes(EntryAttribute.DtA, UtilityMethods.UNIXTimeStamp);
        }


        /// <summary>
        /// Gets the last NoobFSBlock of the directory
        /// </summary>
        private Block EditBlock()
        {
            Block ret = sBlock;
            while (ret.NextBlock != 0)
            {
                ret = Block.Read(sBlock.mPartition, sBlock.NextBlock);
            }
            if (ret.BlockNumber == sBlock.BlockNumber)
            {
                ret = Block.GetFreeBlock(_partition);
                ret.Used = true;
                ret.ContentSize = 0;
                ret.NextBlock = 0;
                sBlock.NextBlock = ret.BlockNumber;
                Block.Write(_partition, sBlock);
                Block.Write(_partition, ret);
            }
            if (_partition.NewBlockArray(1).Length - ret.ContentSize < 12)
            {
                Block block = Block.GetFreeBlock(_partition);
                if (block == null)
                {
                    return null;
                }
                ret.NextBlock = block.BlockNumber;
                Block.Write(_partition, ret);
                block.Used = true;
                ret = block;
            }
            return ret;
        }

        /// <summary>
        /// Get the directory specified by the Fullname passed
        /// </summary>
        /// <param name="fn">The fullname of the directory</param>
        public static Directory GetDirectoryByFullName(String fn)
        {
            Directory dir = Filesystem.cFS.Root;
            if (fn == dir.Name)
            {
                return dir;
            }
            if (fn == null || fn == "")
            {
                return null;
            }
            String[] names = fn.Split('/');
            if (names[0] != "")
            {
                return null;
            }
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] != null && names[i] != "")
                {
                    dir = dir.GetDirectoryByName(names[i]);
                    if (dir == null)
                    {
                        break;
                    }
                }
            }
            return dir;
        }

        /// <summary>
        /// Get the directory specified by the Name passed
        /// </summary>
        /// <param name="n">The name of the child directory</param>
        public Directory GetDirectoryByName(String n)
        {
            Directory[] dirs = RetrieveDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Name == n)
                {
                    return dirs[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Overrides the ToString Method.
        /// </summary>
        public override String ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Get the directory specified by the Fullname passed
        /// </summary>
        /// <param name="fn">The fullname of the directory</param>
        public static File GetFileByFullName(String fn)
        {
            Directory dir = new Directory(Filesystem.cFS.mPartition, 1, Filesystem.Separator);
            if (fn == null || fn == "")
            {
                return null;
            }
            String[] names = fn.Split('/');
            for (int i = 0; i < names.Length - 1; i++)
            {
                if (names[i] != "")
                {
                    dir = dir.GetDirectoryByName(names[i]);
                    if (dir == null)
                    {
                        break;
                    }
                }
            }
            return dir.RetrieveFileByName(names[names.Length - 1]);
        }

        /// <summary>
        /// Get the file specified by the Name passed
        /// </summary>
        /// <param name="n">The name of the child file</param>
        public File RetrieveFileByName(String n)
        {
            File[] files = RetrieveFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name == n)
                {
                    return files[i];
                }
            }
            return null;
        }
    }
}

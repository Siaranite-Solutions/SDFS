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


        /// <summary>
        /// Creates a new File to the current directory
        /// </summary>
        /// <param name="Name">The new File's name</param>
        public void AddFile(String Name)
        {
            Entry[] dirs = RetrieveEntries();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Name == Name)
                {
                    // Entry with same Name already exists!
                    return;
                }
            }
            Block curb = EditBlock();
            Block newfileb = CreateEntry(_partition, Name);
            //if (newfileb != null)
            //{
            BitConverter.GetBytes(newfileb.BlockNumber).CopyTo(curb.Content, curb.ContentSize);
            BitConverter.GetBytes((uint)2).CopyTo(curb.Content, curb.ContentSize + 8);
            curb.ContentSize += 12;
            Block.Write(_partition, curb);
            EditAttributes(EntryAttribute.DtM, UtilityMethods.UNIXTimeStamp);
            EditAttributes(EntryAttribute.DtA, UtilityMethods.UNIXTimeStamp);
            //}
        }

        /// <summary>
        /// Permits to remove a Directory by passing it's name
        /// </summary>
        /// <param name="Name">The Directory's name to remove</param>
        public void RemoveDirectory(String Name)
        {
            Directory[] dirs = RetrieveDirectories();
            bool found = false;
            int index = 0;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Name == Name)
                {
                    index = i;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                RemoveDirectory(dirs[index]);
            }
        }

        /// <summary>
        /// Permits to remove a Directory by passing it
        /// </summary>
        /// <param name="Directory">The Directory to remove</param>
        private void RemoveDirectory(Directory Directory)
        {
            Directory[] subdirs = Directory.RetrieveDirectories();
            for (int i = 0; i < subdirs.Length; i++)
            {
                Directory.RemoveDirectory(subdirs[i]);
            }
            File[] subfiles = Directory.RetrieveFiles();
            for (int i = 0; i < subdirs.Length; i++)
            {
                Directory.RemoveFile(subfiles[i].Name);
            }
            Filesystem.Clean(Directory.sBlock);
            DeleteBlock(Directory.sBlock);
        }

        /// <summary>
        /// Permits to remove a File by passing it's name
        /// </summary>
        /// <param name="Name">The File's name to remove</param>
        public void RemoveFile(String Name)
        {
            File[] files = RetrieveFiles();
            bool found = false;
            int index = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name == Name)
                {
                    index = i;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                Filesystem.Clean(files[index].StartBlock);
                DeleteBlock(files[index].StartBlock);
            }
        }

        /// <summary>
        /// Permits to remove a Block by passing it
        /// </summary>
        /// <param name="Directory">The Block to remove</param>
        private void DeleteBlock(Block FSBlock)
        {
            Block curb = sBlock;
            while (curb.NextBlock != 0)
            {
                int index = 0;
                bool found = false;
                List<Byte> cont = new List<Byte>();
                curb = Block.Read(sBlock.mPartition, sBlock.NextBlock);
                while (index < curb.ContentSize)
                {
                    ulong a = BitConverter.ToUInt64(curb.Content, index);
                    Byte[] app = BitConverter.GetBytes(a);
                    for (int i = 0; i < app.Length; i++)
                    {
                        cont.Add(app[i]);
                    }
                    index += 8;
                    uint sep = BitConverter.ToUInt32(curb.Content, index);
                    index += 4;
                    if (a == FSBlock.BlockNumber)
                    {
                        app = BitConverter.GetBytes((uint)0);
                        for (int i = 0; i < app.Length; i++)
                        {
                            cont.Add(app[i]);
                        }
                        found = true;
                    }
                    else
                    {
                        app = BitConverter.GetBytes(sep);
                        for (int i = 0; i < app.Length; i++)
                        {
                            cont.Add(app[i]);
                        }
                    }
                }
                if (found)
                {
                    curb.Content = cont.ToArray();
                    curb.ContentSize = (uint)cont.Count;
                    Block.Write(_partition, curb);
                }
            }
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
        /// Gets the last Block of the directory
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

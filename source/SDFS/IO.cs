using System;
using System.Collections.Generic;
using System.Text;
using SDFS_Directory = SDFS.Logical.Entries.Directory;
using SDFS_File = SDFS.Logical.Entries.File;
using SDFS.Logical.Entries;
using SDFS.Logical;


namespace SDFS.IO
{
    public class Directory
    {
        private static Logical.Entries.Directory sf;

        /// <summary>
        /// Creates the specified directory
        /// </summary>
        /// <param name="directory"></param>
        public static void Create(string directory)
        {
            Filesystem.cFS.Root.AddDirectory(directory);
            sf = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (sf == null)
            {
                // Cannot create required files, aborting creation of directory
                throw new Exception("Failed creation of directory.");
            }
        }

        /// <summary>
        /// Creates the specified directory
        /// </summary>
        /// <param name="directory"></param>
        public static void Remove(string directory)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                Filesystem.cFS.Root.RemoveDirectory(directory);
            }
            else
            {
                // Cannot create required files, aborting creation of directory
                throw new Exception("Failed deletion of directory.");
            }
        }
    }

    public class File
    {
        /// <summary>
        /// Creates the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        public static void Create(string file, string directory)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                SDFS_File nf = dir.RetrieveFileByName(file);
                if (nf == null)
                {
                    throw new Exception("Could not create!");
                }

            }
            else
            {
                throw new ArgumentException("Bad directory");
            }
        }

        public static void Delete(string file, string directory)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                SDFS_File mFile = dir.RetrieveFileByName(file);
                if (mFile != null)
                {
                    dir.RemoveFile(file);
                }
                else
                {
                    throw new Exception("Could not delete file!");
                }
            }
        }

        /// <summary>
        /// Writes the specified byte array into the specified path and filename
        /// if the file doesn't exist - create it
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        /// <param name="data"></param>
        public static void WriteAllBytes(string file, string directory, byte[] data)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                SDFS_File nf = dir.RetrieveFileByName(file);
                if (nf == null)
                {
                    throw new Exception("Could not create!");
                }
                else
                {
                    nf.WriteAllBytes(data);
                }

            }
            else
            {
                throw new ArgumentException("Bad directory");
            }
        }
        /// <summary>
        /// Writes the specified string into the specified path and filename
        /// if the file doesn't exist - create it
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        /// <param name="data"></param>
        public static void WriteAllText(string file, string directory, string data)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                SDFS_File nf = dir.RetrieveFileByName(file);
                if (nf == null)
                {
                    throw new Exception("Could not create!");
                }
                else
                {
                    nf.WriteAllText(data);
                }

            }
            else
            {
                throw new ArgumentException("Bad directory");
            }
        }



        /// <summary>
        /// Writes the specified byte array into the specified path and filename
        /// if the file doesn't exist - create it
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        /// <param name="data"></param>
        public static byte[] ReadAllBytes(string file, string directory)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                SDFS_File nf = dir.RetrieveFileByName(file);
                if (nf == null)
                {
                    throw new Exception("Could not read!");
                }
                else
                {
                    return nf.ReadAllBytes();
                }

            }
            else
            {
                throw new ArgumentException("Bad directory");
            }
        }
        /// <summary>
        /// Writes the specified string into the specified path and filename
        /// if the file doesn't exist - create it
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        /// <param name="data"></param>
        public static string ReadAllText(string file, string directory)
        {
            SDFS_Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                SDFS_File nf = dir.RetrieveFileByName(file);
                if (nf == null)
                {
                    throw new Exception("Could not create!");
                }
                else
                {
                    return nf.ReadAllText();
                }

            }
            else
            {
                throw new ArgumentException("Bad directory");
            }
        }
    }
}

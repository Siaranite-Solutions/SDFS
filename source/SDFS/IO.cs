using System;
using System.Collections.Generic;
using System.Text;
using SDFS.Logical.Entries;
using SDFS.Logical;

namespace SDFS
{
    class IO
    {
        private static Directory sf;

        /// <summary>
        /// Creates the specified directory
        /// </summary>
        /// <param name="directory"></param>
        private static void CreateDirectoryAndVerify(string directory)
        {
            Filesystem.cFS.Root.AddDirectory(directory);
            sf = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (sf == null)
            {
                // Cannot create required files, aborting creation of directory
                throw new Exception("Failed creation of directories.");
            }
        }
        /// <summary>
        /// Creates the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        private static void CreateFileAndVerify(string file, string directory)
        {
            Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                File nf = dir.RetrieveFileByName(file);
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

        /// <summary>
        /// Writes the specified byte array into the specified path and filename
        /// if the file doesn't exist - create it
        /// </summary>
        /// <param name="file"></param>
        /// <param name="directory"></param>
        /// <param name="data"></param>
        private static void CreateFileWithContentsAndVerify(string file, string directory, byte[] data)
        {
            Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                File nf = dir.RetrieveFileByName(file);
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
        private static void CreateFileWithContentsAndVerify(string file, string directory, string data)
        {
            Directory dir = Filesystem.cFS.Root.GetDirectoryByName(directory);
            if (dir != null)
            {
                dir.AddFile(file);
                File nf = dir.RetrieveFileByName(file);
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
    }
}

using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;

namespace APSIM.Shared.Utilities
{
    public class ZipUtilities
    {
        /// <summary>
        /// Zip a collection of files to the specified stream.
        /// </summary>
        /// <param name="filesToZip">Paths and names of files to zip</param>
        /// <param name="password">Optional password - can be null</param>
        /// <param name="fileName">The file to zip to</param>
        public static void ZipFiles(IEnumerable<string> filesToZip, string password, string fileName)
        {
            using (Stream s = File.Create(fileName))
                ZipFiles(filesToZip, password, s);
        }

        /// <summary>
        /// Zip a collection of files to the specified stream.
        /// </summary>
        /// <param name="filesToZip">Paths and names of files to zip</param>
        /// <param name="password">Optional password - can be null</param>
        /// <param name="s">The stream to zip to</param>
        public static void ZipFiles(IEnumerable<string> filesToZip, string password, Stream s)
        {
            using (ZipOutputStream zip = new ZipOutputStream(s))
            {
                zip.Password = password;
                zip.SetLevel(5); // 0 - store only to 9 - means best compression
                foreach (string FileName in filesToZip)
                {
                    FileStream fs = File.OpenRead(FileName);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();

                    ZipEntry entry = new ZipEntry(Path.GetFileName(FileName));
                    zip.PutNextEntry(entry);
                    zip.Write(buffer, 0, buffer.Length);
                }
                zip.IsStreamOwner = false;
                zip.Finish();
                zip.Close();
            }
        }

        /// <summary>
        /// Unzips the specified zip file into the specified destination folder. Will use the 
        /// specified password. Returns a list of filenames that were created.
        /// </summary>
        /// <param name="fileName">The file name to unzip</param>
        /// <param name="destinationFolder">The folder to unzip to</param>
        /// <param name="password">The optional password. Can be null</param>
        public static string[] UnZipFiles(string fileName, string destinationFolder, string password)
        {
            string[] files;
            using (StreamReader s = new StreamReader(fileName))
                files = UnZipFiles(s.BaseStream, destinationFolder, password);
            return files;
        }

        /// <summary>
        /// Unzips the specified zip file into the specified destination folder. Will use the 
        /// specified password. Returns a list of filenames that were created.
        /// </summary>
        /// <param name="s">The stream to unzip</param>
        /// <param name="destinationFolder">The folder to unzip to</param>
        /// <param name="password">The optional password. Can be null</param>
        public static string[] UnZipFiles(Stream s, string destinationFolder, string password)
        {
            List<string> filesCreated = new List<string>();
            using (ZipInputStream zip = new ZipInputStream(s))
            {
                zip.Password = password;
                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
                    // Convert either '/' or '\' to the local directory separator
                    string destFileName = destinationFolder + Path.DirectorySeparatorChar +
                           entry.Name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                    // Make sure the destination folder exists.
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileName));

                    using (BinaryWriter fileOut = new BinaryWriter(new FileStream(destFileName, FileMode.Create)))
                    {
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = zip.Read(data, 0, data.Length);
                            if (size > 0)
                                fileOut.Write(data, 0, size);
                            else
                                break;
                        }
                    }
                    filesCreated.Add(destFileName);
                }
            }
            return filesCreated.ToArray();
        }

        /// <summary>
        /// Unzips the specified zip and return the stream.
        /// </summary>
        /// <param name="fileName">The zip file to unzip</param>
        /// <param name="fileNameToExtract">The file to extract</param>
        /// <param name="password">The optional password. Can be null</param>
        public static Stream UnZipFile(string fileName, string fileNameToExtract, string password)
        {
            using (Stream s = File.Open(fileName, FileMode.Open, FileAccess.Read))
                return UnzipFile(s, fileNameToExtract, password);
        }

        /// <summary>
        /// Unzips the specified zip and return the stream.
        /// </summary>
        /// <param name="s">The zip stream to unzip</param>
        /// <param name="fileNameToExtract">The file to extract</param>
        /// <param name="password">The optional password. Can be null</param>
        public static Stream UnzipFile(Stream s, string fileNameToExtract, string password)
        {
            MemoryStream memStream = null;
            using (ZipInputStream zip = new ZipInputStream(s))
            {
                zip.Password = password;

                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
                    if (fileNameToExtract == entry.Name)
                    {
                        memStream = new MemoryStream();
                        using (BinaryWriter fileOut = new BinaryWriter(memStream))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = zip.Read(data, 0, data.Length);
                                if (size > 0)
                                    fileOut.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                        break;
                    }
                }
            }

            return memStream;
        }

        /// <summary>
        /// Return a list of filenames in zip file.
        /// </summary>
        /// <param name="fileName">The zip file name</param>
        /// <param name="password">The optional zip password. Can be null</param>
        public static string[] FileNamesInZip(string fileName, string password)
        {
            using (Stream s = File.Open(fileName, FileMode.Open, FileAccess.Read))
                return GetFileNamesInZip(s, password);
        }

        /// <summary>
        /// Return a list of filenames in zip stream.
        /// </summary>
        /// <param name="s">The zip stream</param>
        /// <param name="password">The optional zip password. Can be null</param>
        public static string[] GetFileNamesInZip(Stream s, string password)
        {
            using (ZipInputStream zip = new ZipInputStream(s))
            {
                zip.IsStreamOwner = false;

                if (password != "" && password != null)
                    zip.Password = password;
                List<string> fileNames = new List<string>();
                ZipEntry Entry;
                while ((Entry = zip.GetNextEntry()) != null)
                    fileNames.Add(Entry.Name);
                return fileNames.ToArray();
            }
        }
    }
}

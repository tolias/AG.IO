using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AG.Utilities;

namespace AG.IO
{
    public static class FileOperations
    {
        public static bool CompareFiles(string filePath1, string filePath2)
        {
            var file1 = new FileInfo(filePath1);
            var file2 = new FileInfo(filePath2);

            if (file1.Length != file2.Length)
            {
                return false;
            }

            if (file1.Length > 100000000)
            {
                return CompareLargeFiles(file1, file2);
            }
            else
            {
                return CompareSmallFiles(filePath1, filePath2);
            }
        }



        private static bool CompareSmallFiles(string filePath1, string filePath2)
        {
            byte[] originalFileContent = File.ReadAllBytes(filePath1);
            byte[] backupFileContent = File.ReadAllBytes(filePath2);

            return ByteUtilities.CompareEffectively(originalFileContent, backupFileContent);
        }

        private static bool CompareLargeFiles(FileInfo file1, FileInfo file2)
        {
            long fileLength = file1.Length;
            const int size = 0x1000000;
            bool success = true;

            Parallel.For(0, fileLength / size, x =>
            {
                var start = (int)x * size;

                if (start >= fileLength)
                {
                    return;
                }

                using (FileStream fileStream1 = File.OpenRead(file1.FullName))
                {
                    using (FileStream fileStream2 = File.OpenRead(file2.FullName))
                    {
                        var buffer = new byte[size];
                        var buffer2 = new byte[size];

                        fileStream1.Position = start;
                        fileStream2.Position = start;

                        int count = fileStream1.Read(buffer, 0, size);
                        fileStream2.Read(buffer2, 0, size);

                        if(!ByteUtilities.CompareEffectively(buffer, buffer2))
                        {
                            success = false;
                            return;
                        }
                        //for (int i = 0; i < count; i++)
                        //{
                        //    if (buffer[i] != buffer2[i])
                        //    {
                        //        success = false;
                        //        return;
                        //    }
                        //}
                    }
                }
            });

            return success;
        }
    }
}

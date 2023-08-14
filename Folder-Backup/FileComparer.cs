using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Folder_Backup
{
    public class FileComparer 
    {
        private FileInfo _sourceFileInfo;
        private FileInfo _targetFileInfo;

        private static int CHUNK_SIZE = 4096;

        public FileComparer(string sourceFilePath, string targetFilePath)
        {
            _sourceFileInfo = new FileInfo(sourceFilePath);
            _targetFileInfo = new FileInfo(targetFilePath);
        }

        public bool AreFilesEqual()
        {
            if (!AreFilesSameSize())
            {
                return false;
            }

            if (AreFilesEmpty())
            {
                return true;
            }

            return AreFileContentsTheSame();
        }

        private bool AreFilesEmpty()
        {
            return _sourceFileInfo.Length == 0;
        }

        private bool AreFilesSameSize()
        {
            return _sourceFileInfo.Length == _targetFileInfo.Length;
        }

        private bool AreFileContentsTheSame()
        {
            using (MemoryMappedFile sourceFile = MemoryMappedFile.CreateFromFile(_sourceFileInfo.FullName))
            using (MemoryMappedFile targetFile = MemoryMappedFile.CreateFromFile(_targetFileInfo.FullName))
            {
                using (MemoryMappedViewStream sourceFileStream = sourceFile.CreateViewStream())
                using (MemoryMappedViewStream targetFileStream = targetFile.CreateViewStream())
                {
                    int bufferSize = (int)Math.Min(CHUNK_SIZE, _sourceFileInfo.Length);

                    byte[] sourceByteBuffer = new byte[bufferSize];
                    byte[] targetByteBuffer = new byte[bufferSize];

                    for (long offset = 0; offset < _sourceFileInfo.Length; offset += bufferSize)
                    {
                        int sizeToRead = (int)Math.Min(_sourceFileInfo.Length - offset, bufferSize);

                        sourceFileStream.ReadExactly(sourceByteBuffer, 0, sizeToRead);
                        targetFileStream.ReadExactly(targetByteBuffer, 0, sizeToRead);

                        if (memcmp(sourceByteBuffer, targetByteBuffer, sizeToRead) != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);
    }
}

using System;
using System.IO;

namespace DIskTest.Model.Misc
{
    public class TestFile : IDisposable
    {
        private const int _buffer = 4 * 1024;
        private bool _disposed = false;

        public string Path { get; }
        public FileStream WriteStream
        {
            get;
        }
        public FileStream ReadStream
        {
            get;
        }
        public long TestAreaSizeBytes
        {
            get;
        }

        public TestFile(string drivePath, long testAreaSizeBytes)
        {
            Path = DiskInfo.GetTempFilePath(drivePath);
            TestAreaSizeBytes = testAreaSizeBytes;
            WriteStream = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, _buffer, FileOptions.WriteThrough);
            ReadStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _buffer, (FileOptions)0x20000000);
        }

        ~TestFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                ReadStream.Dispose();
                WriteStream.Dispose();
            }
            File.Delete(Path);
            _disposed = true;
        }
    }
}

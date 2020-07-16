using DIskTest.Model.Misc;
using System.Diagnostics;
using System.IO;

namespace DIskTest.Model.Operations.Read
{
    public class RandomRead : RandomOperation
    {
        private long _fileSize;

        public RandomRead(TestFile file, int blockSize) 
            : base(file.ReadStream, file, blockSize)
        {
            maxTestTime = 30;
            _fileSize = file.TestAreaSizeBytes;
        }

        public override string DisplayName { get => "Random read" + " [" + blockSize / 1024 + "KB] "; }

        protected override void ValidateAndInitParams()
        {
            base.ValidateAndInitParams();
            minBlock = 0;
            maxBlock = (_fileSize / blockSize) - 1;
        }

        protected override void DoOperation(byte[] data, Stopwatch sw, long currBlock, int i)
        {
            sw.Restart();
            fileStream.Seek(currBlock, SeekOrigin.Begin);
            fileStream.Read(data, 0, blockSize);
            sw.Stop();
        }

        protected override byte[] InitBuffer()
        {
            return new byte[blockSize];
        }
    }
}

using DIskTest.Model.Misc;
using System;
using System.Diagnostics;
using System.IO;

namespace DIskTest.Model.Operations.Write
{
    class RandomWrite : RandomOperation
    {
        private long _fileSize;
        private Random _rand;

        public RandomWrite(TestFile file, int blockSize) 
            : base(file.WriteStream, file, blockSize)
        {
            maxTestTime = 30;
            _fileSize = file.TestAreaSizeBytes;
        }

        public override string DisplayName { get => "Random write" + " [" + blockSize / 1024 + "KB] "; }

        protected override void ValidateAndInitParams()
        {
            base.ValidateAndInitParams();
            minBlock = 0;
            maxBlock = (_fileSize / blockSize) - 1;
        }

        protected override void DoOperation(byte[] data, Stopwatch sw, long currBlock, int i)
        {
            data[0] = (byte)_rand.Next();
            sw.Restart();
            fileStream.Seek(currBlock, SeekOrigin.Begin);
            fileStream.Write(data, i % blocksInMemory, blockSize);
            sw.Stop();
        }

        protected override byte[] InitBuffer()
        {
            Status = OperationStatus.InitMemBuffer;
            _rand = new Random();
            var block = new byte[blockSize];
            var data = new byte[blockSize * blocksInMemory];
            for (int i = 0; i < blocksInMemory; i++)
            {
                _rand.NextBytes(block);
                Array.Copy(block, 0, data, blockSize * i, blockSize);
            }
            return data;
        }
    }
}

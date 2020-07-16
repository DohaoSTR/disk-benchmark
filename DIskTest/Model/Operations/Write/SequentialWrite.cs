using DIskTest.Model.Misc;
using System;
using System.Diagnostics;

namespace DIskTest.Model.Operations.Write
{
    class SequentialWrite : SequentialOperation
    {
        public SequentialWrite(TestFile file, int blockSize) 
            : base(file.WriteStream, file, blockSize, file.TestAreaSizeBytes / blockSize)
        { }

        public override string DisplayName { get => "Sequential write" + " [" + blockSize / 1024 / 1024 + "MB] "; }

        protected override void DoOperation(byte[] buffer, Stopwatch sw)
        {
            sw.Restart();
            fileStream.Write(buffer, 0, blockSize);
            sw.Stop();
        }

        protected override byte[] InitBuffer()
        {
            if (totalBlocks == 0) 
                throw new ArgumentOutOfRangeException("totalBlocks", "Номер блока не может быть равен 0!");
            Status = OperationStatus.InitMemBuffer;
            var buffer = new byte[blockSize];
            var rand = new Random();
            rand.NextBytes(buffer);
            return buffer;
        }
    }
}

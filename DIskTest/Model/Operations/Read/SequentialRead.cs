using DIskTest.Model.Misc;
using System;
using System.Diagnostics;

namespace DIskTest.Model.Operations.Read
{
    public class SequentialRead : SequentialOperation
    {
        public SequentialRead(TestFile file, int blockSize) 
            : base(file.ReadStream, file, blockSize, file.TestAreaSizeBytes / blockSize)
        { }

        public override string DisplayName { get => "Sequential read" + " [" + blockSize / 1024 / 1024 + "MB] "; }

        protected override void DoOperation(byte[] buffer, Stopwatch sw)
        {
            sw.Restart();
            fileStream.Read(buffer, 0, blockSize);
            sw.Stop();
        }

        protected override byte[] InitBuffer()
        {
            if (fileStream.Length < blockSize) 
                throw new ArgumentException("Размер файла не может быть меньше размера блока!");
            if (totalBlocks == 0) 
                totalBlocks = (int)(fileStream.Length / blockSize);
            var buffer = new byte[blockSize];
            return buffer;
        }
    }
}

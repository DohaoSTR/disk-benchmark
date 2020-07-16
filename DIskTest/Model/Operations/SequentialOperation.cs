using DIskTest.Model.Misc;
using System;
using System.Diagnostics;
using System.IO;

namespace DIskTest.Model.Operations
{
    public abstract class SequentialOperation : Operation
    {
        protected readonly FileStream fileStream;
        protected TestFile file;
        protected long totalBlocks;

        public long TotalBlocks
        {
            get { return totalBlocks; }
        }

        public SequentialOperation(FileStream fileStream, TestFile file, int blockSize, long totalBlocks)
        {
            if (blockSize <= 0) 
                throw new ArgumentOutOfRangeException("blockSize", "Размер блока не может быть отрицательным!");
            if (totalBlocks < 0) 
                throw new ArgumentOutOfRangeException("totalBlocks", "Номер блока может быть отрицательным!");

            this.fileStream = fileStream;
            this.file = file;
            this.blockSize = blockSize;
            this.totalBlocks = totalBlocks;
        }

        public override ResultsReport Execute()
        {
            Status = OperationStatus.Started;
            byte[] data;
            int prevPercent = -1;
            int curPercent;
            var results = new ResultsReport(this);
            var sw = new Stopwatch();
            try
            {
                data = InitBuffer();
            }
            catch
            {
                NotEnoughMemUpdate(results, 0);
                return results;
            }
            fileStream.Seek(0, SeekOrigin.Begin);
            RestartElapsed();
            Status = OperationStatus.Running;
            for (var i = 1; i < totalBlocks + 1; i++)
            {
                DoOperation(data, sw);
                if (i == 0) 
                {
                    Status = OperationStatus.Running;
                    fileStream.Seek(0, SeekOrigin.Begin);
                }
                if (i > 0) 
                {
                    results.AddTroughputMbs(blockSize, fileStream.Position - blockSize, sw);
                    curPercent = (int)(i * 100 / totalBlocks);
                    if (curPercent > prevPercent)
                    {
                        Update(curPercent, results.GetLatest5AvgResult(), results: results);
                        prevPercent = curPercent;
                    }
                }
            }
            results.TotalTimeMs = StopElapsed();
            FinalUpdate(results, ElapsedMs);
            TestCompleted();
            return results;
        }

        protected abstract void DoOperation(byte[] buffer, Stopwatch sw);

        protected abstract byte[] InitBuffer();

        protected virtual void TestCompleted()
        {
            GC.Collect(2, GCCollectionMode.Forced, true);
        }
    }
}

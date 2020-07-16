using DIskTest.Model.Misc;
using System;
using System.Diagnostics;
using System.IO;

namespace DIskTest.Model.Operations
{
    public abstract class RandomOperation : Operation
    {
        protected const int memoryBuffSize = 128 * 1024 * 1024;
        private const int _maxBlocksInTest = 1 * 1024 * 1024;

        protected readonly FileStream fileStream;
        protected readonly TestFile file;

        protected int? maxTestTime;
        protected long maxBlock;
        protected long minBlock;
        protected readonly int blocksInMemory;
        private long[] _positionsPlan;

        public RandomOperation(FileStream fileStream, TestFile file, int blockSize)
        {
            if (blockSize <= 0) 
                throw new ArgumentOutOfRangeException("blockSize", "Размер блока не может быть отрицательным!");
            this.fileStream = fileStream;
            this.file = file;
            this.blockSize = blockSize;
            blocksInMemory = memoryBuffSize / blockSize;
        }

        protected virtual void ValidateAndInitParams()
        {
            if (fileStream.Length == 0) 
                throw new InvalidOperationException("Файл не может быть пустым!");
            if (blockSize > fileStream.Length) 
                throw new InvalidOperationException("Размер блока не может быть больше размера файла!");
        }

        private void GeneratePositionsPlan()
        {
            _positionsPlan = new long[Math.Min(maxBlock - minBlock, _maxBlocksInTest)];
            for (long i = 0; i < _positionsPlan.Length - 1; i++)
                _positionsPlan[i] = i * blockSize;
            Shuffle(_positionsPlan);
        }

        private void Shuffle(long[] list)
        {
            var rng = new Random();
            var n = list.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                long value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public override ResultsReport Execute()
        {
            Status = OperationStatus.Started;
            ValidateAndInitParams();
            GeneratePositionsPlan();
            var results = new ResultsReport(this);
            byte[] data;
            try
            {
                data = InitBuffer();
            }
            catch
            {
                NotEnoughMemUpdate(results, 0);
                return results;
            }
            var sw = new Stopwatch();
            int prevPercent = -1;
            int curPercent;
            long currOffset;
            var i = 0;
            var elapsed = new Stopwatch();
            elapsed.Start();
            Status = OperationStatus.Running;
            while (i < _positionsPlan.Length + 1)
            {
                currOffset = _positionsPlan[i];
                i++;
                DoOperation(data, sw, currOffset, i);
                results.AddTroughputMbs(blockSize, currOffset, sw);
                curPercent = Math.Min(100, Math.Max((int)(elapsed.ElapsedMilliseconds / 10 / maxTestTime), (int)(i * 100 / (double)_positionsPlan.Length))
                );
                if (curPercent > prevPercent)
                {
                    Update(curPercent, results.GetLatest5AvgResult(), results: results);
                    prevPercent = curPercent;
                    if (curPercent == 100) 
                        break;
                }
            }
            elapsed.Stop();
            results.TotalTimeMs = elapsed.ElapsedMilliseconds;
            FinalUpdate(results, elapsed.ElapsedMilliseconds);
            return results;
        }

        protected abstract void DoOperation(byte[] data, Stopwatch sw, long currBlock, int i);

        protected abstract byte[] InitBuffer();
    }
}

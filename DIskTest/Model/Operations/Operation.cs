using DIskTest.Model.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DIskTest.Model.Operations
{
    public abstract class Operation
    {
        public abstract ResultsReport Execute();
        public abstract string DisplayName { get; }
        public bool IsNormalizedAvg { get; set; }
        public event EventHandler<OperationUpdateEventArgs> StatusUpdate;
        protected int blockSize;

        public int BlockSizeBytes { get { return blockSize; } }

        private OperationStatus status = OperationStatus.NotStarted;

        protected void Update(double? progressPercent = null, double? recentResult = null, long? elapsedMs = null, ResultsReport results = null)
        {
            StatusUpdate?.Invoke(this, new OperationUpdateEventArgs(Status, progressPercent, recentResult, elapsedMs, results));
        }

        protected void FinalUpdate(ResultsReport results, long elapsedMs)
        {
            status = OperationStatus.Completed;
            Update(100, null, elapsedMs, results);
        }

        protected void NotEnoughMemUpdate(ResultsReport results, long elapsedMs)
        {
            status = OperationStatus.NotEnoughMemory;
            Update(100, null, elapsedMs, results);
        }

        public OperationStatus Status
        {
            get
            {
                return status;
            }
            protected internal set
            {
                status = value;
                StatusUpdate?.Invoke(this, new OperationUpdateEventArgs(status, null, null, ElapsedMs, null));
            }
        }

        protected Stopwatch elapsedSw;

        public long ElapsedMs
        {
            get
            {
                return elapsedSw == null ? 0 : elapsedSw.ElapsedMilliseconds;
            }
        }

        protected void RestartElapsed()
        {
            elapsedSw = new Stopwatch();
            elapsedSw.Start();
        }

        protected long StopElapsed()
        {
            elapsedSw.Stop();
            return elapsedSw.ElapsedMilliseconds;
        }
    }
}

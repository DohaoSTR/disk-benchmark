using DIskTest.Model.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace DIskTest.Model
{
    public abstract class TestBase
    {
        protected List<Operation> _operations;
        private Stopwatch _sw = new Stopwatch();

        public event EventHandler<OperationUpdateEventArgs> StatusUpdate;

        protected volatile bool breakCalled = false;
        protected List<ResultsReport> results;

        public TestBase()
        {
            _operations = new List<Operation>();
        }

        public void AddTest(Operation test)
        {
            _operations.Add(test);
            test.StatusUpdate += (sender, e) => StatusUpdate?.Invoke(sender, e);
        }

        public IEnumerable<Operation> ListTests()
        {
            return _operations;
        }

        private void ResetTests()
        {
            foreach (var t in _operations)
                t.Status = OperationStatus.NotStarted;
            breakCalled = false;
        }

        public virtual ResultsReport[] Execute()
        {
            ResetTests();
            results = new List<ResultsReport>();
            _sw.Restart();
            foreach (Operation t in _operations)
            {
                var r = t.Execute();
                if (r != null)
                    results.Add(r);
                if (breakCalled)
                    break;
                CleanUp();
            }
            _sw.Stop();
            return results.ToArray();
        }

        private static void CleanUp()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
        }

        public virtual long ElapsedMs
        {
            get
            {
                return _sw.ElapsedMilliseconds;
            }
        }

        public virtual long RemainingMs
        {
            get
            {
                return -1;
            }
        }
    }
}

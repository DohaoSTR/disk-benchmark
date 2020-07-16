using DIskTest.Model.Misc;
using DIskTest.Model.Operations;
using DIskTest.Model.Operations.Read;
using DIskTest.Model.Operations.Write;
using System;

namespace DIskTest.Model
{
    public class TestOperations : TestBase, IDisposable
    {
        public readonly long FileSize;
        public const int sequentialBlock = 4 * 1024 * 1024;
        public const int randomBlock = 4 * 1024 * 1024;

        private TestFile _file;

        public TestOperations(string drivePath, long fileSize)
        {
            FileSize = fileSize;
            _file = new TestFile(drivePath, fileSize);

            AddTest(new SequentialWrite(_file, sequentialBlock));
            AddTest(new SequentialRead(_file, sequentialBlock));
            AddTest(new RandomWrite(_file, randomBlock));
            AddTest(new RandomRead(_file, randomBlock));
        }

        public override ResultsReport[] Execute()
        {
            var results = base.Execute();
            return results;
        }

        public string FilePath
        {
            get
            {
                return _file.Path;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            _operations = null;
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _file?.Dispose();
            }
        }

        ~TestOperations()
        {
            Dispose(false);
        }
    }
}

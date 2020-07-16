using DIskTest.Model.Operations;

namespace DIskTest.Model
{
    public class OperationUpdateEventArgs
    {
        public ResultsReport Results { get; }

        public double? ProgressPercent { get; }

        public double? RecentResult { get; }

        public OperationStatus Status;

        public long? ElapsedMs { get; }

        public OperationUpdateEventArgs(OperationStatus status, double? progressPercent, double? recentResult, long? elapsedMs, ResultsReport results)
        {
            Results = results;
            ProgressPercent = progressPercent;
            RecentResult = recentResult;
            ElapsedMs = elapsedMs;
            Status = status;
        }
    }
}

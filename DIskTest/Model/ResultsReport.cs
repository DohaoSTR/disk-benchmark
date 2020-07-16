using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DIskTest.Model.Operations
{
    public class ResultsReport
    {
        private int _recalcCount = -1;
        private double avgThroughputReal, avgThroughputNormalized;
        private const double normalizationTimeThreshold = 0.95;

        private const int _intialCapacity = 300000;
        private List<double> _results;
        private List<long> _positions;

        public string TestDisplayName { get; }
        public long BlockSizeBytes { get; }

        public ResultsReport(Operation test)
        {
            _results = new List<double>(_intialCapacity);
            _positions = new List<long>();
            TestDisplayName = test.DisplayName;
            UseNormalizedAvg = test.IsNormalizedAvg;
        }

        public bool UseNormalizedAvg { get; set; }

        public long TotalTimeMs { get; protected internal set; }

        public double Min { get; private set; } = double.MaxValue;

        public double Max { get; private set; } = -1;

        public double Mean { get; private set; } = -1;

        public double Median { get; private set; } = -1;

        public double AvgThroughput
        {
            get
            {
                Recalculate();
                return UseNormalizedAvg ? avgThroughputNormalized : avgThroughputReal;
            }
        }

        public double AvgThroughputNormalized
        {
            get
            {
                Recalculate();
                return avgThroughputNormalized;
            }
        }

        public double AvgThroughputReal
        {
            get
            {
                Recalculate();
                return avgThroughputReal;
            }
        }

        private void Recalculate()
        {
            if (_results.Count == 0) 
                return;
            if (_recalcCount != _results.Count)
            {
                var sorted = new double[_results.Count];
                _results.CopyTo(sorted);
                Array.Sort(sorted);
                Median = sorted[sorted.Length / 2];
                double inverseThroughputs = 0;
                double totalTime = 0;
                foreach (var r in sorted)
                {
                    inverseThroughputs += 1 / r;
                    totalTime += BlockSizeBytes / (r * 1024 * 1024);
                }
                avgThroughputReal = sorted.Length / inverseThroughputs; 
                double inverseNormThroughputs = 0;
                int inverseNormCount = 1;
                double normTime = 0;
                double maxN = -1;
                foreach (var r in sorted)
                {
                    if (r > maxN) maxN = r;
                    inverseNormThroughputs += 1 / r;
                    normTime += BlockSizeBytes / (r * 1024 * 1024);
                    if (normTime > normalizationTimeThreshold * totalTime) 
                        break;
                    inverseNormCount++;
                }
                avgThroughputNormalized = inverseNormCount / inverseNormThroughputs;
                _recalcCount = _results.Count;
            }
        }

        public void AddResultInternal(double result, long? position)
        {
            if (!double.IsInfinity(result))
            {
                _results.Add(result);
                if (result > Max) Max = result;
                if (result < Min) Min = result;
                if (Mean == -1) Mean = result; else Mean = Mean * (_results.Count - 1) / _results.Count + result / _results.Count;
                if (position != null) _positions.Add(position.Value);
            }
        }

        public void AddResult(double result)
        {
            if (_positions.Count > 0)
                throw new InvalidOperationException("Вы не можете вызвать этот метод после перегрузки!");
            AddResultInternal(result, null);
        }

        public void AddResult(double result, long position)
        {
            if (_positions.Count != _results.Count)
                throw new InvalidOperationException("Вы не можете вызвать этот метод после перегрузки!");
            AddResultInternal(result, position);
        }

        public void AddTroughputMbs(long bytes, long position, Stopwatch stopwatch)
        {
            double secs = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            AddResult(((double)bytes / 1024 / 1024) / secs, position);
        }

        public double GetLatest5AvgResult()
        {
            if (_results.Count == 0) return 0;
            double inverseThroughputs = 0;
            for (var i = _results.Count - 1; i >= (_results.Count - 5 > 0 ? _results.Count - 5 : 0); i--)
                inverseThroughputs += 1 / _results[i];
            return _results.Count - 5 > 0 ? 5 / inverseThroughputs : _results.Count / inverseThroughputs;
        }
    }
}

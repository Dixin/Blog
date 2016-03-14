namespace Dixin.Tests.Linq.Parallel
{
    using System.Diagnostics;

    using Dixin.Linq.Parallel;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void VisualizeTest()
        {
            Trace.WriteLine(nameof(Performance.Benchmark));
            Performance.Benchmark();

            Trace.WriteLine(nameof(Performance.Visualize));
            Performance.Visualize();
        }

        [TestMethod]
        public void ComputingTest()
        {
            Trace.WriteLine(nameof(Performance.QuerySmallArray));
            Performance.QuerySmallArray();

            Trace.WriteLine(nameof(Performance.QueryMediumArray));
            Performance.QueryMediumArray();

            Trace.WriteLine(nameof(Performance.QueryLargeArray));
            Performance.QueryLargeArray();
            // QuerySmallArray
            // SequentialComputing: 8
            // ParallelComputing: 1238
            // QueryMediumArray
            // SequentialComputing: 160
            // ParallelComputing: 144
            // QueryLargeArray
            // SequentialComputing: 167
            // ParallelComputing: 87
        }

        [TestMethod]
        public void IOTest()
        {
            Trace.WriteLine(nameof(Performance.DownloadSmallFiles));
            Performance.DownloadSmallFiles();

            Trace.WriteLine(nameof(Performance.DownloadLargeFiles));
            Performance.DownloadLargeFiles();

            Trace.WriteLine(nameof(Performance.ReadFiles));
            Performance.ReadFiles();
        }
    }
}

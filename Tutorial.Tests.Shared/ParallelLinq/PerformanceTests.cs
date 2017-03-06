namespace Tutorial.Tests.ParallelLinq
{
    using Tutorial.ParallelLinq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void ComputingTest()
        {
            Performance.OrderBy();
        }

        [TestMethod]
        public void IOTest()
        {
            Performance.DownloadSmallFiles();
            Performance.DownloadLargeFiles();
            Performance.ReadFiles();
        }
    }
}

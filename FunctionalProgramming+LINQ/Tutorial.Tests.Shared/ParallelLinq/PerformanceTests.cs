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
            Performance.OrderByTestForCount();
            Performance.OrderByTestForKeySelector();
        }

        [TestMethod]
        public void IOTest()
        {
            Performance.RunDownloadSmallFilesTest();
            Performance.RunDownloadLargeFilesTest();
            Performance.ReadFiles();
        }
    }
}

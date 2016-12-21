namespace Dixin.Tests.Linq.Parallel
{
    using Dixin.Linq.Parallel;

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
#if NETFX
            Performance.DownloadSmallFiles();
            Performance.DownloadLargeFiles();
#endif
            Performance.ReadFiles();
        }
    }
}

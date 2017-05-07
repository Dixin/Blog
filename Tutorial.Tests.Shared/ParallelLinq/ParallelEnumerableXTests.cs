namespace Tutorial.Tests.ParallelLinq
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.ParallelLinq;

    [TestClass]
    public class ParallelEnumerableXTests
    {
        [TestMethod]
        public void ForceParallelTest()
        {
            ConcurrentBag<int> threadIds = new ConcurrentBag<int>();
            int forcedDegreeOfParallelism = 5;
            Enumerable.Range(0, forcedDegreeOfParallelism * 10).ForceParallel(
                value => threadIds.Add(Thread.CurrentThread.ManagedThreadId + Functions.ComputingWorkload()),
                forcedDegreeOfParallelism);
            Assert.AreEqual(forcedDegreeOfParallelism, threadIds.Distinct().Count());

        }
    }
}

namespace Dixin.Tests.Linq.Parallel
{
    using Dixin.Linq.Parallel;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class QueryMethodsTests
    {
        [TestMethod]
        public void QueryTest()
        {
            QueryMethods.OptInOutParallel();
            QueryMethods.QueryExpression();
            QueryMethods.ForEachForAll();
            QueryMethods.ForEachForAllTimeSpans();
            QueryMethods.VisualizeForEachForAll();
        }

        [TestMethod]
        public void CancelTest()
        {
            QueryMethods.Cancel();
        }

        [TestMethod]
        public void DegreeOfParallelismTest()
        {
            QueryMethods.DegreeOfParallelism();
        }

        [TestMethod]
        public void MergeTest()
        {
            QueryMethods.Merge();
        }

        [TestMethod]
        public void ExecutionModeTest()
        {
            QueryMethods.ExecutionMode();
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.Aggregate();
        }
    }
}

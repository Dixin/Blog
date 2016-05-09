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
            QueryMethods.WhereSelect();
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
        public void ExecutionModeTest()
        {
            QueryMethods.ExecutionMode();
        }

        [TestMethod]
        public void MergeTest()
        {
            QueryMethods.Except();
            QueryMethods.MergeForSelect();
            QueryMethods.MergeForTakeWhile();
            QueryMethods.MergeForOrderBy();
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.CommutativeAssociative();
            QueryMethods.AggregateCorrectness();
            QueryMethods.VisualizeAggregate();
            QueryMethods.MergeForAggregate();
        }
    }
}

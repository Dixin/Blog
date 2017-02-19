namespace Tutorial.Tests.ParallelLinq
{
    using Tutorial.ParallelLinq;

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
#if NETFX
            QueryMethods.ForEachForAllTimeSpans();
#endif
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
#if NETFX
            QueryMethods.ExecutionMode();
#endif
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
#if NETFX
            QueryMethods.VisualizeAggregate();
#endif
            QueryMethods.MergeForAggregate();
        }
    }
}

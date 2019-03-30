namespace Tutorial.Tests.LinqToSql
{
    using Tutorial.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LazinessTests
    {

        [TestMethod]
        public void DeferredExecutionTest()
        {
            Laziness.DeferredExecution();
        }

        [TestMethod]
        public void EagerLoadingTest()
        {
            Laziness.EagerLoadingWithSelect();
            Laziness.EagerLoadingWithRelationship();
            Laziness.ConditionalEagerLoading();
        }

        [TestMethod]
        public void DisableDeferredLoadingTest()
        {
            Laziness.DisableDeferredLoading();
        }
    }
}

namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.LinqToSql;

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
            Laziness.EagerLoadingWithAssociation();
            Laziness.ConditionalEagerLoading();
        }

        [TestMethod]
        public void DisableDeferredLoadingTest()
        {
            Laziness.DisableDeferredLoading();
        }
    }
}

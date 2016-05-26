namespace Dixin.Tests.Linq.EntityFramework
{
    using System.Linq;

    using Dixin.Linq.EntityFramework;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void CompiedQuery()
        {
            using (LegacyAdventureWorks adventureWorks = new LegacyAdventureWorks())
            {
                string[] productNames = adventureWorks.GetProductNames(539.99M).ToArray();
                EnumerableAssert.Any(productNames);
            }
        }

        [TestMethod]
        public void ViewsTest()
        {
            Performance.PrintViews();
            Performance.QueryPlanCache();
        }

        [TestMethod]
        public void PerformanceTest()
        {
            Performance.ObjectCache();
            Performance.QueryPlanCache();
            Performance.Async().Wait();
        }
    }
}

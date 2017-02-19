namespace Tutorial.Tests.LinqToSql
{
    using System.Linq;

    using Tutorial;
    using Tutorial.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void CompiedQueryTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                string[] productNames = adventureWorks.GetProductNames(539.99M).ToArray();
                EnumerableAssert.Any(productNames);
            }
        }

        [TestMethod]
        public void PerformanceTest()
        {
            Performance.QueryPlanCache();
        }
    }
}

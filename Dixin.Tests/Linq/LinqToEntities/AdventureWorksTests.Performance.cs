namespace Dixin.Tests.Linq.LinqToEntities
{
    using System.Linq;

    using Dixin.Linq.LinqToEntities;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class AdventureWorksTests
    {
        [TestMethod]
        public void CompiedQuery()
        {
            using (LegacyAdventureWorks adventureWorks = new LegacyAdventureWorks())
            {
                string[] productNames = adventureWorks.GetProductNames(100).ToArray();
                EnumerableAssert.Any(productNames);
            }
        }
    }
}

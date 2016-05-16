namespace Dixin.Tests.Linq.EntityFramework
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    using Dixin.Linq.EntityFramework;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class AdventureWorksTests
    {
        [TestMethod]
        public void ContainerTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Assert.AreEqual(nameof(AdventureWorks), adventureWorks.Container().Name);
            }
        }

        [TestMethod]
        public void TableTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory[] categories = adventureWorks
                    .ProductCategories
                    .Include(category => category.ProductSubcategories)
                    .ToArray();
                EnumerableAssert.Any(categories);
                categories.ForEach(category => EnumerableAssert.Any(category.ProductSubcategories));
            }
        }

        [TestMethod]
        public void StoredProcedureWithExecuteStoreQueryTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = adventureWorks.GetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }
    }
}

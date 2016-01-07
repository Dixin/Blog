namespace Dixin.Tests.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    using Dixin.Linq.LinqToEntities;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class AdventureWorksTests
    {
        [TestMethod]
        public void ContainerTest()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                Assert.AreEqual(nameof(AdventureWorksDbContext), adventureWorks.Container().Name);
            }
        }

        [TestMethod]
        public void TableTest()
        {
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
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
            using (AdventureWorksDbContext adventureWorks = new AdventureWorksDbContext())
            {
                ObjectResult<ManagerEmployee> employees = adventureWorks.GetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }
    }
}

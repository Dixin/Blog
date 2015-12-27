namespace Dixin.Tests.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    using Dixin.Linq.LinqToEntities;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AdventureWorksTests
    {
        [TestMethod]
        public void ContainerName()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                Assert.AreEqual(nameof(AdventureWorks), database.Container().Name);
            }
        }

        [TestMethod]
        public void QueryTable()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ProductCategory[] categories = database
                    .ProductCategories
                    .Include(category => category.ProductSubcategories)
                    .ToArray();
                EnumerableAssert.Any(categories);
                categories.ForEach(category => EnumerableAssert.Any(category.ProductSubcategories));
                var x = database.ProductSubcategories.GroupBy(subcategpry => subcategpry.ProductCategoryID)
                    .Select(group => new { group.Key, Count = group.Count() }).ToArray();
            }
        }

        [TestMethod]
        public void CallStoredProcedureWithExecuteStoreQuery()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = database.GetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }
    }
}

namespace Dixin.Tests.Linq.LinqToEntities
{
    using System;
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
                Assert.AreEqual(nameof(AdventureWorks), database.GetContainer().Name);
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
                    .Select(group => new {group.Key, Count = group.Count()}).ToArray();
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

        [TestMethod]
        public void CallStoredProcedureWithSingleResult()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = database.uspGetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }

        [TestMethod]
        public void CallStoreProcedureWithOutParameter()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectParameter errorLogId = new ObjectParameter("ErrorLogID", typeof(int)) { Value = 5 };
                int rows = database.uspLogError(errorLogId);
                Assert.AreEqual(0, errorLogId.Value);
                Assert.AreEqual(typeof(int), errorLogId.ParameterType);
                Assert.AreEqual(-1, rows);
            }
        }

        [TestMethod]
        public void CallStoreProcedureWithMultipleResults()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectResult<ProductCategory> categories = database.uspGetCategoryAndSubCategory(1);
                EnumerableAssert.Single(categories);
                EnumerableAssert.Any(categories.GetNextResult<ProductSubcategory>());
            }
        }

        [TestMethod]
        public void CallTableValuedFunction()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                IQueryable<ContactInformation> employees = database.ufnGetContactInformation(1).Take(2);
                EnumerableAssert.Single(employees);
            }
        }

        [TestMethod]
        public void CallComposableScalarValuedFunction()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                IQueryable<Product> products = database
                    .Products
                    .Where(product => product.ListPrice <= database.ufnGetProductListPrice(999, DateTime.Now));
                EnumerableAssert.Any(products);

                try
                {
                    decimal? cost = database.ufnGetProductListPrice(999, DateTime.Now);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        [TestMethod]
        public void CallNonComposableScalarValuedFunction()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                decimal? cost = database.ufnGetProductStandardCost(999, DateTime.Now);
                Assert.IsNotNull(cost);
                Assert.IsTrue(cost > 1);

                try
                {
                    database
                        .Products
                        .Where(product => product.ListPrice >= database.ufnGetProductStandardCost(999, DateTime.Now)).ToArray();
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                }
            }
        }
    }
}

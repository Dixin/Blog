namespace Tutorial.Tests.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToSql;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class FunctionsTests
    {
        [TestMethod]
        public void StoredProcedureWithSingleResultTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ISingleResult<ManagerEmployee> employees = adventureWorks.uspGetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }

        [TestMethod]
        public void StoreProcedureWithOutParameterTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int? errorLogId = 5;
                int returnValue = adventureWorks.uspLogError(ref errorLogId);
                Assert.AreEqual(0, errorLogId.Value);
                Assert.AreEqual(0, returnValue);
            }
        }

        [TestMethod]
        public void StoreProcedureWithMultipleResultsTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (IMultipleResults results = adventureWorks.uspGetCategoryAndSubcategory(1))
            {
                EnumerableAssert.Single(results.GetResult<ProductCategory>());
                EnumerableAssert.Any(results.GetResult<ProductSubcategory>());
            }
        }

        [TestMethod]
        public void TableValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ContactInformation> employees = adventureWorks.ufnGetContactInformation(1).Take(2);
                EnumerableAssert.Single(employees);
            }
        }

        [TestMethod]
        public void ScalarValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> products = adventureWorks
                    .Products
                    .Where(product => product.ListPrice <= adventureWorks.ufnGetProductListPrice(999, DateTime.Now));
                EnumerableAssert.Any(products);

                decimal? price = adventureWorks.ufnGetProductListPrice(999, DateTime.Now);
                Assert.IsTrue(price > 1);

                decimal? cost = adventureWorks.ufnGetProductStandardCost(999, DateTime.Now);
                Assert.IsNotNull(cost);
                Assert.IsTrue(cost > 1);

                products = adventureWorks
                        .Products
                        .Where(product => product.ListPrice >= adventureWorks.ufnGetProductStandardCost(999, DateTime.Now));
                EnumerableAssert.Any(products);
            }
        }
    }
}

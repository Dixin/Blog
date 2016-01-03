namespace Dixin.Tests.Linq.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Linq;

    using Dixin.Linq.LinqToSql;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AdventureWorksTests
    {
        [TestMethod]
        public void QueryTable()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<ProductCategory>(category => category.ProductSubcategories);
                database.LoadOptions = options;
                ProductCategory[] categories = database.ProductCategories.ToArray();
                EnumerableAssert.Any(categories);
                categories.ForEach(category => EnumerableAssert.Any(category.ProductSubcategories));
            }
        }

        [TestMethod]
        public void CallStoredProcedureWithSingleResult()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            {
                ISingleResult<ManagerEmployee> employees = database.uspGetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }

        [TestMethod]
        public void CallStoreProcedureWithOutParameter()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            {
                int? errorLogId = 5;
                int returnValue = database.uspLogError(ref errorLogId);
                Assert.AreEqual(0, errorLogId.Value);
                Assert.AreEqual(0, returnValue);
            }
        }

        [TestMethod]
        public void CallStoreProcedureWithMultipleResults()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            using (IMultipleResults results = database.uspGetCategoryAndSubCategory(1))
            {
                EnumerableAssert.Single(results.GetResult<ProductCategory>());
                EnumerableAssert.Any(results.GetResult<ProductSubcategory>());
            }
        }

        [TestMethod]
        public void CallTableValuedFunction()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            {
                IQueryable<ContactInformation> employees = database.ufnGetContactInformation(1).Take(2);
                EnumerableAssert.Single(employees);
            }
        }

        [TestMethod]
        public void CallScalarValuedFunction()
        {
            using (AdventureWorksDataContext database = new AdventureWorksDataContext())
            {
                IQueryable<Product> products = database
                    .Products
                    .Where(product => product.ListPrice <= database.ufnGetProductListPrice(999, DateTime.Now));
                EnumerableAssert.Any(products);

                decimal? price = database.ufnGetProductListPrice(999, DateTime.Now);
                Assert.IsTrue(price > 1);

                decimal? cost = database.ufnGetProductStandardCost(999, DateTime.Now);
                Assert.IsNotNull(cost);
                Assert.IsTrue(cost > 1);

                products = database
                        .Products
                        .Where(product => product.ListPrice >= database.ufnGetProductStandardCost(999, DateTime.Now));
                EnumerableAssert.Any(products);
            }
        }
    }
}

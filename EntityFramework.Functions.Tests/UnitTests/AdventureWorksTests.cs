namespace EntityFramework.Functions.Tests.UnitTests
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Globalization;
    using System.Linq;

    using EntityFramework.Functions.Tests.Examples;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AdventureWorksTests
    {
        [TestMethod]
        public void CallStoredProcedureWithSingleResult()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = database.uspGetManagerEmployees(2);
                Assert.IsTrue(employees.Any());
            }
        }

        [TestMethod]
        public void CallStoreProcedureWithOutParameter()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                ObjectParameter errorLogId = new ObjectParameter("ErrorLogID", typeof(int)) { Value = 5 };
                int? rows = database.LogError(errorLogId);
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
                // The first type of result type: a sequence of ProductCategory objects.
                ObjectResult<ProductCategory> categories = database.uspGetCategoryAndSubCategory(1);
                Assert.IsNotNull(categories.Single());
                // The second type of result type: a sequence of ProductCategory objects.
                ObjectResult<ProductSubcategory> subcategories = categories.GetNextResult<ProductSubcategory>();
                Assert.IsTrue(subcategories.Any());
            }
        }

        [TestMethod]
        public void CallTableValuedFunction()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                IQueryable<ContactInformation> employees = database.ufnGetContactInformation(1).Take(2);
                Assert.IsNotNull(employees.Single());
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
            }
        }

        [TestMethod]
        public void NonComposableScalarValuedFunctionInLinq()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                try
                {
                    database
                        .Products
                        .Where(product => product.ListPrice >= database.ufnGetProductStandardCost(999, DateTime.Now))
                        .ToArray();
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        [TestMethod]
        public void ComposableScalarValuedFunctionInLinq()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                IQueryable<Product> products = database
                    .Products
                    .Where(product => product.ListPrice <= database.ufnGetProductListPrice(999, DateTime.Now));
                Assert.IsTrue(products.Any());
            }
        }

        [TestMethod]
        public void CallComposableScalarValuedFunction()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                try
                {
                    database.ufnGetProductListPrice(999, DateTime.Now);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        [TestMethod]
        public void AggregateFunctionInLinq()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                var categories = database.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name).Concat()
                    })
                    .ToArray();
                Assert.IsTrue(categories.Length > 0);
                categories.ForEach(category =>
                    {
                        Assert.IsTrue(category.CategoryId > 0);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(category.SubcategoryNames));
                    });
            }
        }

        [TestMethod]
        public void BuitInFunctionInLinq()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                var categories = database.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name.Left(4)).Concat()
                    })
                    .ToArray();
                Assert.IsTrue(categories.Length > 0);
                categories.ForEach(category =>
                {
                    Assert.IsTrue(category.CategoryId > 0);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(category.SubcategoryNames));
                });
            }
        }

        [TestMethod]
        public void NiladicFunctionInLinq()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                var firstCategory = database.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name.Left(4)).Concat(),
                        CurrentTimestamp = NiladicFunctions.CurrentTimestamp(),
                        CurrentUser = NiladicFunctions.CurrentUser(),
                        SessionUser = NiladicFunctions.SessionUser(),
                        SystemUser = NiladicFunctions.SystemUser(),
                        User = NiladicFunctions.User()
                    })
                    .First();
                Assert.IsNotNull(firstCategory);
                Assert.IsNotNull(firstCategory.CurrentTimestamp);
                Assert.IsTrue(DateTime.Now >= firstCategory.CurrentTimestamp);
                Assert.AreEqual("dbo", firstCategory.CurrentUser, true, CultureInfo.InvariantCulture);
                Assert.AreEqual("dbo", firstCategory.SessionUser, true, CultureInfo.InvariantCulture);
                Assert.AreEqual($@"{Environment.UserDomainName}\{Environment.UserName}", firstCategory.SystemUser, true, CultureInfo.InvariantCulture);
                Assert.AreEqual("dbo", firstCategory.User, true, CultureInfo.InvariantCulture);
            }
        }
    }
}

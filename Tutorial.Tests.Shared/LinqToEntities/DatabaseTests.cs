namespace Tutorial.Tests.LinqToEntities
{
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;
    using Tutorial.Tests.LinqToObjects;
#else
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;
    using Tutorial.Tests.LinqToObjects;
#endif

    [TestClass]
    public class DatabaseTests
    {
#if NETFX
        [TestMethod]
        public void ContainerTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Assert.AreEqual(nameof(AdventureWorks), adventureWorks.Container().Name);
            }
        }
#endif

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
                Assert.IsTrue(categories.Any(category => category.ProductSubcategories.Any()));
            }
        }

        [TestMethod]
        public void OneToOneTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Person[] people = adventureWorks
                    .People
                    .Include(person => person.Employee)
                    .ToArray();
                EnumerableAssert.Multiple(people);
                EnumerableAssert.Any(people.Where(person => person.Employee != null));

                Employee[] employees = adventureWorks.Employees.Include(employee => employee.Person).ToArray();
                EnumerableAssert.Multiple(employees);
                Assert.IsTrue(employees.All(employee => employee.Person != null));
            }
        }

        [TestMethod]
        public void ManyToManyTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product[] products = adventureWorks
                    .Products
#if NETFX
                    .Include(product => product.ProductProductPhotos.Select(productProductPhoto => productProductPhoto.ProductPhoto))
#else
                    .Include(product => product.ProductProductPhotos).ThenInclude(productProductPhoto => productProductPhoto.ProductPhoto)
#endif
                    .ToArray();
                EnumerableAssert.Multiple(products);
                EnumerableAssert.Any(products.Where(product => product.ProductProductPhotos.Any(productProductPhoto => productProductPhoto.ProductPhoto != null)));

                ProductPhoto[] photos = adventureWorks.ProductPhotos
#if NETFX
                    .Include(photo => photo.ProductProductPhotos.Select(productProductPhoto => productProductPhoto.Product))
#else
                    .Include(photo => photo.ProductProductPhotos).ThenInclude(productProductPhoto => productProductPhoto.Product)
#endif
                    .ToArray();
                EnumerableAssert.Multiple(photos);
                Assert.IsTrue(photos.All(photo => photo.ProductProductPhotos.Any(productProductPhoto => productProductPhoto.Product != null)));
            }
        }

        [TestMethod]
        public void InheritanceTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                SalesTransactionHistory[] transactions = adventureWorks
                    .SalesTransactions
                    .Where(transaction => transaction.ActualCost > 0)
                    .ToArray();
                EnumerableAssert.Multiple(transactions);
            }
        }

#if NETFX
        [TestMethod]
        public void StoredProcedureWithComplexTypeTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = adventureWorks.GetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }
#endif

        [TestMethod]
        public void ViewTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<vEmployee> employees1 = adventureWorks.vEmployees;
                EnumerableAssert.Multiple(employees1);

                IQueryable<vEmployee> employees2 = adventureWorks.vEmployees
                    .Where(product => product.BusinessEntityID > 0 && !string.IsNullOrEmpty(product.JobTitle));
                EnumerableAssert.Multiple(employees2);
            }
        }
    }
}

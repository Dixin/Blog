namespace Dixin.Tests.Linq.EntityFramework
{
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
#endif
    using System.Linq;

    using Dixin.Linq.EntityFramework;
    using Dixin.Linq.Tests;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DatabaseTests
    {
#if NETFX
        [TestMethod]
        public void ContainerTest()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Assert.AreEqual(nameof(WideWorldImporters), adventureWorks.Container().Name);
            }
        }
#endif

        [TestMethod]
        public void TableTest()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Supplier[] categories = adventureWorks
                    .Suppliers
                    .Include(category => category.StockItems)
                    .Where(supplier => supplier.StockItems.Count > 0)
                    .ToArray();
                EnumerableAssert.Any(categories);
                categories.ForEach(category => EnumerableAssert.Any(category.StockItems));
            }
        }

#if NETFX
        [TestMethod]
        public void StoredProcedureWithExecuteStoreQueryTest()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                ObjectResult<ManagerEmployee> employees = adventureWorks.GetManagerEmployees(2);
                EnumerableAssert.Any(employees);
            }
        }
#endif
    }
}

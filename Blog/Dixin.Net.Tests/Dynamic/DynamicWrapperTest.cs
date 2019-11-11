namespace Dixin.Tests.Dynamic
{
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Linq;

    using Dixin.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DynamicWrapperTest
    {
        [TestMethod]
        public void GetInvokeMemberConvertFromTypeTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> query =
                    adventureWorks.Products.Where(product => product.ProductID > 0).OrderBy(p => p.Name).Take(2);
                IEnumerable<Product> results =
                    adventureWorks.ToDynamic().Provider.Execute(query.Expression).ReturnValue;
                Assert.IsTrue(results.Any());
            }
        }
    }

    [Database(Name = "[AdventureWorks]")]
    public class AdventureWorks : DataContext
    {
        public AdventureWorks()
            : base(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30") { }

        public Table<Product> Products => this.GetTable<Product>();
    }

    [Table(Name = "[Production].[Product]")]
    public class Product
    {
        [Column(DbType = "int NOT NULL IDENTITY",
            IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int ProductID { get; set; }

        [Column(DbType = "nvarchar(50) NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }

        [Column(DbType = "money NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public decimal ListPrice { get; set; }
    }
}

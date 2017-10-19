namespace Tutorial.Tests.LinqToEntities
{
#if NETFX
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using Tutorial.LinqToEntities;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SaveChangesTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static Product Create()
        {
            using (AdventureWorks context0 = new AdventureWorks())
            {
                Assert.AreEqual(
                    1,
                    context0.Database.ExecuteSqlCommand(
                        @"INSERT INTO [Production].[Product]
                            ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
                        VALUES
                            ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
                    nameof(Product.Name),
                    InitialListPrice));
                return context0.Products.OrderByDescending(product => product.ProductID).First();
            }
        }

        [TestMethod]
        public void UpdateUpdateStoreWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateUpdateClientWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateUpdateMergeTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateStoreWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateClientWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateMergeTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteStoreWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteClientWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteMergeTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteStoreWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteClientWinsTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteMergeTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }
    }

    [TestClass]
    public class SaveChangesAsyncTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static async Task<Product> CreateAsync()
        {
            using (AdventureWorks context0 = new AdventureWorks())
            {
                Assert.AreEqual(1, await context0.Database.ExecuteSqlCommandAsync(
                    @"INSERT INTO [Production].[Product]
                            ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
                        VALUES
                            ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
                    nameof(Product.Name),
                    InitialListPrice));
                return await context0.Products.OrderByDescending(product => product.ProductID).FirstAsync();
            }
        }

        [TestMethod]
        public async Task UpdateUpdateStoreWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateUpdateClientWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateUpdateMergeAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateStoreWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateClientWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateMergeAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteStoreWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteClientWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteMergeAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteStoreWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteClientWinsAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteMergeAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }
    }

    [TestClass]
    public class SaveChangesWithRetryStrategyTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static Product Create()
        {
            using (AdventureWorks context0 = new AdventureWorks())
            {
                Assert.AreEqual(1, context0.Database.ExecuteSqlCommand(
                    @"INSERT INTO [Production].[Product]
                            ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
                        VALUES
                            ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
                    nameof(Product.Name),
                    InitialListPrice));
                return context0.Products.OrderByDescending(product => product.ProductID).First();
            }
        }

        [TestMethod]
        public void UpdateUpdateStoreWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateUpdateClientWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateUpdateMergeWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateStoreWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateClientWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteUpdateMergeWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteStoreWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteClientWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void DeleteDeleteMergeWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteStoreWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteClientWinsWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public void UpdateDeleteMergeWithRetryStrategyTest()
        {
            using (new TransactionHelper())
            {
                Product product = Create();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }
    }

    [TestClass]
    public class SaveChangesWithRetryStrategyAsyncTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static async Task<Product> CreateAsync()
        {
            using (AdventureWorks context0 = new AdventureWorks())
            {
                Assert.AreEqual(1, await context0.Database.ExecuteSqlCommandAsync(
                    @"INSERT INTO [Production].[Product]
                            ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
                        VALUES
                            ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
                    nameof(Product.Name),
                    InitialListPrice));
                return await context0.Products.OrderByDescending(product => product.ProductID).FirstAsync();
            }
        }

        [TestMethod]
        public async Task UpdateUpdateStoreWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateUpdateClientWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateUpdateMergeWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateStoreWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateClientWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteUpdateMergeWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteStoreWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteClientWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task DeleteDeleteMergeWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteStoreWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteClientWinsWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }

        [TestMethod]
        public async Task UpdateDeleteMergeWithRetryStrategyAsyncTest()
        {
            using (new TransactionHelper())
            {
                Product product = await CreateAsync();
                using (AdventureWorks context1 = new AdventureWorks())
                using (AdventureWorks context2 = new AdventureWorks())
                {
                    Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
                    Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, await context1.SaveChangesAsync());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                using (AdventureWorks context3 = new AdventureWorks())
                {
                    Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            }
        }
    }
#else
    using System.Linq;

    using Tutorial.LinqToEntities;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static TransactionHelper;

    [TestClass]
    public class SaveChangesTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static Product Create(AdventureWorks context0)
        {
            Assert.AreEqual(
                1,
                context0.Database.ExecuteSqlCommand(
                    @"INSERT INTO [Production].[Product]
                        ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate], [Style])
                    VALUES
                        ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE(), N'U')",
                nameof(Product.Name),
                InitialListPrice));
            return context0.Products.OrderByDescending(product => product.ProductID).AsNoTracking().First();
        }

        [TestMethod]
        public void UpdateUpdateStoreWinsTest()
        {
            Rollback((context0, context1, context2, context3) =>
            {
                Product product = Create(context0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateUpdateClientWinsTest()
        {
            Rollback((context0, context1, context2, context3) =>
            {
                Product product = Create(context0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateUpdateMergeTest()
        {
            Rollback((context0, context1, context2, context3) =>
            {
                Product product = Create(context0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateStoreWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateClientWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateMergeTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteStoreWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteClientWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteMergeTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteStoreWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteClientWinsTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteMergeTest()
        {
            Rollback((context1, context2, context3) =>
            {
                Product product = Create(context1);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }
    }

    //[TestClass]
    //public class SaveChangesAsyncTests
    //{
    //    private const string Name1 = nameof(Name1);

    //    private const string Name2 = nameof(Name2);

    //    private const decimal InitialListPrice = 100M;

    //    private const decimal UpdatedListPrice = 555M;

    //    private static readonly int? UpdatedSubcategory = 1;

    //    private static async Task<Product> CreateAsync()
    //    {
    //        using (AdventureWorks context0 = new AdventureWorks())
    //        {
    //            Assert.AreEqual(1, await context0.Database.ExecuteSqlCommandAsync(
    //                @"INSERT INTO [Production].[Product]
    //                        ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
    //                    VALUES
    //                        ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
    //                nameof(Product.Name),
    //                InitialListPrice));
    //            return await context0.Products.OrderByDescending(product => product.ProductID).FirstAsync();
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateStoreWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.IsNull(productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateClientWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = 1;
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name2, productCopy3.Name);
    //                Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
    //                Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateMergeAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = 1;
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateStoreWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateClientWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateMergeAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteStoreWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteClientWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteMergeAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteStoreWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.IsNull(productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteClientWinsAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteMergeAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }
    //}

    [TestClass]
    public class SaveChangesWithRetryStrategyTests
    {
        private const string Name1 = nameof(Name1);

        private const string Name2 = nameof(Name2);

        private const decimal InitialListPrice = 100M;

        private const decimal UpdatedListPrice = 555M;

        private static readonly int? UpdatedSubcategory = 1;

        private static Product Create(AdventureWorks context0)
        {
            Assert.AreEqual(
                1,
                context0.Database.ExecuteSqlCommand(
                    @"INSERT INTO [Production].[Product]
                        ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate], [Style])
                    VALUES
                        ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE(), N'U')",
                    nameof(Product.Name),
                    InitialListPrice));
            return context0.Products.OrderByDescending(product => product.ProductID).AsNoTracking().First();
        }

        [TestMethod]
        public void UpdateUpdateStoreWinsWithRetryStrategyTest()
        {
            Rollback((context0, context1, context2, context3) =>
            {
                Product product = Create(context0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateUpdateClientWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name2, productCopy3.Name);
                    Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateUpdateMergeWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = 1;
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateStoreWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateClientWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteUpdateMergeWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    productCopy2.Name = Name2;
                    productCopy2.ProductSubcategoryID = UpdatedSubcategory;
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteStoreWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteClientWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void DeleteDeleteMergeWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    context1.Products.Remove(productCopy1);
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteStoreWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(0, context2.SaveChanges(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNotNull(productCopy3);
                    Assert.AreEqual(Name1, productCopy3.Name);
                    Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
                    Assert.IsNull(productCopy3.ProductSubcategoryID);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteClientWinsWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }

        [TestMethod]
        public void UpdateDeleteMergeWithRetryStrategyTest()
        {
            Rollback((ontext0, context1, context2, context3) =>
            {
                Product product = Create(ontext0);
                {
                    Product productCopy1 = context1.Products.Find(product.ProductID);
                    Product productCopy2 = context2.Products.Find(product.ProductID);

                    productCopy1.Name = Name1;
                    productCopy1.ListPrice = UpdatedListPrice;
                    Assert.AreEqual(1, context1.SaveChanges());

                    context2.Products.Remove(productCopy2);
                    Assert.AreEqual(1, context2.SaveChanges(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
                }
                {
                    Product productCopy3 = context3.Products.Find(product.ProductID);
                    Assert.IsNull(productCopy3);
                }
            });
        }
    }

    //[TestClass]
    //public class SaveChangesWithRetryStrategyAsyncTests
    //{
    //    private const string Name1 = nameof(Name1);

    //    private const string Name2 = nameof(Name2);

    //    private const decimal InitialListPrice = 100M;

    //    private const decimal UpdatedListPrice = 555M;

    //    private static readonly int? UpdatedSubcategory = 1;

    //    private static async Task<Product> CreateAsync()
    //    {
    //        using (AdventureWorks context0 = new AdventureWorks())
    //        {
    //            Assert.AreEqual(1, await context0.Database.ExecuteSqlCommandAsync(
    //                @"INSERT INTO [Production].[Product]
    //                        ([Name], [ProductNumber], [MakeFlag], [FinishedGoodsFlag], [SafetyStockLevel], [ReorderPoint], [StandardCost], [ListPrice], [DaysToManufacture], [SellStartDate], [rowguid], [ModifiedDate])
    //                    VALUES
    //                        ({0}, N'ProductNumber', 1, 1, 100, 100, 1000, {1}, 1, GETDATE(), NEWID(), GETDATE())",
    //                default,
    //                nameof(Product.Name),
    //                InitialListPrice));
    //            return await context0.Products.OrderByDescending(product => product.ProductID).FirstAsync();
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateStoreWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.IsNull(productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateClientWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = 1;
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name2, productCopy3.Name);
    //                Assert.AreEqual(InitialListPrice, productCopy3.ListPrice);
    //                Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateUpdateMergeWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = 1;
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.AreEqual(UpdatedSubcategory, productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateStoreWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateClientWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteUpdateMergeWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                productCopy2.Name = Name2;
    //                productCopy2.ProductSubcategoryID = UpdatedSubcategory;
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteStoreWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteClientWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task DeleteDeleteMergeWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                context1.Products.Remove(productCopy1);
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteStoreWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(0, await context2.SaveChangesAsync(RefreshConflict.StoreWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNotNull(productCopy3);
    //                Assert.AreEqual(Name1, productCopy3.Name);
    //                Assert.AreEqual(UpdatedListPrice, productCopy3.ListPrice);
    //                Assert.IsNull(productCopy3.ProductSubcategoryID);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteClientWinsWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.ClientWins, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public async Task UpdateDeleteMergeWithRetryStrategyAsyncTest()
    //    {
    //        using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //        {
    //            Product product = await CreateAsync();
    //            using (AdventureWorks context1 = new AdventureWorks())
    //            using (AdventureWorks context2 = new AdventureWorks())
    //            {
    //                Product productCopy1 = await context1.Products.FindAsync(product.ProductID);
    //                Product productCopy2 = await context2.Products.FindAsync(product.ProductID);

    //                productCopy1.Name = Name1;
    //                productCopy1.ListPrice = UpdatedListPrice;
    //                Assert.AreEqual(1, await context1.SaveChangesAsync());

    //                context2.Products.Remove(productCopy2);
    //                Assert.AreEqual(1, await context2.SaveChangesAsync(RefreshConflict.MergeClientAndStore, RetryStrategy.DefaultFixed));
    //            }
    //            using (AdventureWorks context3 = new AdventureWorks())
    //            {
    //                Product productCopy3 = await context3.Products.FindAsync(product.ProductID);
    //                Assert.IsNull(productCopy3);
    //            }
    //        }
    //    }
    //}
#endif
}
namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Transactions;

    internal static class Tracking
    {
        internal static void EntitiesFromSameContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product productById = adventureWorks.Products.Single(product => product.ProductID == 999);
                Product productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // True.
            }
        }

        internal static void MappingsFromSameContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var productById = adventureWorks.Products
                    .Where(product => product.ProductID == 999)
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                var productByName = adventureWorks.Products
                    .Where(product => product.Name == "Road-750 Black, 52")
                    .Select(product => new { Name = product.Name, ListPrice = product.ListPrice })
                    .Single();
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
            }
        }

        internal static void EntitiesFromContexts()
        {
            Product productById;
            Product productByName;
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productById = adventureWorks.Products.Single(product => product.ProductID == 999);
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
            }
            Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // False.
        }

        internal static void Changes()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product insert = new Product() { Name = "Insert", ListPrice = 123 };
                adventureWorks.Products.Add(insert);
                IQueryable<Product> update = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                update.ForEach(product => product.ListPrice += 50);
                IQueryable<Product> delete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML"));
                adventureWorks.Products.RemoveRange(delete);

                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // True.

                adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
                {
                    Product product = tracking.Entity;
                    switch (tracking.State)
                    {
                        case EntityState.Added:
                        case EntityState.Deleted:
                        case EntityState.Unchanged:
                            Trace.WriteLine($"{tracking.State}: ({product.ProductID}, {product.Name}, {product.ListPrice})");
                            break;
                        case EntityState.Modified:
                            Product original = tracking.OriginalValues.ToObject() as Product;
                            Trace.WriteLine($"{tracking.State}: ({original.ProductID}, {original.Name}, {original.ListPrice}) -> ({product.ProductID}, {product.Name}, {product.ListPrice})");
                            break;
                    }
                });
                // Added: (0, Insert, 123)
                // Modified: (951, HL Crankset, 404.9900) -> (951, HL Crankset, 454.9900)
                // Modified: (996, HL Bottom Bracket, 121.4900) -> (996, HL Bottom Bracket, 171.4900)
                // Deleted: (950, ML Crankset, 256.4900)
                // Deleted: (995, ML Bottom Bracket, 101.2400)
            }
        }

        internal static void Attach()
        {
            Product product = new Product() { Name = "On the fly", ListPrice = 1 };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                product.ListPrice = 2;
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Any()); // False.
                adventureWorks.Products.Attach(product);
                product.ListPrice = 3;
                adventureWorks.ChangeTracker.Entries<Product>()
                    .Where(tracking => tracking.State == EntityState.Modified)
                    .ForEach(tracking => Trace.WriteLine(
                        $"{tracking.OriginalValues[nameof(Product.ListPrice)]} -> {tracking.CurrentValues[nameof(Product.ListPrice)]}")); // 2 -> 3.
            }
        }

        internal static void AssociationChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Include(entity => entity.ProductSubcategories).First();
                Trace.WriteLine(category.ProductSubcategories.Count); // 12.
                ProductSubcategory[] subcategories = category.ProductSubcategories.ToArray();
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == category)); // True.

                category.ProductSubcategories.Clear();
                Trace.WriteLine(category.ProductSubcategories.Count); // 0.
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Count(tracking => tracking.State != EntityState.Unchanged)); // 12.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == null)); // True.
            }
        }
    }

    internal static class Changes
    {
        internal static int Insert()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = new ProductCategory() { Name = "Category" };
                ProductSubcategory subcategory = new ProductSubcategory() { Name = "Subcategory" };
                category.ProductSubcategories.Add(subcategory);
                adventureWorks.ProductCategories.Add(category);

                Trace.WriteLine(category.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductCategoryID); // null.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0.

                Trace.WriteLine(adventureWorks.SaveChanges()); // 2

                Trace.WriteLine(category.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 38.
                return subcategory.ProductSubcategoryID; // Save for later.
            }
        }

        internal static void Update()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.First();
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Single(entity => entity.Name == "Subcategory");
                Trace.WriteLine(subcategory.Name); // Subcategory.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.

                subcategory.Name = "Update"; // Update property.
                subcategory.ProductCategory = category; // Update association.

                Trace.WriteLine(adventureWorks.SaveChanges()); // 1.

                Trace.WriteLine(subcategory.Name); // Update.
                Trace.WriteLine(subcategory.ProductCategoryID); // 4.
            }
        }

        internal static void UpdateWithNoChange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product product = adventureWorks.Products.Find(999);
                product.ListPrice += product.ListPrice;
                product.ListPrice /= 2; // Change tracked entity then change back.
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Any(tracking =>
                    tracking.State != EntityState.Unchanged)); // False.
                Trace.WriteLine(adventureWorks.SaveChanges()); // 0.
            }
        }

        internal static void Delete()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                adventureWorks.ProductCategories.Remove(category);
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
            }
        }

        internal static void DeleteWithNoQuery(int subcategoryId)
        {
            ProductSubcategory subcategory = new ProductSubcategory() { ProductSubcategoryID = subcategoryId };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductSubcategories.Attach(subcategory);
                adventureWorks.ProductSubcategories.Remove(subcategory);
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
            }
        }

        internal static void DeleteWithAssociation()
        {
            Insert(); // Insert ProductCategory "Category" and ProductSubcategory "Subcategory".
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                    .Single(entity => entity.Name == "Subcategory");
                adventureWorks.ProductCategories.Remove(category);
                adventureWorks.ProductSubcategories.Remove(subcategory);
                Trace.WriteLine(adventureWorks.SaveChanges()); // 2.
            }
        }

        internal static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> products = adventureWorks.Products.Take(10).AsNoTracking();
                adventureWorks.Products.RemoveRange(products);
                Trace.WriteLine(adventureWorks.SaveChanges());
                // InvalidOperationException: The object cannot be deleted because it was not found in the ObjectStateManager.
            }
        }
    }

    internal static class Transactions
    {
        internal static void Implicit()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.First();
                Trace.WriteLine(category.Name); // Accessories.
                category.Name = "Update";
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First();
                Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                subcategory.ProductCategoryID = -1;
                try
                {
                    adventureWorks.SaveChanges();
                }
                catch (DbUpdateException exception)
                {
                    Trace.WriteLine(exception);
                    // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                    // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details. 
                    // ---> System.Data.SqlClient.SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                    adventureWorks.Entry(category).Reload();
                    Trace.WriteLine(category.Name); // Accessories.
                    adventureWorks.Entry(subcategory).Reload();
                    Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                }
            }
        }

        internal static void ExplicitLocal()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbConnection connection = adventureWorks.Database.Connection)
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        adventureWorks.Database.UseTransaction(transaction);
                        ProductCategory category = new ProductCategory() { Name = "Transaction" };
                        adventureWorks.ProductCategories.Add(category);
                        Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = N'Transaction'";
                            command.Transaction = transaction;
                            Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static void ExplicitDistributable()
        {
            using (TransactionScope scope = new TransactionScope())
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbConnection connection = adventureWorks.Database.Connection)
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO [Production].[ProductCategory] ([Name]) VALUES (N'Transaction')";
                    Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                }
                ProductCategory category = adventureWorks.ProductCategories.Single(entity => entity.Name == "Transaction");
                adventureWorks.ProductCategories.Remove(category);
                Trace.WriteLine(adventureWorks.SaveChanges()); // 1.
                scope.Complete();
            }
        }
    }

    internal partial class Client : IDisposable
    {
        private readonly AdventureWorks adventureWorks = new AdventureWorks();

        internal TEntity Read<TEntity>
            (params object[] keys) where TEntity : class =>
                this.adventureWorks.Set<TEntity>().Find(keys);

        internal int Write(Action write)
        {
            write();
            return this.adventureWorks.SaveChanges();
        }

        public void Dispose() => this.adventureWorks.Dispose();
    }

    internal static partial class Concurrency
    {
        internal static void DefaultControl() // Check no column, last client wins.
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (Client client1 = new Client())
            using (Client client2 = new Client())
            using (Client client3 = new Client())
            {
                const int id = 1;
                ProductCategory category1 = client1.Read<ProductCategory>(id);
                ProductCategory category2 = client2.Read<ProductCategory>(id);
                client1.Write(() => category1.Name = nameof(client1));
                client2.Write(() => category2.Name = nameof(client2));

                Trace.WriteLine(client3.Read<ProductCategory>(id).Name); // client2.
            } // ROLLBACK TRANSACTION.
        }
    }

#if DEMO
    public partial class ProductPhoto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public DateTime ModifiedDate { get; set; }
    }
#endif

    internal static partial class Concurrency
    {
        internal static void CheckModifiedDate()
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (Client client1 = new Client())
            using (Client client2 = new Client())
            {
                const int id = 1;
                ProductPhoto photo1 = client1.Read<ProductPhoto>(id);
                ProductPhoto photo2 = client2.Read<ProductPhoto>(id);
                client1.Write(() => photo1.LargePhotoFileName = nameof(client1));
                client2.Write(() => photo2.LargePhotoFileName = nameof(client2)); // DbUpdateConcurrencyException.
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class Product
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    internal partial class Client
    {
        internal void Write(Action write, Action<IEnumerable<DbEntityEntry>> resolve, int retryCount = 3)
        {
            write();
            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    this.adventureWorks.SaveChanges();
                }
                catch (DbUpdateConcurrencyException exception) when (retry < retryCount - 1)
                {
                    resolve(exception.Entries);
                }
            }
        }
    }

    internal static partial class Concurrency
    {
        internal static void Conflict(Action<IEnumerable<DbEntityEntry>> resolve)
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (Client client1 = new Client())
            using (Client client2 = new Client())
            using (Client client3 = new Client())
            {
                const int id = 999;
                Product product1 = client1.Read<Product>(id);
                Product product2 = client2.Read<Product>(id);
                client1.Write(() => { product1.Name = nameof(client1); product1.ListPrice = 0; });
                client2.Write(() => { product2.Name = nameof(client2); product2.ProductSubcategoryID = null; }, resolve);

                Product product3 = client3.Read<Product>(id);
                Trace.WriteLine($"({product3.Name}, {product3.ListPrice}, {product3.ProductSubcategoryID})");
            } // ROLLBACK TRANSACTION.
        }
        
        private static string String
            (this object cell) => cell is byte[] ? BitConverter.ToString((byte[])cell) : cell.ToString();

        internal static void DatabaseWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                DbPropertyValues clientOriginalValues = conflict.OriginalValues.Clone();
                conflict.Reload();
                DbPropertyValues databaseValues = conflict.CurrentValues;
                databaseValues
                    .PropertyNames
                    .Where(property=> !object.Equals(clientOriginalValues[property], databaseValues[property]))
                    .ForEach(property => Trace.WriteLine(
                        $"{property}: client {clientOriginalValues[property].String()}, database {databaseValues[property].String()}"));
            })); 
        // RowVersion: client 00-00-00-00-00-00-08-03, database 00-00-00-00-00-03-34-51
        // Name: client Road-750 Black, 52, database task1
        // ListPrice: client 539.9900, database 0.0000
        // (task1, 0.0000, 2)

        internal static void ClientWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                DbPropertyValues databaseValues = conflict.GetDatabaseValues();
                DbPropertyValues clientOriginalValues = conflict.OriginalValues;
                databaseValues
                    .PropertyNames
                    .Where(property => !object.Equals(clientOriginalValues[property], databaseValues[property]))
                    .ForEach(property => Trace.WriteLine(
                        $"{property}: client {clientOriginalValues[property].String()} -> {conflict.CurrentValues[property].String()}, database {databaseValues[property].String()}."));
                clientOriginalValues.SetValues(databaseValues);
            })); 
        // RowVersion: client 00-00-00-00-00-00-08-03 -> 00-00-00-00-00-00-08-03, database 00-00-00-00-00-03-34-52.
        // Name: client Road-750 Black, 52 -> task2, database task1.
        // ListPrice: client 539.9900 -> 539.9900, database 0.0000.
        // (task2, 539.9900, )

        internal static void MergeClientAndDatabase() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                DbPropertyValues databaseValues = conflict.GetDatabaseValues();
                DbPropertyValues clientOriginalValues = conflict.OriginalValues;
                DbPropertyValues clientUpdatedValues = conflict.CurrentValues;
                databaseValues
                    .PropertyNames
                    .Where(property => !object.Equals(clientOriginalValues[property], databaseValues[property]))
                    .ForEach(property =>
                    {
                        Trace.WriteLine(
                            $"{property}: client {clientOriginalValues[property].String()} -> {clientUpdatedValues[property].String()}, database {databaseValues[property].String()}");
                        clientUpdatedValues[property] = databaseValues[property];
                    });
                conflict.OriginalValues.SetValues(databaseValues);
            })); 
        // RowVersion: client 00-00-00-00-00-00-08-03 -> 00-00-00-00-00-00-08-03, database 00-00-00-00-00-03-34-54
        // Name: client Road-750 Black, 52 -> task2, database task1
        // ListPrice: client 539.9900 -> 539.9900, database 0.0000
        // (task1, 0.0000, )
    }
}

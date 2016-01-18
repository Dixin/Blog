namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    public static class Tracking
    {
        public static void EntitiesFromSameContext()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product productById = adventureWorks.Products.Single(product => product.ProductID == 999);
                Product productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
                Trace.WriteLine(object.ReferenceEquals(productById, productByName)); // True.
            }
        }

        public static void MappingsFromSameContext()
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

        public static void EntitiesFromContexts()
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

        public static void Changes()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> update = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                update.ForEach(product => product.ListPrice += 50);
                IQueryable<Product> delete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML"));
                adventureWorks.Products.RemoveRange(delete);
                Product insert = new Product() { Name = "Insert", ListPrice = 123 };
                adventureWorks.Products.Add(insert);
                Trace.WriteLine(adventureWorks.ChangeTracker.HasChanges()); // True.
                adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
                {
                    Trace.Write($"{tracking.State}: ");
                    Product product = tracking.Entity;
                    switch (tracking.State)
                    {
                        case EntityState.Added:
                        case EntityState.Deleted:
                        case EntityState.Unchanged:
                            Trace.WriteLine($"{product.ProductID}, {product.Name}, {product.ListPrice}");
                            break;
                        case EntityState.Modified:
                            Trace.WriteLine(string.Join(", ", tracking.CurrentValues.PropertyNames.Select(
                                property => $"{tracking.OriginalValues[property]} -> {tracking.CurrentValues[property]}")));
                            break;
                    }
                });
                // Added: 0, Insert, 123
                // Modified: 951 -> 951, 8 -> 8, HL Crankset -> HL Crankset, 404.9900 -> 454.9900
                // Modified: 996 -> 996, 5 -> 5, HL Bottom Bracket -> HL Bottom Bracket, 121.4900 -> 171.4900
                // Deleted: 950, ML Crankset, 256.4900
                // Deleted: 995, ML Bottom Bracket, 101.2400
            }
        }

        public static void Attach()
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

        public static void AssociationChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Include(entity => entity.ProductSubcategories).First();
                ProductSubcategory[] subcategories = category.ProductSubcategories.ToArray();
                Trace.WriteLine(category.ProductSubcategories.Count); // 12.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == category)); // True.

                category.ProductSubcategories.Clear();
                Trace.WriteLine(category.ProductSubcategories.Count); // 0.
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries<Product>().Any()); // False.
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == null)); // True.
            }
        }
    }

    public static class Changes
    {
        public static void Insert()
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

                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 

                Trace.WriteLine(category.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 38.
                subcategoryId = subcategory.ProductSubcategoryID;
            }
        }

        private static int subcategoryId;

        public static void Update()
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

                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 

                Trace.WriteLine(subcategory.Name); // Subcategory update.
                Trace.WriteLine(subcategory.ProductCategoryID); // 4.
            }
        }

        public static void UpdateWithNoChange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories.AsNoTracking().Single(entity => entity.Name == "Category");
                category.Name = "Update"; // Change untracked entity.
                Product product = adventureWorks.Products.Single(entity => entity.ProductID == 999);
                product.ListPrice += product.ListPrice;
                product.ListPrice /= 2; // Change tracked entity then change back.
                Trace.WriteLine(adventureWorks.ChangeTracker.Entries().Any(tracking =>
                    tracking.State != EntityState.Unchanged)); // False.
                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 0.
            }
        }

        public static void Delete()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                adventureWorks.ProductCategories.Remove(category);
                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 1.
            }
        }

        public static void DeleteWithNoQuery()
        {
            ProductSubcategory subcategory = new ProductSubcategory() { ProductSubcategoryID = subcategoryId };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductSubcategories.Attach(subcategory);
                adventureWorks.ProductSubcategories.Remove(subcategory);
                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 1.
            }
        }

        public static void DeleteWithAssociation()
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
                int savedCount = adventureWorks.SaveChanges();
                Trace.WriteLine(savedCount); // 
            }
        }

        public static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> products = adventureWorks.Products.Take(10).AsNoTracking();
                adventureWorks.Products.RemoveRange(products);
                adventureWorks.SaveChanges();
                // InvalidOperationException: The object cannot be deleted because it was not found in the ObjectStateManager.
            }
        }
    }

    public static class Transaction
    {
        public static void Implicit()
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
                    category = adventureWorks.ProductCategories.AsNoTracking().First();
                    Trace.WriteLine(category.Name); // Accessories.
                    subcategory = adventureWorks.ProductSubcategories.AsNoTracking().First();
                    Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                }
            }
        }

        public static void ExplicitLocal()
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

        public static void ExplicitDistributable()
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

    public static partial class Concurrency
    {
        public static async Task UpdateWithDelayAsync(Action<AdventureWorks> readWrite)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                readWrite(adventureWorks);
                await Task.Delay(TimeSpan.FromSeconds(2));
                adventureWorks.SaveChanges();
            }
        }

        public static async Task LastWins() // No control.
        {
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) // BEGIN TRANSACTION.
            {
                const int id = 1;
                Task task1 = UpdateWithDelayAsync(adventureWorks =>
                    adventureWorks.ProductCategories.Find(id).Name = nameof(task1));
                await Task.Delay(TimeSpan.FromSeconds(id));
                Task task2 = UpdateWithDelayAsync(adventureWorks =>
                    adventureWorks.ProductCategories.Find(id).Name = nameof(task2));
                await Task.WhenAll(task1, task2); // task2 wins.

                using (AdventureWorks adventureWorks = new AdventureWorks())
                {
                    Trace.WriteLine(adventureWorks.ProductCategories.Find(id).Name); // task2.
                }
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class ProductPhoto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public DateTime ModifiedDate { get; set; }
    }

    public static partial class Concurrency
    {
        public static async Task Check()
        {
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) // BEGIN TRANSACTION.
            {
                const int id = 1;
                Task task1 = UpdateWithDelayAsync(adventureWorks =>
                    adventureWorks.ProductPhotos.Find(id).LargePhotoFileName = nameof(task1));
                await Task.Delay(TimeSpan.FromSeconds(1));
                Task task2 = UpdateWithDelayAsync(adventureWorks =>
                    adventureWorks.ProductPhotos.Find(id).LargePhotoFileName = nameof(task2));
                await Task.WhenAll(task1, task2);
                // AggregateException: One or more errors occurred.
                // ---> DbUpdateConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded.
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class Product
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class Concurrency
    {
        public static async Task UpdateWithRetryAsync(
            Action<AdventureWorks> readWrite, Action<DbUpdateConcurrencyException> resolve, int retryCount = 3)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                readWrite(adventureWorks);
                await Task.Delay(TimeSpan.FromSeconds(2));
                for (int retry = 0; retry < retryCount; retry++)
                {
                    try
                    {
                        adventureWorks.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException exception) when (retry < retryCount - 1)
                    {
                        resolve(exception);
                    }
                }
            }
        }

        public static async Task ResolveConflict(Action<DbUpdateConcurrencyException> resolve)
        {
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) // BEGIN TRANSACTION.
            {
                const int id = 999;
                Task task1 = UpdateWithDelayAsync(
                    adventureWorks =>
                        {
                            Product product = adventureWorks.Products.Find(id);
                            product.Name = nameof(task1);
                            product.ListPrice = 0;
                        });
                await Task.Delay(TimeSpan.FromSeconds(1));
                Task task2 = UpdateWithRetryAsync(
                    adventureWorks =>
                        {
                            Product product = adventureWorks.Products.Find(id);
                            product.Name = nameof(task2);
                            product.ProductSubcategoryID = null;
                        },
                    resolve);
                await Task.WhenAll(task1, task2);

                using (AdventureWorks adventureWorks = new AdventureWorks())
                {
                    Product product = adventureWorks.Products.Find(id);
                    Trace.WriteLine($"({product.Name}, {product.ListPrice}, {product.ProductSubcategoryID})");
                    // (task1, 0.0000, 2).
                }
            } // ROLLBACK TRANSACTION.
        }

        public static Task DatabaseWins() => ResolveConflict(exception =>
            exception.Entries.Single().Reload()); // (task1, 0.0000, 2).

        public static Task ClientWins() => ResolveConflict(exception =>
            {
                DbEntityEntry entry = exception.Entries.Single();
                entry.OriginalValues.SetValues(entry.GetDatabaseValues());
            }); // (task2, 539.9900, ).

        public static Task MergeClientAndDatabase() => ResolveConflict(exception =>
            {
                DbEntityEntry entry = exception.Entries.Single();
                DbPropertyValues databaseValues = entry.GetDatabaseValues();
                databaseValues.PropertyNames.ForEach(property =>
                    {
                        if (!object.Equals(databaseValues[property], entry.OriginalValues[property]))
                        {
                            entry.CurrentValues[property] = databaseValues[property];
                        }
                    });
                entry.OriginalValues.SetValues(databaseValues);
            }); // (task1, 0.0000, ).
    }
}

namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Data.Common;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Data.SqlClient;
    using System.Diagnostics;
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
                Product insert = new UniversalProduct() { Name = "Insert", ListPrice = 123 };
                // Product insert = new Product() causes InvalidOperationException for InsertOnSubmit:
                // InvalidOperationException: Instance of type 'Dixin.Linq.LinqToSql.Product' could not be added. This type is not part of the mapped type system.
                adventureWorks.Products.InsertOnSubmit(insert);
                IQueryable<Product> update = adventureWorks.Products
                    .Where(product => product.Name.Contains("HL"));
                update.ForEach(product => product.ListPrice += 50);
                IQueryable<Product> delete = adventureWorks.Products
                    .Where(product => product.Name.Contains("ML"));
                adventureWorks.Products.DeleteAllOnSubmit(delete);

                ChangeSet changeSet = adventureWorks.GetChangeSet();
                Trace.WriteLine(changeSet.Inserts.Any()); // True.
                Trace.WriteLine(changeSet.Updates.Any()); // True.
                Trace.WriteLine(changeSet.Deletes.Any()); // True.

                changeSet.Inserts.OfType<Product>().ForEach(product => Trace.WriteLine(
                    $"{nameof(ChangeSet.Inserts)}: ({product.ProductID}, {product.Name}, {product.ListPrice})"));
                changeSet.Updates.OfType<Product>().ForEach(product =>
                    {
                        Product original = adventureWorks.Products.GetOriginalEntityState(product);
                        Trace.WriteLine($"{nameof(ChangeSet.Updates)}: ({original.ProductID}, {original.Name}, {original.ListPrice}) -> ({product.ProductID}, {product.Name}, {product.ListPrice})");
                    });
                changeSet.Deletes.OfType<Product>().ForEach(product => Trace.WriteLine(
                    $"{nameof(ChangeSet.Deletes)}: ({product.ProductID}, {product.Name}, {product.ListPrice})"));
                // Inserts: (0, Insert, 123)
                // Updates: (951, HL Crankset, 404.9900) -> (951, HL Crankset, 454.9900)
                // Updates: (996, HL Bottom Bracket, 121.4900) -> (996, HL Bottom Bracket, 171.4900)
                // Deletes: (950, ML Crankset, 256.4900)
                // Deletes: (995, ML Bottom Bracket, 101.2400)
            }
        }

        internal static void Attach()
        {
            Product product = new UniversalProduct() { Name = "On the fly", ListPrice = 1 };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                product.ListPrice = 2;
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Any()); // False.
                adventureWorks.Products.Attach(product);
                product.ListPrice = 3;
                adventureWorks.GetChangeSet().Updates.OfType<Product>().ForEach(attached => Trace.WriteLine(
                    $"{adventureWorks.Products.GetOriginalEntityState(attached).ListPrice} -> {attached.ListPrice}")); // 2 -> 3.
            }
        }

        internal static void AssociationChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                DataLoadOptions loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<ProductCategory>(entity => entity.ProductSubcategories);
                adventureWorks.LoadOptions = loadOptions;
                ProductCategory category = adventureWorks.ProductCategories.First();
                Trace.WriteLine(category.ProductSubcategories.Count); // 12.
                ProductSubcategory[] subcategories = category.ProductSubcategories.ToArray();
                Trace.WriteLine(subcategories.All(subcategory => subcategory.ProductCategory == category)); // True.

                category.ProductSubcategories.Clear();
                Trace.WriteLine(category.ProductSubcategories.Count); // 0.
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Count); // 12.
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
                adventureWorks.ProductCategories.InsertOnSubmit(category);

                Trace.WriteLine(category.ProductCategoryID); // 0.
                Trace.WriteLine(subcategory.ProductCategoryID); // null.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 0.

                adventureWorks.SubmitChanges();

                Trace.WriteLine(category.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductCategoryID); // 5.
                Trace.WriteLine(subcategory.ProductSubcategoryID); // 38.
                return subcategory.ProductSubcategoryID;
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

                adventureWorks.SubmitChanges();

                Trace.WriteLine(subcategory.Name); // Subcategory update.
                Trace.WriteLine(subcategory.ProductCategoryID); // 4.
            }
        }

        internal static void UpdateWithNoChange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product product = adventureWorks.Find<Product>(999);
                product.ListPrice += product.ListPrice;
                product.ListPrice /= 2; // Change tracked entity then change back.
                Trace.WriteLine(adventureWorks.GetChangeSet().Updates.Any()); // False.
                adventureWorks.SubmitChanges();
            }
        }

        internal static void Delete()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Category");
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.SubmitChanges();
            }
        }

        internal static void DeleteWithNoQuery(int subcategoryId)
        {
            ProductSubcategory subcategory = new ProductSubcategory() { ProductSubcategoryID = subcategoryId };
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductSubcategories.Attach(subcategory, false);
                adventureWorks.ProductSubcategories.DeleteOnSubmit(subcategory);
                adventureWorks.SubmitChanges();
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
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.ProductSubcategories.DeleteOnSubmit(subcategory);
                adventureWorks.SubmitChanges();
            }
        }

        internal static void UntrackedChanges()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ObjectTrackingEnabled = false;
                IQueryable<Product> products = adventureWorks.Products.Take(10);
                adventureWorks.Products.DeleteAllOnSubmit(products);
                adventureWorks.SubmitChanges();
                // InvalidOperationException: Object tracking is not enabled for the current data context instance.
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
                    adventureWorks.SubmitChanges();
                }
                catch (SqlException exception)
                {
                    Trace.WriteLine(exception);
                    // SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                    adventureWorks.Refresh(RefreshMode.OverwriteCurrentValues, category, subcategory);
                    Trace.WriteLine(category.Name); // Accessories.
                    Trace.WriteLine(subcategory.ProductCategoryID); // 1.
                }
            }
        }

        internal static void ExplicitLocal()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbConnection connection = adventureWorks.Connection)
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        adventureWorks.Transaction = transaction;
                        ProductCategory category = new ProductCategory() { Name = "Transaction" };
                        adventureWorks.ProductCategories.InsertOnSubmit(category);
                        adventureWorks.SubmitChanges();
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = N'Transaction'";
                            command.Transaction = transaction;
                            Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                        }
                        transaction.Commit();
                    }
                    catch(Exception)
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
            using (DbConnection connection = adventureWorks.Connection)
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO [Production].[ProductCategory] ([Name]) VALUES (N'Transaction')";
                    Trace.WriteLine(command.ExecuteNonQuery()); // 1.
                }
                ProductCategory category = adventureWorks.ProductCategories.Single(entity => entity.Name == "Transaction");
                adventureWorks.ProductCategories.DeleteOnSubmit(category);
                adventureWorks.SubmitChanges();
                scope.Complete();
            }
        }
    }

    internal partial class Client : IDisposable
    {
        private readonly AdventureWorks adventureWorks = new AdventureWorks();

        internal TEntity Read<TEntity>
            (params object[] keys) where TEntity : class => 
                this.adventureWorks.Find<TEntity>(keys);

        internal void Write(Action write)
        {
            write();
            this.adventureWorks.SubmitChanges();
        }

        public void Dispose() => this.adventureWorks.Dispose();
    }

    internal static partial class Concurrency
    {
        internal static void DefaultControl() // Check all columns, first client wins.
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
                try
                {
                    client2.Write(() => category2.Name = nameof(client2));
                }
                catch (ChangeConflictException exception)
                {
                    Trace.WriteLine(exception); // Row not found or changed.
                }

                Trace.WriteLine(client3.Read<ProductCategory>(id).Name); // client1.
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class ProductPhoto
    {
        [Column(DbType = "datetime NOT NULL", UpdateCheck = UpdateCheck.Always)]
        public DateTime ModifiedDate { get; set; }
    }

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
                client2.Write(() => photo2.LargePhotoFileName = nameof(client2)); // ChangeConflictException.
            } // ROLLBACK TRANSACTION.
        }
    }

    public partial class Product
    {
        [Column(AutoSync = AutoSync.Always, DbType = "rowversion NOT NULL",
            CanBeNull = false, IsDbGenerated = true, IsVersion = true, UpdateCheck = UpdateCheck.Never)]
        public Binary RowVersion { get; set; }
    }

    internal partial class Client
    {
        internal void Write(Action write, Action<ChangeConflictCollection> resolve, int retryCount = 3)
        {
            write();
            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    this.adventureWorks.SubmitChanges();
                }
                catch (ChangeConflictException) when (retry < retryCount - 1)
                {
                    resolve(this.adventureWorks.ChangeConflicts);
                }
            }
        }
    }

    internal static partial class Concurrency
    {
        internal static void Conflict(Action<ChangeConflictCollection> resolve)
        {
            using (new TransactionScope()) // BEGIN TRANSACTION.
            using (Client client1 = new Client())
            using (Client client2 = new Client())
            using (Client client3 = new Client())
            using (Client client4 = new Client())
            {
                const int id = 999;
                Product product1 = client1.Read<Product>(id);
                Product product2 = client2.Read<Product>(id);
                client1.Write(() => { product1.Name = nameof(client1); product1.ListPrice = 0; });
                Product product4 = client4.Read<Product>(id);
                Trace.WriteLine($"({product4.Name}, {product4.ListPrice}, {product4.ProductSubcategoryID})");
                client2.Write(() => { product2.Name = nameof(client2); product2.ProductSubcategoryID = null; }, resolve);

                Product product3 = client3.Read<Product>(id);
                Trace.WriteLine($"({product3.Name}, {product3.ListPrice}, {product3.ProductSubcategoryID})");
            } // ROLLBACK TRANSACTION.
        }

        internal static void DatabaseWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                    $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
                conflict.Resolve(RefreshMode.OverwriteCurrentValues);
            }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/Y="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client1, 0.0000, 2)

        internal static void ClientWins() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                    $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
                conflict.Resolve(RefreshMode.KeepCurrentValues);
            }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/c="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client2, 539.9900, )

        internal static void MergeClientAndDatabase() => Conflict(conflicts => conflicts.ForEach(conflict =>
            {
                conflict.MemberConflicts.ForEach(member => Trace.WriteLine(
                    $"{member.Member.Name}: client: {member.OriginalValue} -> {member.CurrentValue}, database: {member.DatabaseValue}"));
                conflict.Resolve(RefreshMode.KeepChanges);
            }));
        // RowVersion: client: "AAAAAAAACAM=" -> "AAAAAAAACAM=", database: "AAAAAAADQ/k="
        // Name: client: Road-750 Black, 52 -> client2, database: client1
        // ListPrice: client: 539.9900 -> 539.9900, database: 0.0000
        // (client2, 0.0000, )
    }
}

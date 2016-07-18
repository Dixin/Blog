namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Transactions;
    using Dixin.Common;
    using Dixin.Linq.EntityFramework.Full;
    using Dixin.Linq.EntityFramework.FullWithViews;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using IsolationLevel = System.Data.IsolationLevel;

    public class LegacyAdventureWorks : ObjectContext
    {
        private ObjectSet<Product> products;

        public LegacyAdventureWorks()
            : base(new AdventureWorks().ObjectContext().Connection as EntityConnection)
        {
        }

        public ObjectSet<Product> Products => this.products ?? (this.products = this.CreateObjectSet<Product>());
    }

    internal static class CompiledQueries
    {
        private static readonly Func<LegacyAdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((LegacyAdventureWorks adventureWorks, decimal listPrice) => adventureWorks
                .Products
                .Where(product => product.ListPrice == listPrice)
                .Select(product => product.Name));

        internal static IQueryable<string> GetProductNames
            (this LegacyAdventureWorks adventureWorks, decimal listPrice) =>
                GetProductNamesCompiled(adventureWorks, listPrice);
    }

    internal static partial class Performance
    {
        internal static void AddRange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductCategories.Load(); // Warm up.
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<ProductCategory> categories = Enumerable
                    .Range(0, 100).Select(index => new ProductCategory() { Name = index.ToString() });
                DbSet<ProductCategory> repository = adventureWorks.ProductCategories;
                foreach (ProductCategory category in categories)
                {
                    repository.Add(category);
                }
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<ProductCategory> categories = Enumerable
                    .Range(0, 100).Select(index => new ProductCategory() { Name = index.ToString() });
                adventureWorks.ProductCategories.AddRange(categories);
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 2
            }
        }

        internal static void RemoveRange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Products.Load(); // Warm up.
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Product[] products = adventureWorks.Products.ToArray();
                DbSet<Product> repository = adventureWorks.Products;
                foreach (Product product in products)
                {
                    repository.Remove(product);
                }
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Product[] products = adventureWorks.Products.ToArray();
                adventureWorks.Products.RemoveRange(products);
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 2
            }
        }

        internal static void MappingViews()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            using (FullAdventureWorks adventureWorks = new FullAdventureWorks())
            {
                adventureWorks.Production_ProductCategories.Load();
            }
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.ElapsedMilliseconds); // 4786

            stopwatch.Restart();
            using (FullAdventureWorksWithViews adventureWorks = new FullAdventureWorksWithViews())
            {
                adventureWorks.Production_ProductCategories.Load();
            }
            stopwatch.Stop(); // 1340
            Trace.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        internal static void CachedEntity()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category1 = adventureWorks.ProductCategories
                    .Single(entity => entity.ProductCategoryID == 1);
                category1.Name = "Cache";

                ProductCategory category2 = adventureWorks.ProductCategories
                    .Single(entity => entity.Name == "Bikes");
                Trace.WriteLine(category2.Name); // Cache
                Trace.WriteLine(category1 == category2); // True

                ProductCategory category3 = adventureWorks.ProductCategories
                    .SqlQuery(@"
                        SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                        FROM [Production].[ProductCategory]
                        ORDER BY [ProductCategory].[ProductCategoryID]")
                    .Single();
                Trace.WriteLine(category1 == category3); // True
            }
        }

        internal static void UncachedEntity()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category1 = adventureWorks.ProductCategories
                    .Single(entity => entity.ProductCategoryID == 1);
                category1.Name = "Cache";

                ProductCategory category2 = adventureWorks.ProductCategories
                    .AsNoTracking().Single(entity => entity.Name == "Bikes");
                Trace.WriteLine(category2.Name); // Bikes
                Trace.WriteLine(category1 == category2); // False

                ProductCategory category3 = adventureWorks.Database
                    .SqlQuery<ProductCategory>(@"
                        SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                        FROM [Production].[ProductCategory]
                        ORDER BY [ProductCategory].[ProductCategoryID]")
                    .Single();
                Trace.WriteLine(category1 == category3); // False
            }
        }

        internal static void Find()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product[] products = adventureWorks.Products
                    .Where(product => product.Name.StartsWith("Road")).ToArray(); // SELECT.
                Product fromCache = adventureWorks.Products.Find(999); // No database query.
                Trace.WriteLine(products.Contains(fromCache)); // True
            }
        }

        internal static void TranslationCache()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int minLength = 1;
                IQueryable<ProductCategory> query = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= minLength)
                    .Include(category => category.ProductSubcategories);
                query.Load();
            }
        }

        internal static void UncachedTranslation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> queryWithConstant1 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= 1);
                queryWithConstant1.Load();

                IQueryable<ProductCategory> queryWithConstant2 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= 10);
                queryWithConstant2.Load();
            }
        }

        internal static void CachedTranslation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int minLength = 1;
                IQueryable<ProductCategory> queryWithClosure1 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= minLength);
                queryWithClosure1.Load();

                minLength = 10;
                IQueryable<ProductCategory> queryWithClosure2 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= minLength);
                queryWithClosure2.Load();
            }
        }

        [CompilerGenerated]
        private sealed class DisplayClass1
        {
            public int minLength;
        }

        [CompilerGenerated]
        private sealed class DisplayClass2
        {
            public int minLength;
        }

        internal static void CompiledCachedTranslation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int minLength = 1;
                DisplayClass1 displayClass1 = new DisplayClass1() { minLength = minLength };
                IQueryable<ProductCategory> queryWithClosure1 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= displayClass1.minLength);
                queryWithClosure1.Load();

                minLength = 10;
                DisplayClass1 displayClass2 = new DisplayClass1() { minLength = minLength };
                IQueryable<ProductCategory> queryWithClosure2 = adventureWorks.ProductCategories
                    .Where(category => category.Name.Length >= displayClass2.minLength);
                queryWithClosure2.Load();
            }
        }

        internal static void Translation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Products.Load(); // Warm up.
            }

            Func<string, Expression<Func<Product, bool>>> getPredicateWithConstant = startWith =>
                {
                    ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
                    Func<string, bool> startsWithMethod = string.Empty.StartsWith;
                    Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                        Expression.Call(
                            instance: Expression.Property(productParameterExpression, nameof(Product.Name)),
                            method: startsWithMethod.Method,
                            arguments: Expression.Constant(startWith, typeof(string))),
                        productParameterExpression);
                    return predicateExpression;
                };

            Func<string, Expression<Func<Product, bool>>> getPredicateWithVariable =
                startWith => product => product.Name.StartsWith(startWith);

            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Enumerable.Range(0, 1000).ForEach(value =>
                    {
                        IQueryable<Product> query = adventureWorks.Products
                            .Where(getPredicateWithConstant(value.ToString()));
                        query.Load();
                    });
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();
                Enumerable.Range(0, 1000).ForEach(value =>
                    {
                        IQueryable<Product> query = adventureWorks.Products
                            .Where(getPredicateWithVariable(value.ToString()));
                        query.Load();
                    });
                stopwatch.Stop();
                Trace.WriteLine(stopwatch.ElapsedMilliseconds);
            }
        }

        internal static void UncachedSkipTake()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int skip = 1;
                int take = 1;
                IQueryable<ProductSubcategory> skipTakeWithVariable1 = adventureWorks.ProductSubcategories
                    .OrderBy(p => p.ProductSubcategoryID).Skip(skip).Take(take);
                skipTakeWithVariable1.Load();

                skip = 10;
                take = 10;
                IQueryable<ProductSubcategory> skipTakeWithVariable2 = adventureWorks.ProductSubcategories
                    .OrderBy(p => p.ProductSubcategoryID).Skip(skip).Take(take);
                skipTakeWithVariable2.Load();
            }
        }

        internal static void CachedSkipTake()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                int skip = 1;
                int take = 1;
                IQueryable<ProductSubcategory> skipTakeWithClosure1 = adventureWorks.ProductSubcategories
                    .OrderBy(p => p.ProductSubcategoryID).Skip(() => skip).Take(() => take);
                skipTakeWithClosure1.Load();

                skip = 10;
                take = 10;
                IQueryable<ProductSubcategory> skipTakeWithClosure2 = adventureWorks.ProductSubcategories
                    .OrderBy(p => p.ProductSubcategoryID).Skip(() => skip).Take(() => take);
                skipTakeWithClosure2.Load();
            }
        }
    }

    internal static partial class Performance
    {
        internal static async Task Async()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> categories = adventureWorks.ProductCategories;
                await categories.ForEachAsync( // Async version of foreach/ForEach.
                    category => Trace.WriteLine(category.Name));

                ProductSubcategory subcategory = await adventureWorks.ProductSubcategories
                    .FirstAsync(entity => entity.Name.Contains("Bike")); // Async version of First.
                Trace.WriteLine(subcategory.Name);

                Product[] products = await adventureWorks.Products
                    .Where(product => product.ListPrice <= 10)
                    .ToArrayAsync(); // Async version of ToArray.

                adventureWorks.Products.RemoveRange(products);
                await adventureWorks.SaveChangesAsync(); // Async version of SaveChanges.
            }
        }
    }

    public static partial class DbEntutyEntryExtensions
    {
        public static async Task<DbEntityEntry> RefreshAsync(this DbEntityEntry tracking, RefreshConflict refreshMode)
        {
            tracking.NotNull(nameof(tracking));

            switch (refreshMode)
            {
                case RefreshConflict.StoreWins:
                    {
                        await tracking.ReloadAsync();
                        break;
                    }
                case RefreshConflict.ClientWins:
                    {
                        DbPropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                        if (databaseValues == null)
                        {
                            tracking.State = EntityState.Detached;
                        }
                        else
                        {
                            tracking.OriginalValues.SetValues(databaseValues);
                        }
                        break;
                    }
                case RefreshConflict.MergeClinetAndStore:
                    {
                        DbPropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                        if (databaseValues == null)
                        {
                            tracking.State = EntityState.Detached;
                        }
                        else
                        {
                            DbPropertyValues originalValues = tracking.OriginalValues.Clone();
                            tracking.OriginalValues.SetValues(databaseValues);
                            databaseValues.PropertyNames
                                .Where(property => !object.Equals(originalValues[property], databaseValues[property]))
                                .ForEach(property => tracking.Property(property).IsModified = false);
                        }
                        break;
                    }
            }
            return tracking;
        }
    }

    public static partial class DbContextExtensions
    {
        public static async Task<int> SaveChangesAsync(
            this DbContext context, Func<IEnumerable<DbEntityEntry>, Task> resolveConflictsAsync, int retryCount = 3)
        {
            context.NotNull(nameof(context));
            Argument.Range(retryCount > 0, $"{retryCount} must be greater than 0.", nameof(retryCount));

            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    return await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException exception) when (retry < retryCount)
                {
                    await resolveConflictsAsync(exception.Entries);
                }
            }
            return await context.SaveChangesAsync();
        }

        public static async Task<int> SaveChangesAsync(
            this DbContext context, Func<IEnumerable<DbEntityEntry>, Task> resolveConflictsAsync, RetryStrategy retryStrategy)
        {
            context.NotNull(nameof(context));
            resolveConflictsAsync.NotNull(nameof(resolveConflictsAsync));
            retryStrategy.NotNull(nameof(retryStrategy));

            RetryPolicy retryPolicy = new RetryPolicy(
                new TransientDetection<DbUpdateConcurrencyException>(), retryStrategy);
            retryPolicy.Retrying += (sender, e) =>
                resolveConflictsAsync(((DbUpdateConcurrencyException)e.LastException).Entries).Wait();
            return await retryPolicy.ExecuteAsync(async () => await context.SaveChangesAsync());
        }
    }

    public static partial class DbContextExtensions
    {
        public static async Task<int> SaveChangesAsync(
            this DbContext context, RefreshConflict refreshMode, int retryCount = 3)
        {
            context.NotNull(nameof(context));
            Argument.Range(retryCount > 0, $"{retryCount} must be greater than 0.", nameof(retryCount));

            return await context.SaveChangesAsync(
                async conflicts =>
                {
                    foreach (DbEntityEntry tracking in conflicts)
                    {
                        await tracking.RefreshAsync(refreshMode);
                    }
                },
                retryCount);
        }

        public static async Task<int> SaveChangesAsync(
            this DbContext context, RefreshConflict refreshMode, RetryStrategy retryStrategy)
        {
            context.NotNull(nameof(context));
            retryStrategy.NotNull(nameof(retryStrategy));

            return await context.SaveChangesAsync(
                async conflicts =>
                {
                    foreach (DbEntityEntry tracking in conflicts)
                    {
                        await tracking.RefreshAsync(refreshMode);
                    }
                },
                retryStrategy);
        }
    }

    internal static partial class Performance
    {
        internal static async Task SaveChangesAsync()
        {
            using (AdventureWorks adventureWorks1 = new AdventureWorks())
            using (AdventureWorks adventureWorks2 = new AdventureWorks())
            {
                const int id = 950;
                Product productCopy1 = await adventureWorks1.Products.FindAsync(id);
                Product productCopy2 = await adventureWorks2.Products.FindAsync(id);

                productCopy1.Name = nameof(adventureWorks1);
                productCopy1.ListPrice = 100;
                await adventureWorks1.SaveChangesAsync();

                productCopy2.Name = nameof(adventureWorks2);
                productCopy2.ProductSubcategoryID = 1;
                await adventureWorks2.SaveChangesAsync(RefreshConflict.MergeClinetAndStore);
            }
        }

        internal static async Task DbContextTransactionAsync()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            using (DbContextTransaction transaction = adventureWorks.Database.BeginTransaction(
                IsolationLevel.ReadUncommitted))
            {
                try
                {
                    Trace.WriteLine(adventureWorks.QueryCurrentIsolationLevel()); // ReadUncommitted

                    ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                    adventureWorks.ProductCategories.Add(category);
                    Trace.WriteLine(await adventureWorks.SaveChangesAsync()); // 1

                    Trace.WriteLine(await adventureWorks.Database.ExecuteSqlCommandAsync(
                        "DELETE FROM [Production].[ProductCategory] WHERE [Name] = {0}",
                        nameof(ProductCategory))); // 1
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        internal static async Task DbTransactionAsync()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                await connection.OpenAsync();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (AdventureWorks adventureWorks = new AdventureWorks(connection))
                        {
                            adventureWorks.Database.UseTransaction(transaction);
                            Trace.WriteLine(adventureWorks.QueryCurrentIsolationLevel()); // Serializable

                            ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                            adventureWorks.ProductCategories.Add(category);
                            Trace.WriteLine(await adventureWorks.SaveChangesAsync()); // 1.
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            Trace.WriteLine(await command.ExecuteNonQueryAsync()); // 1
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

        internal static async Task TransactionScopeAsync()
        {
            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = DbContextExtensions.CurrentIsolationLevelSql;
                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();
                        Trace.WriteLine(reader[0]); // RepeatableRead
                    }
                }

                using (AdventureWorks adventureWorks = new AdventureWorks())
                {
                    ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                    adventureWorks.ProductCategories.Add(category);
                    Trace.WriteLine(await adventureWorks.SaveChangesAsync()); // 1
                }

                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@p0";
                    parameter.Value = nameof(ProductCategory);
                    command.Parameters.Add(parameter);

                    await connection.OpenAsync();
                    Trace.WriteLine(await command.ExecuteNonQueryAsync()); // 1
                }

                scope.Complete();
            }
        }
    }
}

#if DEMO
namespace System.Data.Entity
{
    using System.Linq;
    using System.Linq.Expressions;

    public static class QueryableExtensions
    {
        public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, Expression<Func<int>> countAccessor);

        public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, Expression<Func<int>> countAccessor);
    }
}
#endif
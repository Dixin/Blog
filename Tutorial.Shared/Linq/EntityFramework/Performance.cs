namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
#if EF
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
#endif
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
#if EF
    using System.Transactions;

    using Dixin.Linq.EntityFramework.Full;
    using Dixin.Linq.EntityFramework.FullWithViews;
#else

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage;
#endif

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using IsolationLevel = System.Data.IsolationLevel;

#if EF
    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
    using PropertyValues = System.Data.Entity.Infrastructure.DbPropertyValues;
    using IDbContextTransaction = System.Data.Entity.DbContextTransaction;
    using System.Threading;
#endif

#if EF
    public class LegacyAdventureWorks : ObjectContext
    {
        private ObjectSet<Product> products;

        public LegacyAdventureWorks()
            : base((EntityConnection)new AdventureWorks().ObjectContext().Connection)
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
#endif

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
                stopwatch.ElapsedMilliseconds.WriteLine(); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<ProductCategory> categories = Enumerable
                    .Range(0, 100).Select(index => new ProductCategory() { Name = index.ToString() });
                adventureWorks.ProductCategories.AddRange(categories);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
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
                stopwatch.ElapsedMilliseconds.WriteLine(); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Product[] products = adventureWorks.Products.ToArray();
                adventureWorks.Products.RemoveRange(products);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
            }
        }

#if EF
        internal static void MappingViews()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            using (FullAdventureWorks adventureWorks = new FullAdventureWorks())
            {
                adventureWorks.Production_ProductCategories.Load();
            }
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.WriteLine(); // 4786

            stopwatch.Restart();
            using (FullAdventureWorksWithViews adventureWorks = new FullAdventureWorksWithViews())
            {
                adventureWorks.Production_ProductCategories.Load();
            }
            stopwatch.Stop(); // 1340
            stopwatch.ElapsedMilliseconds.WriteLine();
        }
#endif

        internal static void CachedEntity(AdventureWorks adventureWorks)
        {
            ProductCategory category1 = adventureWorks.ProductCategories
                .Single(entity => entity.ProductCategoryID == 1);
            category1.Name = "Cache";

            ProductCategory category2 = adventureWorks.ProductCategories
                .Single(entity => entity.Name == "Bikes");
            category2.Name.WriteLine(); // Cache
            (category1 == category2).WriteLine(); // True

#if EF
            ProductCategory category3 = adventureWorks.ProductCategories
                .SqlQuery(@"
                        SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                        FROM [Production].[ProductCategory]
                        ORDER BY [ProductCategory].[ProductCategoryID]")
                .Single();
#else
            ProductCategory category3 = adventureWorks.ProductCategories
                .FromSql(@"
                    SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                    FROM [Production].[ProductCategory]
                    ORDER BY [ProductCategory].[ProductCategoryID]")
                .Single();
#endif
            (category1 == category3).WriteLine(); // True
        }

        internal static void UncachedEntity(AdventureWorks adventureWorks)
        {
            ProductCategory category1 = adventureWorks.ProductCategories
                .Single(entity => entity.ProductCategoryID == 1);
            category1.Name = "Cache";

            ProductCategory category2 = adventureWorks.ProductCategories
                .AsNoTracking().Single(entity => entity.Name == "Bikes");
            category2.Name.WriteLine(); // Bikes
            (category1 == category2).WriteLine(); // False

#if EF
            ProductCategory category3 = adventureWorks.Database
                .SqlQuery<ProductCategory>(@"
                        SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                        FROM [Production].[ProductCategory]
                        ORDER BY [ProductCategory].[ProductCategoryID]")
                .Single();
#else
            ProductCategory category3 = adventureWorks.ProductCategories
                .FromSql(@"
                    SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                    FROM [Production].[ProductCategory]
                    ORDER BY [ProductCategory].[ProductCategoryID]")
                .Single();
#endif
            (category1 == category3).WriteLine(); // False
        }

        internal static void Find(AdventureWorks adventureWorks)
        {
            Product[] products = adventureWorks.Products
                .Where(product => product.Name.StartsWith("Road")).ToArray(); // SELECT.
            Product fromCache = adventureWorks.Products.Find(999); // No database query.
            products.Contains(fromCache).WriteLine(); // True
        }

        internal static void TranslationCache(AdventureWorks adventureWorks)
        {
            int minLength = 1;
            IQueryable<ProductCategory> query = adventureWorks.ProductCategories
                .Where(category => category.Name.Length >= minLength)
                .Include(category => category.ProductSubcategories);
            query.Load();
        }

        internal static void UncachedTranslation(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> queryWithConstant1 = adventureWorks.ProductCategories
                .Where(category => category.Name.Length >= 1);
            queryWithConstant1.Load();

            IQueryable<ProductCategory> queryWithConstant2 = adventureWorks.ProductCategories
                .Where(category => category.Name.Length >= 10);
            queryWithConstant2.Load();
        }

        internal static void CachedTranslation(AdventureWorks adventureWorks)
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

        internal static void CompiledCachedTranslation(AdventureWorks adventureWorks)
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
                            method: startsWithMethod.GetMethodInfo(),
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
                stopwatch.ElapsedMilliseconds.WriteLine();

                stopwatch.Restart();
                Enumerable.Range(0, 1000).ForEach(value =>
                    {
                        IQueryable<Product> query = adventureWorks.Products
                            .Where(getPredicateWithVariable(value.ToString()));
                        query.Load();
                    });
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine();
            }
        }

        internal static void UncachedSkipTake(AdventureWorks adventureWorks)
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

#if EF
        internal static void CachedSkipTake(AdventureWorks adventureWorks)
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
#endif
    }

    internal static partial class Performance
    {
        internal static async Task Async(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> categories = adventureWorks.ProductCategories;
            await categories.ForEachAsync( // Async version of foreach/ForEach.
                category => category.Name.WriteLine());

            ProductSubcategory subcategory = await adventureWorks.ProductSubcategories
                .FirstAsync(entity => entity.Name.Contains("Bike")); // Async version of First.
            subcategory.Name.WriteLine();

            Product[] products = await adventureWorks.Products
                .Where(product => product.ListPrice <= 10)
                .ToArrayAsync(); // Async version of ToArray.

            adventureWorks.Products.RemoveRange(products);
            await adventureWorks.SaveChangesAsync(); // Async version of SaveChanges.
        }
    }

    public static partial class DbEntityEntryExtensions
    {
        public static async Task<EntityEntry> RefreshAsync(this EntityEntry tracking, RefreshConflict refreshMode)
        {
            switch (refreshMode)
            {
                case RefreshConflict.StoreWins:
                    {
                        await tracking.ReloadAsync();
                        break;
                    }
                case RefreshConflict.ClientWins:
                    {
                        PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
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
                case RefreshConflict.MergeClientAndStore:
                    {
                        PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                        if (databaseValues == null)
                        {
                            tracking.State = EntityState.Detached;
                        }
                        else
                        {
                            PropertyValues originalValues = tracking.OriginalValues.Clone();
                            tracking.OriginalValues.SetValues(databaseValues);
#if EF
                            databaseValues.PropertyNames
                                .Where(property => !object.Equals(originalValues[property], databaseValues[property]))
                                .ForEach(property => tracking.Property(property).IsModified = false);
#else
                            databaseValues.Properties
                                .Where(property => !object.Equals(originalValues[property.Name], databaseValues[property.Name]))
                                .ForEach(property => tracking.Property(property.Name).IsModified = false);
#endif
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
            this DbContext context, Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), $"{retryCount} must be greater than 0.");
            }

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
            this DbContext context, Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryStrategy retryStrategy)
        {
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
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), $"{retryCount} must be greater than 0.");
            }

            return await context.SaveChangesAsync(
                async conflicts =>
                {
                    foreach (EntityEntry tracking in conflicts)
                    {
                        await tracking.RefreshAsync(refreshMode);
                    }
                },
                retryCount);
        }

        public static async Task<int> SaveChangesAsync(
            this DbContext context, RefreshConflict refreshMode, Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryStrategy retryStrategy) =>
                await context.SaveChangesAsync(
                    async conflicts =>
                        {
                            foreach (EntityEntry tracking in conflicts)
                            {
                                await tracking.RefreshAsync(refreshMode);
                            }
                        },
                    retryStrategy);
    }

    internal static partial class Performance
    {
        internal static async Task SaveChangesAsync()
        {
            using (AdventureWorks adventureWorks1 = new AdventureWorks())
            using (AdventureWorks adventureWorks2 = new AdventureWorks())
            {
                const int id = 999;
                Product productCopy1 = await adventureWorks1.Products.FindAsync(id);
                Product productCopy2 = await adventureWorks2.Products.FindAsync(id);

                productCopy1.Name = nameof(adventureWorks1);
                productCopy1.ListPrice = 100;
                await adventureWorks1.SaveChangesAsync();

                productCopy2.Name = nameof(adventureWorks2);
                productCopy2.ProductSubcategoryID = 1;
                await adventureWorks2.SaveChangesAsync(RefreshConflict.MergeClientAndStore);
            }
        }

#if !EF
        internal static async Task DbContextTransactionAsync(AdventureWorks adventureWorks)
        {
            await adventureWorks.Database.CreateExecutionStrategy().ExecuteAsync(async () => 
            {
                using (IDbContextTransaction transaction = await adventureWorks.Database.BeginTransactionAsync(
                IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        adventureWorks.CurrentIsolationLevel().WriteLine(); // ReadUncommitted

                        ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                        adventureWorks.ProductCategories.Add(category);
                        adventureWorks.SaveChanges().WriteLine(); // 1

                        adventureWorks.Database.ExecuteSqlCommand(
                            sql: "DELETE FROM [Production].[ProductCategory] WHERE [Name] = {0}",
                            parameters: nameof(ProductCategory)).WriteLine(); // 1
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }
#endif

//#if !EF
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
                            await adventureWorks.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                            {
                                adventureWorks.Database.UseTransaction(transaction);
                                adventureWorks.CurrentIsolationLevel().WriteLine(); // Serializable

                                ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                                adventureWorks.ProductCategories.Add(category);
                                (await adventureWorks.SaveChangesAsync()).WriteLine(); // 1.
                            });
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            (await command.ExecuteNonQueryAsync()).WriteLine(); // 1
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
//#endif

#if EF
        internal static async Task TransactionScopeAsync()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                await adventureWorks.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
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
                                reader[0].WriteLine(); // RepeatableRead
                            }
                        }

                        ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                        adventureWorks.ProductCategories.Add(category);
                        await adventureWorks.SaveChangesAsync().WriteLine(); // 1

                        using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);

                            await connection.OpenAsync();
                            (await command.ExecuteNonQueryAsync()).WriteLine(); // 1
                        }

                        scope.Complete();
                    }
                });
            }
        }
#endif
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
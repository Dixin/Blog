namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
#if NETFX
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
#if NETFX
    using System.Transactions;

    using Dixin.Linq.EntityFramework.Full;
    using Dixin.Linq.EntityFramework.FullWithViews;
#else

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
#endif

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using IsolationLevel = System.Data.IsolationLevel;

#if NETFX
    using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
    using PropertyValues = System.Data.Entity.Infrastructure.DbPropertyValues;
    using IDbContextTransaction = System.Data.Entity.DbContextTransaction;
#endif

#if NETFX
    public class LegacyAdventureWorks : ObjectContext
    {
        private ObjectSet<StockItem> products;

        public LegacyAdventureWorks()
            : base((EntityConnection)new WideWorldImporters().ObjectContext().Connection)
        {
        }

        public ObjectSet<StockItem> StockItems => this.products ?? (this.products = this.CreateObjectSet<StockItem>());
    }

    internal static class CompiledQueries
    {
        private static readonly Func<LegacyAdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((LegacyAdventureWorks adventureWorks, decimal listPrice) => adventureWorks
                .StockItems
                .Where(product => product.UnitPrice == listPrice)
                .Select(product => product.StockItemName));

        internal static IQueryable<string> GetProductNames
            (this LegacyAdventureWorks adventureWorks, decimal listPrice) =>
                GetProductNamesCompiled(adventureWorks, listPrice);
    }
#endif

    internal static partial class Performance
    {
        internal static void AddRange()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                adventureWorks.SupplierCategories.Load(); // Warm up.
            }
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<SupplierCategory> categories = Enumerable
                    .Range(-100, 100).Select(tempId => new SupplierCategory() { SupplierCategoryID = tempId, SupplierCategoryName = tempId.ToString() });
                DbSet<SupplierCategory> repository = adventureWorks.SupplierCategories;
                foreach (SupplierCategory category in categories)
                {
                    repository.Add(category);
                }
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 48
            }
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<SupplierCategory> categories = Enumerable
                    .Range(-100, 100).Select(tempId => new SupplierCategory() { SupplierCategoryID = tempId, SupplierCategoryName = tempId.ToString() });
                adventureWorks.SupplierCategories.AddRange(categories);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
            }
        }

        internal static void RemoveRange()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                adventureWorks.StockItems.Load(); // Warm up.
            }
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                StockItem[] products = adventureWorks.StockItems.ToArray();
                DbSet<StockItem> repository = adventureWorks.StockItems;
                foreach (StockItem product in products)
                {
                    repository.Remove(product);
                }
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 1682
            }
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                StockItem[] products = adventureWorks.StockItems.ToArray();
                adventureWorks.StockItems.RemoveRange(products);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
            }
        }

#if NETFX
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

        internal static void CachedEntity(WideWorldImporters adventureWorks)
        {
            SupplierCategory category1 = adventureWorks.SupplierCategories
                .Single(entity => entity.SupplierCategoryID == 1);
            category1.SupplierCategoryName = "Cache";

            SupplierCategory category2 = adventureWorks.SupplierCategories
                .Single(entity => entity.SupplierCategoryName == "Other Wholesaler");
            category2.SupplierCategoryName.WriteLine(); // Cache
            (category1 == category2).WriteLine(); // True

#if NETFX
            SupplierCategory category3 = adventureWorks.SupplierCategories
                .SqlQuery(@"
                    SELECT TOP (1) [SupplierCategories].[SupplierCategoryID], [SupplierCategories].[SupplierCategoryName], [SupplierCategories].[LastEditedBy]
                    FROM [Purchasing].[SupplierCategories]
                    ORDER BY [SupplierCategories].[SupplierCategoryID]")
                .Single();
#else
            SupplierCategory category3 = adventureWorks.SupplierCategories
                .FromSql(@"
                    SELECT TOP (1) [SupplierCategories].[SupplierCategoryID], [SupplierCategories].[SupplierCategoryName], [SupplierCategories].[LastEditedBy]
                    FROM [Purchasing].[SupplierCategories]
                    ORDER BY [SupplierCategories].[SupplierCategoryID]")
                .Single();
#endif
            (category1 == category3).WriteLine(); // True
        }

        internal static void UncachedEntity(WideWorldImporters adventureWorks)
        {
            SupplierCategory category1 = adventureWorks.SupplierCategories
                .Single(entity => entity.SupplierCategoryID == 1);
            category1.SupplierCategoryName = "Cache";

            SupplierCategory category2 = adventureWorks.SupplierCategories
                .AsNoTracking().Single(entity => entity.SupplierCategoryName == "Other Wholesaler");
            category2.SupplierCategoryName.WriteLine(); // Other Wholesaler
            (category1 == category2).WriteLine(); // False

#if NETFX
            SupplierCategory category3 = adventureWorks.Database
                .SqlQuery<SupplierCategory>(@"
                    SELECT TOP (1) [SupplierCategories].[SupplierCategoryID], [SupplierCategories].[SupplierCategoryName], [SupplierCategories].[LastEditedBy]
                    FROM [Purchasing].[SupplierCategories]
                    ORDER BY [SupplierCategories].[SupplierCategoryID]")
                .Single();
#else
            SupplierCategory category3 = adventureWorks.SupplierCategories
                .FromSql(@"
                    SELECT TOP (1) [SupplierCategories].[SupplierCategoryID], [SupplierCategories].[SupplierCategoryName], [SupplierCategories].[LastEditedBy]
                    FROM [Purchasing].[SupplierCategories]
                    ORDER BY [SupplierCategories].[SupplierCategoryID]")
                .Single();
#endif
            (category1 == category3).WriteLine(); // False
        }

        internal static void Find(WideWorldImporters adventureWorks)
        {
            StockItem[] products = adventureWorks.StockItems
                .Where(product => product.StockItemName.StartsWith("Road")).ToArray(); // SELECT.
            StockItem fromCache = adventureWorks.StockItems.Find(999); // No database query.
            products.Contains(fromCache).WriteLine(); // True
        }

        internal static void TranslationCache(WideWorldImporters adventureWorks)
        {
            int minLength = 1;
            IQueryable<SupplierCategory> query = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= minLength)
                .Include(category => category.Suppliers);
            query.Load();
        }

        internal static void UncachedTranslation(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> queryWithConstant1 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= 1);
            queryWithConstant1.Load();

            IQueryable<SupplierCategory> queryWithConstant2 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= 10);
            queryWithConstant2.Load();
        }

        internal static void CachedTranslation(WideWorldImporters adventureWorks)
        {
            int minLength = 1;
            IQueryable<SupplierCategory> queryWithClosure1 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= minLength);
            queryWithClosure1.Load();

            minLength = 10;
            IQueryable<SupplierCategory> queryWithClosure2 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= minLength);
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

        internal static void CompiledCachedTranslation(WideWorldImporters adventureWorks)
        {
            int minLength = 1;
            DisplayClass1 displayClass1 = new DisplayClass1() { minLength = minLength };
            IQueryable<SupplierCategory> queryWithClosure1 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= displayClass1.minLength);
            queryWithClosure1.Load();

            minLength = 10;
            DisplayClass1 displayClass2 = new DisplayClass1() { minLength = minLength };
            IQueryable<SupplierCategory> queryWithClosure2 = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryName.Length >= displayClass2.minLength);
            queryWithClosure2.Load();
        }

        internal static void Translation()
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                adventureWorks.StockItems.Load(); // Warm up.
            }

            Func<string, Expression<Func<StockItem, bool>>> getPredicateWithConstant = startWith =>
                {
                    ParameterExpression productParameterExpression = Expression.Parameter(typeof(StockItem), "product");
                    Func<string, bool> startsWithMethod = string.Empty.StartsWith;
                    Expression<Func<StockItem, bool>> predicateExpression = Expression.Lambda<Func<StockItem, bool>>(
                        Expression.Call(
                            instance: Expression.Property(productParameterExpression, nameof(StockItem.StockItemName)),
                            method: startsWithMethod.GetMethodInfo(),
                            arguments: Expression.Constant(startWith, typeof(string))),
                        productParameterExpression);
                    return predicateExpression;
                };

            Func<string, Expression<Func<StockItem, bool>>> getPredicateWithVariable =
                startWith => product => product.StockItemName.StartsWith(startWith);

            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Enumerable.Range(0, 1000).ForEach(value =>
                    {
                        IQueryable<StockItem> query = adventureWorks.StockItems
                            .Where(getPredicateWithConstant(value.ToString()));
                        query.Load();
                    });
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine();

                stopwatch.Restart();
                Enumerable.Range(0, 1000).ForEach(value =>
                    {
                        IQueryable<StockItem> query = adventureWorks.StockItems
                            .Where(getPredicateWithVariable(value.ToString()));
                        query.Load();
                    });
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine();
            }
        }

        internal static void UncachedSkipTake(WideWorldImporters adventureWorks)
        {
            int skip = 1;
            int take = 1;
            IQueryable<Supplier> skipTakeWithVariable1 = adventureWorks.Suppliers
                .OrderBy(p => p.SupplierID).Skip(skip).Take(take);
            skipTakeWithVariable1.Load();

            skip = 10;
            take = 10;
            IQueryable<Supplier> skipTakeWithVariable2 = adventureWorks.Suppliers
                .OrderBy(p => p.SupplierID).Skip(skip).Take(take);
            skipTakeWithVariable2.Load();
        }

#if NETFX
        internal static void CachedSkipTake(WideWorldImporters adventureWorks)
        {
            int skip = 1;
            int take = 1;
            IQueryable<Supplier> skipTakeWithClosure1 = adventureWorks.Suppliers
                .OrderBy(p => p.SupplierID).Skip(() => skip).Take(() => take);
            skipTakeWithClosure1.Load();

            skip = 10;
            take = 10;
            IQueryable<Supplier> skipTakeWithClosure2 = adventureWorks.Suppliers
                .OrderBy(p => p.SupplierID).Skip(() => skip).Take(() => take);
            skipTakeWithClosure2.Load();
        }
#endif
    }

    internal static partial class Performance
    {
        internal static async Task Async(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> categories = adventureWorks.SupplierCategories;
            await categories.ForEachAsync( // Async version of foreach/ForEach.
                category => category.SupplierCategoryName.WriteLine());

            StockItem[] products = await adventureWorks.StockItems
                .Where(product => product.UnitPrice <= 10)
                .ToArrayAsync(); // Async version of ToArray.

            Supplier subcategory = await adventureWorks.Suppliers
                .FirstAsync(entity => entity.SupplierName.Contains("A Datum Corporation")); // Async version of First.
            subcategory.SupplierName.WriteLine();

            subcategory.SupplierName = nameof(Supplier);
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
#if NETFX
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
            this DbContext context, Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, RetryStrategy retryStrategy)
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
            this DbContext context, RefreshConflict refreshMode, RetryStrategy retryStrategy) =>
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
        internal static async Task SaveChangesAsync(WideWorldImporters adventureWorks1, WideWorldImporters adventureWorks2)
        {
            const int id = 1;
            StockItem productCopy1 = await adventureWorks1.StockItems.FindAsync(id);
            StockItem productCopy2 = await adventureWorks2.StockItems.FindAsync(id);

            productCopy1.StockItemName = nameof(adventureWorks1);
            productCopy1.UnitPrice = 100;
            await adventureWorks1.SaveChangesAsync();

            productCopy2.StockItemName = nameof(adventureWorks2);
            productCopy2.SupplierID = 1;
            await adventureWorks2.SaveChangesAsync(RefreshConflict.MergeClientAndStore);
        }

#if !NETFX
        internal static async Task DbContextTransactionAsync(WideWorldImporters adventureWorks)
        {
            await adventureWorks.Database.CreateExecutionStrategy().Execute(async () => 
            {
                using (IDbContextTransaction transaction = await adventureWorks.Database.BeginTransactionAsync(
                IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        adventureWorks.QueryCurrentIsolationLevel().WriteLine(); // ReadUncommitted

                        SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                        adventureWorks.SupplierCategories.Add(category);
                        adventureWorks.SaveChanges().WriteLine(); // 1

                        adventureWorks.Database.ExecuteSqlCommand(
                            sql: "DELETE FROM [Purchasing].[SupplierCategories] WHERE [SupplierCategoryName] = {0}",
                            parameters: nameof(SupplierCategory)).WriteLine(); // 1
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

#if !NETFX
        internal static async Task DbTransactionAsync()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                await connection.OpenAsync();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (WideWorldImporters adventureWorks = new WideWorldImporters(connection))
                        {
                            await adventureWorks.Database.CreateExecutionStrategy().Execute(async () =>
                            {
                                adventureWorks.Database.UseTransaction(transaction);
                                adventureWorks.QueryCurrentIsolationLevel().WriteLine(); // Serializable

                                SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                                adventureWorks.SupplierCategories.Add(category);
                                (await adventureWorks.SaveChangesAsync()).WriteLine(); // 1.
                            });
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Purchasing].[SupplierCategories] WHERE [SupplierCategoryName] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(SupplierCategory);
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
#endif

#if NETFX
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
                        reader[0].WriteLine(); // RepeatableRead
                    }
                }

                using (WideWorldImporters adventureWorks = new WideWorldImporters())
                {
                    SupplierCategory category = new SupplierCategory() { SupplierCategoryName = nameof(SupplierCategory) };
                    adventureWorks.SupplierCategories.Add(category);
                    await adventureWorks.SaveChangesAsync().WriteLine(); // 1
                }

                using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@p0";
                    parameter.Value = nameof(SupplierCategory);
                    command.Parameters.Add(parameter);

                    await connection.OpenAsync();
                    (await command.ExecuteNonQueryAsync()).WriteLine(); // 1
                }

                scope.Complete();
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
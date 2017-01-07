namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
#if NETFX
    using System.Data.Entity;
    using System.Data.Entity.Core.Common.CommandTrees;
#endif
    using System.Linq;

#if NETFX
    using Dixin.Linq;
#endif

#if !NETFX
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Storage;
#endif

    public static class QueryableExtensions
    {
#if NETFX
        public static IEnumerator<TEntity> GetIterator<TEntity>(
            this IQueryable<TEntity> query, DbContext dbContext)
        {
            IEnumerator<TEntity> sqlReader = null;
            bool isSqlExecuted = false;
            return new Iterator<TEntity>(
                state: IteratorState.Start,
                start: () =>
                    {
                        "|_Convert expression tree to database command tree.".WriteLine();
                        DbQueryCommandTree commandTree = dbContext.Compile(query.Expression);
                        "|_Generate SQL from database command tree.".WriteLine();
                        DbCommand sql = dbContext.Generate(commandTree);
                        "|_Build SQL query.".WriteLine();
                        IEnumerable<TEntity> sqlQuery = dbContext.Database.SqlQuery<TEntity>(
                            sql.CommandText,
                            sql.Parameters.Cast<DbParameter>().Select(parameter => parameter.Value).ToArray());
                        sqlReader = sqlQuery.GetEnumerator();
                    },
                moveNext: () =>
                    {
                        if (!isSqlExecuted)
                        {
                            "|_Execute SQL query.".WriteLine();
                            isSqlExecuted = true;
                        }
                        $"|_Read a row and materialize to {typeof(TEntity).Name} entity.".WriteLine();
                        return sqlReader.MoveNext();
                    },
                getCurrent: () => sqlReader.Current,
                dispose: () => sqlReader.Dispose());
        }
#endif

#if !NETFX
        public static IEnumerator<TEntity> GetIterator<TEntity>(
            this IQueryable<TEntity> query, DbContext dbContext) where TEntity : class
        {
            IReadOnlyDictionary<string, object> parameters = null;
            IRelationalCommand command = null;
            RelationalDataReader reader = null;
            Func<DbDataReader, TEntity> materializer = null;
            return new Iterator<TEntity>(
                state: IteratorState.Start,
                start: () =>
                {
                    "|_Convert expression tree to database command tree.".WriteLine();
                    (SelectExpression Expression, IReadOnlyDictionary<string, object> Parameters) result = dbContext.Compile(query.Expression);
                    SelectExpression selectExpression = result.Expression;
                    parameters = result.Parameters;
                    "|_Generate SQL from database command tree.".WriteLine();
                    command = dbContext.Generate(selectExpression, parameters);
                    "|_Build SQL query.".WriteLine();
                    materializer = dbContext.GetMaterializer<TEntity>(selectExpression, parameters);
                },
                moveNext: () =>
                {
                    if (reader == null)
                    {
                        "|_Execute SQL query.".WriteLine();
                        IRelationalConnection connection = dbContext.GetService<IRelationalConnection>();
                        reader = command.ExecuteReader(connection, parameters);
                    }
                    $"|_Read a row and materialize to {typeof(TEntity).Name} entity.".WriteLine();
                    return reader.DbDataReader.Read();
                },
                getCurrent: () => materializer(reader.DbDataReader),
                dispose: () => reader.Dispose());
        }
#endif
    }

    internal static partial class Laziness
    {
        internal static void WhereAndSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> categories = adventureWorks.SupplierCategories
                .Where(category => category.SupplierCategoryID > 1);
            // categories.WriteLines(category => category.Name));
            "Iterator - Create from LINQ to Entities query.".WriteLine();
            using (IEnumerator<SupplierCategory> iterator = categories
                .GetIterator(adventureWorks)) // products.GetEnumerator()
            {
                while (new Func<bool>(() =>
                    {
                        "Iterator - Movie to next.".WriteLine();
                        return iterator.MoveNext(); // Translate and execute query.
                    })())
                {
                    SupplierCategory category = iterator.Current;
                    $"Iterator - Get current.: {category.SupplierCategoryName}.".WriteLine();
                }
            }
        }
    }

    internal static partial class Laziness
    {
        internal static void ImplicitLazyLoading(WideWorldImporters adventureWorks)
        {
            Supplier subcategory = adventureWorks.Suppliers.First(); // Database query.
            subcategory.SupplierName.WriteLine();
            SupplierCategory associatedCategory = subcategory.SupplierCategory; // Database query.
            associatedCategory.SupplierCategoryName.WriteLine();
            ICollection<StockItem> associatedProducts = subcategory.StockItems; // Database query.
            associatedProducts.Count.WriteLine();
        }

        internal static void ExplicitLazyLoading(WideWorldImporters adventureWorks)
        {
            Supplier subcategory = adventureWorks.Suppliers.First(); // Database query.
            subcategory.SupplierName.WriteLine();
            adventureWorks
                .Entry(subcategory) // Return DbEntityEntry<ProductSubcategory>.
                .Reference(entity => entity.SupplierCategory) // Return DbReferenceEntry<ProductSubcategory, ProductCategory>.
                .Load(); // Database query.
            subcategory.SupplierCategory.SupplierCategoryName.WriteLine();
            adventureWorks
                .Entry(subcategory) // Return DbEntityEntry<ProductSubcategory>.
                .Collection(entity => entity.StockItems) // Return DbCollectionEntry<ProductSubcategory, Product>.
                .Load(); // Database query.
            subcategory.StockItems.Count.WriteLine();
        }

        internal static void ExplicitLazyLoadingWithQuery(WideWorldImporters adventureWorks)
        {
            Supplier subcategory = adventureWorks.Suppliers.First(); // Database query.
            subcategory.SupplierName.WriteLine();
            string associatedCategoryName = adventureWorks
                .Entry(subcategory).Reference(entity => entity.SupplierCategory)
                .Query() // Return IQueryable<ProductCategory>.
                .Select(category => category.SupplierCategoryName).Single(); // Database query.
            associatedCategoryName.WriteLine();
            int associatedProductsCount = adventureWorks
                .Entry(subcategory).Collection(entity => entity.StockItems)
                .Query() // Return IQueryable<Product>.
                .Count(); // Database query.
            associatedProductsCount.WriteLine();
        }

        internal static void LazyLoadingAndDeferredExecution(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> subcategories = adventureWorks.Suppliers;
            subcategories
                .ForEach(subcategory =>  // Reading subcategories is in progress.
                    $"{subcategory.SupplierCategory.SupplierCategoryName}/{subcategory.SupplierName}: {subcategory.StockItems.Count}".WriteLine());
            // EntityCommandExecutionException: There is already an open DataReader associated with this Command which must be closed first.
        }

        internal static void LazyLoadingAndImmediateExecution(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> subcategories = adventureWorks.Suppliers;
            subcategories
                .ToArray() // Finish reading subcategories.
                .ForEach(subcategory =>
                    $"{subcategory.SupplierCategory/* Finish reading category. */.SupplierCategoryName}/{subcategory.SupplierName}: {subcategory.StockItems/* Finish reading products. */.Count}".WriteLine());
        }

        internal static void EagerLoadingWithInclude(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> subcategories = adventureWorks.Suppliers
                .Include(subcategory => subcategory.SupplierCategory)
                .Include(subcategory => subcategory.StockItems);
            subcategories.WriteLines(subcategory =>
                $"{subcategory.SupplierCategory.SupplierCategoryName}/{subcategory.SupplierName}: {subcategory.StockItems.Count}");
        }

        internal static void EagerLoadingWithIncludeAndSelect(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> categories = adventureWorks.SupplierCategories
                .Include(category => category.Suppliers.Select(subcategory => subcategory.StockItems));
            categories.WriteLines(category =>
                $@"{category.SupplierCategoryName}: {string.Join(", ", category.Suppliers
                    .Select(subcategory => $"{subcategory.SupplierName}-{subcategory.StockItems.Count}"))}");
        }

        internal static void EagerLoadingWithSelect(WideWorldImporters adventureWorks)
        {
            var subcategories = adventureWorks.Suppliers.Select(subcategory => new
            {
                Name = subcategory.SupplierName,
                CategoryName = subcategory.SupplierCategory.SupplierCategoryName,
                ProductCount = subcategory.StockItems.Count
            });
            subcategories.WriteLines(subcategory =>
                $"{subcategory.CategoryName}/{subcategory.Name}: {subcategory.ProductCount}");
        }

        internal static void PrintSubcategoriesWithLazyLoading(WideWorldImporters adventureWorks)
        {
            Supplier[] subcategories = adventureWorks.Suppliers
                .GroupBy(subcategory => subcategory.SupplierCategoryID, (key, group) => group.FirstOrDefault())
                .ToArray(); // 1 query for N subcategories.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.SupplierName} ({subcategory.SupplierCategory.SupplierCategoryName})"); // N queries.
        }

        internal static void PrintSubcategoriesWithEagerLoading(WideWorldImporters adventureWorks)
        {
            Supplier[] subcategories = adventureWorks.Suppliers
                .GroupBy(subcategory => subcategory.SupplierCategoryID, (key, group) => group.FirstOrDefault())
                .Include(subcategory => subcategory.SupplierCategory)
                .ToArray(); // 1 query for N subcategories.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.SupplierName} ({subcategory.SupplierCategory.SupplierCategoryName})"); // N queries.
        }

        internal static void ConditionalEagerLoadingWithInclude(WideWorldImporters adventureWorks)
        {
            IQueryable<Supplier> subcategories = adventureWorks.Suppliers
                .Include(subcategory => subcategory.StockItems.Where(product => product.UnitPrice > 0));
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.SupplierName}: {string.Join(
                    ", ", subcategory.StockItems.Select(product => product.StockItemName))}");
            // ArgumentException: The Include path expression must refer to a navigation property defined on the type. Use dotted paths for reference navigation properties and the Select operator for collection navigation properties.
        }

        internal static void ConditionalEagerLoadingWithSelect(WideWorldImporters adventureWorks)
        {
            var subcategories = adventureWorks.Suppliers.Select(subcategory => new
            {
                Subcategory = subcategory,
                Products = subcategory.StockItems.Where(product => product.UnitPrice > 0)
            });
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.Subcategory.SupplierName}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.StockItemName))}");
        }

#if NETFX
        internal static void DisableLazyLoading(WideWorldImporters adventureWorks)
        {
            adventureWorks.Configuration.LazyLoadingEnabled = false;
            Supplier subcategory = adventureWorks.Suppliers.First(); // Database query.
            subcategory.SupplierName.WriteLine();
            SupplierCategory associatedCategory = subcategory.SupplierCategory; // No database query.
            (associatedCategory == null).WriteLine(); // True
            ICollection<StockItem> associatedProducts = subcategory.StockItems; // No database query.
            associatedProducts.Count.WriteLine(); // 0
        }

        internal static void DisableProxy(WideWorldImporters adventureWorks)
        {
            adventureWorks.Configuration.ProxyCreationEnabled = false;
            Supplier subcategory = adventureWorks.Suppliers.First(); // Database query.
            subcategory.SupplierName.WriteLine();
            SupplierCategory associatedCategory = subcategory.SupplierCategory; // No database query.
            (associatedCategory == null).WriteLine(); // True
            ICollection<StockItem> associatedProducts = subcategory.StockItems; // No database query.
            associatedProducts.Count.WriteLine(); // 0
        }
#endif
    }

    internal static class DataAccess
    {
        internal static IQueryable<StockItem> QueryCategoryProducts(string category)
        {
            using (WideWorldImporters adventureWorks = new WideWorldImporters())
            {
                return adventureWorks.StockItems.Where(
                    product => product.Supplier.SupplierCategory.SupplierCategoryName == category);
            }
        }
    }

    internal static class UI
    {
        internal static void RenderCategoryProducts(string category) => DataAccess
            .QueryCategoryProducts(category)
            .Select(product => product.StockItemName)
            .WriteLines();
        // InvalidOperationException: The operation cannot be completed because the DbContext has been disposed.
    }
}

#if DEMO
namespace Dixin.Linq.EntityFramework
{
    public partial class AdventureWorks
    {
        public AdventureWorks()
            : base(ConnectionStrings.AdventureWorks)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;

    public class DbContext
    {
        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class DbEntityEntry<TEntity> where TEntity : class
    {
        public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(
            Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class;

        public DbCollectionEntry<TEntity, TElement> Collection<TElement>(
            Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class;

        // Other members.
    }
}
#endif

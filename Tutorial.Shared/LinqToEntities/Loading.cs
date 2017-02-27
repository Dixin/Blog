namespace Tutorial.LinqToEntities
{
#if EF
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.SqlClient;
    using System.Linq;

    using static Tutorial.LinqToObjects.EnumerableX;
#else
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Storage;
#endif

    public static class QueryableExtensions
    {
#if EF
        public static IEnumerator<TEntity> GetEntityIterator<TEntity>(
            this IQueryable<TEntity> query, DbContext dbContext)
        {
            IEnumerator<TEntity> entityIterator = null;
            return new Iterator<TEntity>(
                start: () =>
                {
                    "| |_Compile LINQ expression tree to database expression tree.".WriteLine();
                    DbQueryCommandTree compilation = dbContext.Compile(query.Expression);
                    "|_Generate SQL from database expression tree.".WriteLine();
                    DbCommand sql = dbContext.Generate(compilation);
                    IEnumerable<TEntity> sqlQuery = dbContext.Database.SqlQuery<TEntity>(
                        sql: sql.CommandText,
                        parameters: sql.Parameters.Cast<SqlParameter>().Select(parameter => ((ICloneable)parameter).Clone()).ToArray());
                    entityIterator = sqlQuery.GetEnumerator();
                    "| |_Execute generated SQL.".WriteLine();
                },
                moveNext: () => entityIterator.MoveNext(),
                getCurrent: () =>
                {
                    $"| |_Materialize data row to {typeof(TEntity).Name} entity.".WriteLine();
                    return entityIterator.Current;
                },
                dispose: () => entityIterator.Dispose(),
                end: () => "  |_End.".WriteLine()).Start();
        }
#else
        public static IEnumerator<TEntity> GetEntityIterator<TEntity>(
            this IQueryable<TEntity> query, DbContext dbContext) where TEntity : class
        {
            "| |_Compile LINQ expression tree to database expression tree.".WriteLine();
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) compilation =
                dbContext.Compile(query.Expression);

            IEnumerator<TEntity> entityIterator = null;
            return new Iterator<TEntity>(
                start: () =>
                {
                    "| |_Generate SQL from database expression tree.".WriteLine();
                    IRelationalCommand sql = dbContext.Generate(
                        compilation.DatabaseExpression, compilation.Parameters);
                    IEnumerable<TEntity> sqlQuery = dbContext.Set<TEntity>().FromSql(
                        sql: sql.CommandText,
                        parameters: compilation.Parameters
                            .Select(parameter => new SqlParameter(parameter.Key, parameter.Value)).ToArray());
                    entityIterator = sqlQuery.GetEnumerator();
                    "| |_Execute generated SQL.".WriteLine();
                },
                moveNext: () => entityIterator.MoveNext(),
                getCurrent: () =>
                {
                    $"| |_Materialize data row to {typeof(TEntity).Name} entity.".WriteLine();
                    return entityIterator.Current;
                },
                dispose: () => entityIterator.Dispose(),
                end: () => "  |_End.".WriteLine()).Start();
        }
#endif
    }

    internal static partial class Loading
    {
        internal static void DeferredExecution(AdventureWorks adventureWorks)
        {
            IQueryable<Product> categories = adventureWorks.Products
                .Where(product => product.Name.Length > 100)
                .Take(3);
            "Iterator - Create from LINQ to Entities query.".WriteLine();
            using (IEnumerator<Product> iterator = categories.GetEntityIterator(adventureWorks))
            {
                int index = 0;
                while (new Func<bool>(() =>
                    {
                        $"|_Iterator - [{++index}] Move next.".WriteLine();
                        return iterator.MoveNext(); // Translate and execute query.
                    })())
                {
                    Product product = iterator.Current;
                    $"| |_Iterator - [{index}] Get current: {product.Name}.".WriteLine();
                }
            }
            // Iterator - Create from LINQ to Entities query.
            // | |_Compile LINQ expression tree to database expression tree.
            // |_Iterator - [1] Move next.
            // | |_Generate SQL from database expression tree.
            // | |_Execute generated SQL.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [1] Get current: ML Crankset.
            // |_Iterator - [2] Move next.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [2] Get current: HL Crankset.
            // |_Iterator - [3] Move next.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [3] Get current: Touring-2000 Blue, 60.
            // |_Iterator - [4] Move next.
            //   |_End.
        }
    }

    internal static partial class Loading
    {
        internal static void ExplicitLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]
            subcategory.Name.WriteLine();

            adventureWorks
                .Entry(subcategory) // Return EntityEntry<ProductSubcategory>.
                .Reference(entity => entity.ProductCategory) // Return ReferenceEntry<ProductSubcategory, ProductCategory>.
                .Load(); // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductCategoryID], [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            subcategory.ProductCategory.Name.WriteLine();

            adventureWorks
                .Entry(subcategory) // Return EntityEntry<ProductSubcategory>.
                .Collection(entity => entity.Products) // Return CollectionEntry<ProductSubcategory, Product>.
                .Load(); // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductID], [e].[ListPrice], [e].[Name], [e].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [e]
            // WHERE [e].[ProductSubcategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            subcategory.Products.WriteLines(product => product.Name);
        }

        internal static void ExplicitLoadingWithQuery(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]
            subcategory.Name.WriteLine();
            string categoryName = adventureWorks
                .Entry(subcategory).Reference(entity => entity.ProductCategory)
                .Query() // Return IQueryable<ProductCategory>.
                .Select(category => category.Name).Single(); // Execute query.
            // exec sp_executesql N'SELECT TOP(2) [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            categoryName.WriteLine();

            IQueryable<string> products = adventureWorks
                .Entry(subcategory).Collection(entity => entity.Products)
                .Query() // Return IQueryable<Product>.
                .Select(product => product.Name); // Execute query.
            // exec sp_executesql N'SELECT [e].[Name]
            // FROM [Production].[Product] AS [e]
            // WHERE [e].[ProductSubcategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            products.WriteLines();
        }

        internal static void EagerLoadingWithInclude(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategoriesWithCategory = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.ProductCategory);
            subcategoriesWithCategory.WriteLines(subcategory =>
                $"{subcategory.ProductCategory.Name}: {subcategory.Name}");
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID], [p].[ProductCategoryID], [p].[Name]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // INNER JOIN [Production].[ProductCategory] AS [p] ON [subcategory].[ProductCategoryID] = [p].[ProductCategoryID]

            IQueryable<ProductSubcategory> subcategoriesWithProducts = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.Products);
            subcategoriesWithProducts.WriteLines(subcategory => $@"{subcategory.Name}: {string.Join(
                ", ", subcategory.Products.Select(product => product.Name))}");
#if EF
            // SELECT 
            //    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [Project1].[Name] AS [Name], 
            //    [Project1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Project1].[C1] AS [C1], 
            //    [Project1].[ProductID] AS [ProductID], 
            //    [Project1].[Name1] AS [Name1], 
            //    [Project1].[ListPrice] AS [ListPrice], 
            //    [Project1].[ProductSubcategoryID1] AS [ProductSubcategoryID1], 
            //    [Project1].[RowVersion] AS [RowVersion]
            //    FROM ( SELECT 
            //        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //        [Extent2].[ProductID] AS [ProductID], 
            //        [Extent2].[Name] AS [Name1], 
            //        [Extent2].[ListPrice] AS [ListPrice], 
            //        [Extent2].[ProductSubcategoryID] AS [ProductSubcategoryID1], 
            //        [Extent2].[RowVersion] AS [RowVersion], 
            //        CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
            //        FROM  [Production].[ProductSubcategory] AS [Extent1]
            //        LEFT OUTER JOIN [Production].[Product] AS [Extent2] ON [Extent1].[ProductSubcategoryID] = [Extent2].[ProductSubcategoryID]
            //    )  AS [Project1]
            //    ORDER BY [Project1].[ProductSubcategoryID] ASC, [Project1].[C1] ASC
#else
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductSubcategoryID]

            // SELECT [p].[ProductID], [p].[ListPrice], [p].[Name], [p].[ProductSubcategoryID], [p].[RowVersion]
            // FROM [Production].[Product] AS [p]
            // WHERE EXISTS (
            //    SELECT 1
            //    FROM [Production].[ProductSubcategory] AS [subcategory]
            //    WHERE [p].[ProductSubcategoryID] = [subcategory].[ProductSubcategoryID])
            // ORDER BY [p].[ProductSubcategoryID]
#endif
        }

#if EF
        internal static void EagerLoadingMultipleLevels(AdventureWorks adventureWorks)
        {
            IQueryable<Product> products = adventureWorks.Products
                .Include(product => product.ProductProductPhotos
                    .Select(productProductPhoto => productProductPhoto.ProductPhoto));
            products.WriteLines(product => $@"{product.Name}: {string.Join(
                ", ",
                product.ProductProductPhotos.Select(productProductPhoto =>
                    productProductPhoto.ProductPhoto.LargePhotoFileName))}");
            // SELECT 
            //    [Project1].[ProductID] AS [ProductID], 
            //    [Project1].[Name] AS [Name], 
            //    [Project1].[ListPrice] AS [ListPrice], 
            //    [Project1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [Project1].[RowVersion] AS [RowVersion], 
            //    [Project1].[C1] AS [C1], 
            //    [Project1].[ProductID1] AS [ProductID1], 
            //    [Project1].[ProductPhotoID] AS [ProductPhotoID], 
            //    [Project1].[ProductPhotoID1] AS [ProductPhotoID1], 
            //    [Project1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //    [Project1].[ModifiedDate] AS [ModifiedDate]
            //    FROM ( SELECT 
            //        [Extent1].[ProductID] AS [ProductID], 
            //        [Extent1].[Name] AS [Name], 
            //        [Extent1].[ListPrice] AS [ListPrice], 
            //        [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //        [Extent1].[RowVersion] AS [RowVersion], 
            //        [Join1].[ProductID] AS [ProductID1], 
            //        [Join1].[ProductPhotoID1] AS [ProductPhotoID], 
            //        [Join1].[ProductPhotoID2] AS [ProductPhotoID1], 
            //        [Join1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //        [Join1].[ModifiedDate] AS [ModifiedDate], 
            //        CASE WHEN ([Join1].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
            //        FROM  [Production].[Product] AS [Extent1]
            //        LEFT OUTER JOIN  (SELECT [Extent2].[ProductID] AS [ProductID], [Extent2].[ProductPhotoID] AS [ProductPhotoID1], [Extent3].[ProductPhotoID] AS [ProductPhotoID2], [Extent3].[LargePhotoFileName] AS [LargePhotoFileName], [Extent3].[ModifiedDate] AS [ModifiedDate]
            //            FROM  [Production].[ProductProductPhoto] AS [Extent2]
            //            INNER JOIN [Production].[ProductPhoto] AS [Extent3] ON [Extent2].[ProductPhotoID] = [Extent3].[ProductPhotoID] ) AS [Join1] ON [Extent1].[ProductID] = [Join1].[ProductID]
            //    )  AS [Project1]
            //    ORDER BY [Project1].[ProductID] ASC, [Project1].[C1] ASC
        }
#else
        internal static void EagerLoadingMultipleLevels(AdventureWorks adventureWorks)
        {
            IQueryable<Product> products = adventureWorks.Products
                .Include(product => product.ProductProductPhotos)
                .ThenInclude(productProductPhoto => productProductPhoto.ProductPhoto);
            products.WriteLines(product => $@"{product.Name}: {string.Join(
                ", ",
                product.ProductProductPhotos.Select(productProductPhoto =>
                    productProductPhoto.ProductPhoto.LargePhotoFileName))}");
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ProductID]

            // SELECT [p].[ProductID], [p].[ProductPhotoID], [p0].[ProductPhotoID], [p0].[LargePhotoFileName], [p0].[ModifiedDate]
            // FROM [Production].[ProductProductPhoto] AS [p]
            // INNER JOIN [Production].[ProductPhoto] AS [p0] ON [p].[ProductPhotoID] = [p0].[ProductPhotoID]
            // WHERE EXISTS (
            //    SELECT 1
            //    FROM [Production].[Product] AS [product]
            //    WHERE [p].[ProductID] = [product].[ProductID])
            // ORDER BY [p].[ProductID]
        }
#endif
        internal static void LazyLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP (1) 
            //    [c].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [c].[Name] AS [Name], 
            //    [c].[ProductCategoryID] AS [ProductCategoryID]
            //    FROM [Production].[ProductSubcategory] AS [c]
            subcategory.Name.WriteLine();

            ProductCategory category = subcategory.ProductCategory; // Execute query.
            // exec sp_executesql N'SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[ProductCategory] AS [Extent1]
            //    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1
            category.Name.WriteLine();

            ICollection<Product> products = subcategory.Products; // Execute query.
            // exec sp_executesql N'SELECT 
            //    [Extent1].[ProductID] AS [ProductID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent1].[ListPrice] AS [ListPrice], 
            //    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE [Extent1].[ProductSubcategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1
            products.WriteLines(product => product.Name);
        }

        internal static void MultipleLazyLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories.ToArray(); // Execute query.
            // SELECT 
            //    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
            //    [Extent1].[Name] AS [Name], 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID]
            //    FROM [Production].[ProductSubcategory] AS [Extent1]

            subcategories.WriteLines(subcategory => 
                $"{subcategory.Name} ({subcategory.ProductCategory.Name})"); // Execute query.
            // exec sp_executesql N'SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[ProductCategory] AS [Extent1]
            //    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=1

            // exec sp_executesql N'SELECT 
            //    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[ProductCategory] AS [Extent1]
            //    WHERE [Extent1].[ProductCategoryID] = @EntityKeyValue1',N'@EntityKeyValue1 int',@EntityKeyValue1=2

            // ...
        }

        internal static void LazyLoadingAndDeferredExecution(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
            subcategories
                .ForEach(subcategory =>  // Reading subcategories is in progress.
                    $"{subcategory.ProductCategory.Name}/{subcategory.Name}: {subcategory.Products.Count}".WriteLine());
            // EntityCommandExecutionException: There is already an open DataReader relational with this Command which must be closed first.
        }

        internal static void LazyLoadingAndImmediateExecution(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
            subcategories
                .ToArray() // Finish reading subcategories.
                .ForEach(subcategory =>
                    $"{subcategory.ProductCategory/* Finish reading category. */.Name}/{subcategory.Name}: {subcategory.Products/* Finish reading products. */.Count}".WriteLine());
        }

        internal static void EagerLoadingWithIncludeAndSelect(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> categories = adventureWorks.ProductCategories
                .Include(category => category.ProductSubcategories.Select(subcategory => subcategory.Products));
            categories.WriteLines(category =>
                $@"{category.Name}: {string.Join(", ", category.ProductSubcategories
                    .Select(subcategory => $"{subcategory.Name}-{subcategory.Products.Count}"))}");
        }

        internal static void EagerLoadingWithSelect(AdventureWorks adventureWorks)
        {
            var subcategories = adventureWorks.ProductSubcategories.Select(subcategory => new
            {
                Name = subcategory.Name,
                CategoryName = subcategory.ProductCategory.Name,
                ProductCount = subcategory.Products.Count
            });
            subcategories.WriteLines(subcategory =>
                $"{subcategory.CategoryName}/{subcategory.Name}: {subcategory.ProductCount}");
        }

        internal static void PrintSubcategoriesWithLazyLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories
                .GroupBy(subcategory => subcategory.ProductCategoryID, (key, group) => group.FirstOrDefault())
                .ToArray(); // 1 query for N subcategories.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Name} ({subcategory.ProductCategory.Name})"); // N queries for each subcategory's category.
        }

        internal static void PrintSubcategoriesWithEagerLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories
                .GroupBy(subcategory => subcategory.ProductCategoryID, (key, group) => group.FirstOrDefault())
                .Include(subcategory => subcategory.ProductCategory)
                .ToArray(); // 1 query for N subcategories.
            subcategories.WriteLines(subcategory =>
                $"{subcategory.Name} ({subcategory.ProductCategory.Name})"); // N queries.
        }

        internal static void ConditionalEagerLoadingWithInclude(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.Products.Where(product => product.ListPrice > 0));
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}");
            // ArgumentException: The Include path expression must refer to a navigation property defined on the type. Use dotted paths for reference navigation properties and the Select operator for collection navigation properties.
        }

        internal static void ConditionalEagerLoadingWithSelect(AdventureWorks adventureWorks)
        {
            var subcategories = adventureWorks.ProductSubcategories.Select(subcategory => new
            {
                Subcategory = subcategory,
                Products = subcategory.Products.Where(product => product.ListPrice > 0)
            });
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.Subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}");
        }

#if EF
        internal static void DisableLazyLoading(AdventureWorks adventureWorks)
        {
            adventureWorks.Configuration.LazyLoadingEnabled = false;
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            subcategory.Name.WriteLine();
            ProductCategory category = subcategory.ProductCategory; // No query.
            (category == null).WriteLine(); // True

            ICollection<Product> products = subcategory.Products; // No query.
            (products == null).WriteLine(); // True
        }

        internal static void DisableProxy(AdventureWorks adventureWorks)
        {
            adventureWorks.Configuration.ProxyCreationEnabled = false;
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.

            ProductCategory relationalCategory = subcategory.ProductCategory; // No database query.
            (relationalCategory == null).WriteLine(); // True

            ICollection<Product> relationalProducts = subcategory.Products; // No database query.
            subcategory.Products.IsNullOrEmpty().WriteLine(); // No database query. True
        }
#endif
    }

    internal static class DataAccess
    {
        internal static IQueryable<Product> QueryCategoryProducts(string category)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                return adventureWorks.Products.Where(
                    product => product.ProductSubcategory.ProductCategory.Name == category);
            }
        }
    }

    internal static class UI
    {
        internal static void RenderCategoryProducts(string category) => DataAccess
            .QueryCategoryProducts(category)
            .Select(product => product.Name)
            .WriteLines();
        // InvalidOperationException: The operation cannot be completed because the DbContext has been disposed.
    }
}

#if DEMO
namespace Tutorial.LinqToEntities
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

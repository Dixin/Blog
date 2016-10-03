namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics;
    using System.Linq;

    using Dixin.Common;
    using Dixin.Linq;

    public static class QueryableExtensions
    {
        public static IEnumerator<TSource> GetIterator<TSource>(
            this IQueryable<TSource> query, DbContext dbContext)
        {
            query.NotNull(nameof(query));
            dbContext.NotNull(nameof(dbContext));

            IEnumerator<TSource> sqlReader = null;
            bool isSqlExecuted = false;
            return new Iterator<TSource>(
                state: IteratorState.Start,
                start: () =>
                    {
                        Trace.WriteLine("|_Convert expression tree to database command tree.");
                        DbQueryCommandTree commandTree = dbContext.Convert(query.Expression);
                        Trace.WriteLine("|_Generate SQL from database command tree.");
                        DbCommand sql = dbContext.Generate(commandTree);
                        Trace.WriteLine("|_Build SQL query.");
                        IEnumerable<TSource> sqlQuery = dbContext.Database.SqlQuery<TSource>(
                            sql.CommandText,
                            sql.Parameters.Cast<DbParameter>().Select(parameter => parameter.Value).ToArray());
                        sqlReader = sqlQuery.GetEnumerator();
                    },
                moveNext: () =>
                    {
                        if (!isSqlExecuted)
                        {
                            Trace.WriteLine("|_Execute SQL query.");
                            isSqlExecuted = true;
                        }
                        Trace.WriteLine($"|_Try reading a row and materializing to {typeof(TSource).Name} object.");
                        return sqlReader.MoveNext();
                    },
                getCurrent: () => sqlReader.Current,
                dispose: () => sqlReader.Dispose());
        }
    }

    internal static partial class Laziness
    {
        internal static void WhereAndSelect()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> products = adventureWorks.Products
                    .Where(product => product.Name.StartsWith("M"));
                // products.ForEach(product => Trace.WriteLine(product));
                Trace.WriteLine("Get iterator from LINQ to Entities query.");
                using (IEnumerator<Product> iterator = products
                    .GetIterator(adventureWorks)) // products.GetEnumerator()
                {
                    while (new Func<bool>(() =>
                        {
                            Trace.WriteLine("Try moving iterator to next.");
                            return iterator.MoveNext(); // Translate and execute query.
                        })())
                    {
                        Product product = iterator.Current;
                        Trace.WriteLine($"Get iterator current product: {product.Name}.");
                    }
                }
            }
        }
    }

    internal static partial class Laziness
    {
        internal static void ImplicitLazyLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.
                Trace.WriteLine(subcategory.Name);
                ProductCategory associatedCategory = subcategory.ProductCategory; // Database query.
                Trace.WriteLine(associatedCategory.Name);
                ICollection<Product> associatedProducts = subcategory.Products; // Database query.
                Trace.WriteLine(associatedProducts.Count);
            }
        }

        internal static void ExplicitLazyLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.
                Trace.WriteLine(subcategory.Name);
                adventureWorks
                    .Entry(subcategory) // Return DbEntityEntry<ProductSubcategory>.
                    .Reference(entity => entity.ProductCategory) // Return DbReferenceEntry<ProductSubcategory, ProductCategory>.
                    .Load(); // Database query.
                Trace.WriteLine(subcategory.ProductCategory.Name);
                adventureWorks
                    .Entry(subcategory) // Return DbEntityEntry<ProductSubcategory>.
                    .Collection(entity => entity.Products) // Return DbCollectionEntry<ProductSubcategory, Product>.
                    .Load(); // Database query.
                Trace.WriteLine(subcategory.Products.Count);
            }
        }

        internal static void ExplicitLazyLoadingWithQuery()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.
                Trace.WriteLine(subcategory.Name);
                string associatedCategoryName = adventureWorks
                    .Entry(subcategory).Reference(entity => entity.ProductCategory)
                    .Query() // Return IQueryable<ProductCategory>.
                    .Select(category => category.Name).Single(); // Database query.
                Trace.WriteLine(associatedCategoryName);
                int associatedProductsCount = adventureWorks
                    .Entry(subcategory).Collection(entity => entity.Products)
                    .Query() // Return IQueryable<Product>.
                    .Count(); // Database query.
                Trace.WriteLine(associatedProductsCount);
            }
        }

        internal static void LazyLoadingAndDeferredExecution()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
                subcategories
                    .ForEach(subcategory => Trace.WriteLine( // Reading subcategories is in progress.
                        $"{subcategory.ProductCategory.Name}/{subcategory.Name}: {subcategory.Products.Count}"));
                // EntityCommandExecutionException: There is already an open DataReader associated with this Command which must be closed first.
            }
        }

        internal static void LazyLoadingAndImmediateExecution()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
                subcategories
                    .ToArray() // Finish reading subcategories.
                    .ForEach(subcategory => Trace.WriteLine(
                        $"{subcategory.ProductCategory/* Finish reading category. */.Name}/{subcategory.Name}: {subcategory.Products/* Finish reading products. */.Count}"));
            }
        }

        internal static void EagerLoadingWithInclude()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories
                    .Include(subcategory => subcategory.ProductCategory)
                    .Include(subcategory => subcategory.Products);
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $"{subcategory.ProductCategory.Name}/{subcategory.Name}: {subcategory.Products.Count}"));
            }
        }

        internal static void EagerLoadingWithIncludeAndSelect()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> categories = adventureWorks.ProductCategories
                    .Include(category => category.ProductSubcategories.Select(subcategory => subcategory.Products));
                categories.ForEach(category => Trace.WriteLine(
                    $@"{category.Name}: {string.Join(", ", category.ProductSubcategories
                        .Select(subcategory => $"{subcategory.Name}-{subcategory.Products.Count}"))}"));
            }
        }

        internal static void EagerLoadingWithSelect()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var subcategories = adventureWorks.ProductSubcategories.Select(subcategory => new
                {
                    Name = subcategory.Name,
                    CategoryName = subcategory.ProductCategory.Name,
                    ProductCount = subcategory.Products.Count
                });
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $"{subcategory.CategoryName}/{subcategory.Name}: {subcategory.ProductCount}"));
            }
        }

        internal static void PrintSubcategoriesWithLazyLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID, (key, group) => group.FirstOrDefault())
                    .ToArray(); // 1 query for N subcategories.
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $"{subcategory.Name} ({subcategory.ProductCategory.Name})")); // N queries.
            }
        }

        internal static void PrintSubcategoriesWithEagerLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID, (key, group) => group.FirstOrDefault())
                    .Include(subcategory => subcategory.ProductCategory)
                    .ToArray(); // 1 query for N subcategories.
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $"{subcategory.Name} ({subcategory.ProductCategory.Name})")); // N queries.
            }
        }

        internal static void ConditionalEagerLoadingWithInclude()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories
                    .Include(subcategory => subcategory.Products.Where(product => product.ListPrice > 0));
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $@"{subcategory.Name}: {string.Join(
                        ", ", subcategory.Products.Select(product => product.Name))}"));
                // ArgumentException: The Include path expression must refer to a navigation property defined on the type. Use dotted paths for reference navigation properties and the Select operator for collection navigation properties.
            }
        }

        internal static void ConditionalEagerLoadingWithSelect()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var subcategories = adventureWorks.ProductSubcategories.Select(subcategory => new
                {
                    Subcategory = subcategory,
                    Products = subcategory.Products.Where(product => product.ListPrice > 0)
                });
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $@"{subcategory.Subcategory.Name}: {string.Join(
                        ", ", subcategory.Products.Select(product => product.Name))}"));
            }
        }

        internal static void DisableLazyLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Configuration.LazyLoadingEnabled = false;
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.
                Trace.WriteLine(subcategory.Name);
                ProductCategory associatedCategory = subcategory.ProductCategory; // No database query.
                Trace.WriteLine(associatedCategory == null); // True
                ICollection<Product> associatedProducts = subcategory.Products; // No database query.
                Trace.WriteLine(associatedProducts.Count); // 0
            }
        }

        internal static void DisableProxy()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Configuration.ProxyCreationEnabled = false;
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Database query.
                Trace.WriteLine(subcategory.Name);
                ProductCategory associatedCategory = subcategory.ProductCategory; // No database query.
                Trace.WriteLine(associatedCategory == null); // True
                ICollection<Product> associatedProducts = subcategory.Products; // No database query.
                Trace.WriteLine(associatedProducts.Count); // 0
            }
        }
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
            .ForEach(name => Trace.WriteLine(name));
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

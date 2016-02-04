namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static partial class QueryMethods
    {
        internal static void InlinePredicate()
        {
            IQueryable<string> query = AdventureWorks.Products
                .Where(product => product.ListPrice > 0 && product.ProductSubcategory != null)
                .Select(product => product.Name); // Define query.
            query.ForEach(); // Execute query.
        }

        internal static void InlinePredicateCompiled()
        {
            ParameterExpression product = Expression.Parameter(typeof(Product), nameof(product));
            IQueryable<string> query = AdventureWorks.Products // ... FROM [Product].
                .Where(Expression.Lambda<Func<Product, bool>>( // ... WHERE ....
                    Expression.AndAlso( // ... AND ....
                        Expression.GreaterThan( // ... > ....
                            Expression.Property(product, nameof(Product.ListPrice)), // [Product].[ListPrice].
                            Expression.Constant(0M, typeof(decimal))), // 0.
                        Expression.NotEqual( // ... IS NOT ....
                            Expression.Property(product, nameof(Product.ProductSubcategory)), // [Product].[ProductSubcategoryID].
                            Expression.Constant(null, typeof(object)))), // NULL.
                    product))
                .Select(Expression.Lambda<Func<Product, string>>( // SELECT ....
                    Expression.Property(product, nameof(Product.Name)), // [Product].[Name].
                    product)); // Define query.
            query.ForEach(); // Execute query.
        }

        internal static bool IsValid
            (this Product product) => product.ListPrice > 0 && product.ProductSubcategory != null;

        internal static void MethodPredicate()
        {
            IQueryable<string> query = AdventureWorks.Products
                .Where(product => product.IsValid())
                .Select(product => product.Name); // Define query.
            query.ForEach(); // Execute query.
            // NotSupportedException: Method 'Boolean IsValid(Dixin.Linq.LinqToSql.Product)' has no supported translation to SQL.
        }

        internal static void MethodPredicateCompiled()
        {
            ParameterExpression product = Expression.Parameter(typeof(Product), nameof(product));
            IQueryable<string> query = AdventureWorks.Products
                .Where(Expression.Lambda<Func<Product, bool>>( // product => product.IsValid().
                    Expression.Call(
                        null,
                        typeof(QueryMethods).GetMethod(nameof(IsValid), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod),
                        product), // IsValid has no SQL translation.
                    product))
                .Select(Expression.Lambda<Func<Product, string>>(
                    Expression.Property(product, nameof(Product.Name)),
                    product)); // Define query.
            query.ForEach(); // Execute query.
            // NotSupportedException: Method 'Boolean IsValid(Dixin.Linq.LinqToSql.Product)' has no supported translation to SQL.
        }

        internal static void MethodSelector()
        {
            var query = AdventureWorks.Products
                .Where(product => product.ProductID > 100)
                .Select(product => new { Name = product.Name, IsValid = product.IsValid() }); // Define query.
            query.ForEach(); // Execute query.
        }

        internal static void LocalSelector()
        {
            var query = AdventureWorks.Products
                .Where(product => product.ProductID > 100)
                .AsEnumerable()
                .Select(product => new { Name = product.Name, IsValid = product.IsValid() }); // Define query.
            query.ForEach(); // Execute query.
        }
    }
}

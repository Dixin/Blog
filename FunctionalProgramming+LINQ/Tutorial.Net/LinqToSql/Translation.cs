namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;

    using static Tutorial.LinqToObjects.EnumerableX;

    internal static partial class Translation
    {
        private static readonly AdventureWorks AdventureWorks = new AdventureWorks();

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
            // NotSupportedException: Method 'Boolean IsValid(Tutorial.LinqToSql.Product)' has no supported translation to SQL.
        }

        internal static void MethodPredicateCompiled()
        {
            ParameterExpression product = Expression.Parameter(typeof(Product), nameof(product));
            Func<Product, bool> isValidMethod = IsValid;
            IQueryable<string> query = AdventureWorks.Products
                .Where(Expression.Lambda<Func<Product, bool>>( // product => product.IsValid().
                    Expression.Call(isValidMethod.Method, product), // IsValid has no SQL translation.
                    product))
                .Select(Expression.Lambda<Func<Product, string>>(
                    Expression.Property(product, nameof(Product.Name)),
                    product)); // Define query.
            query.ForEach(); // Execute query.
            // NotSupportedException: Method 'Boolean IsValid(Tutorial.LinqToSql.Product)' has no supported translation to SQL.
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

        internal static void RemoteMethod()
        {
            IQueryable<DateTime> query = AdventureWorks.ProductPhotos
                .Where(photo => SqlMethods.DateDiffYear(photo.ModifiedDate, DateTime.Now) >= 5)
                .Select(photo => photo.ModifiedDate); // Define query.
            query.ForEach(); // Execute query.
        }
    }
}

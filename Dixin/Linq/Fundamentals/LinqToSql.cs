namespace Dixin.Linq.Fundamentals
{
    using System.Linq;

    using Dixin.Linq.LinqToSql;

    internal static partial class LinqToSql
    {
        internal static string[] ProductNames(string categoryName)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<string> query =
                    from product in adventureWorks.Products
                    where product.ProductSubcategory.ProductCategory.Name == categoryName
                    orderby product.ListPrice ascending
                    select product.Name; // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }

    internal static partial class LinqToSql
    {
        internal static string[] ProductNames(string categoryName, int pageSize, int pageIndex)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<string> query =
                    (from product in adventureWorks.Products
                     where product.ProductSubcategory.ProductCategory.Name == categoryName
                     orderby product.ListPrice ascending
                     select product.Name)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }

    internal static partial class LinqToSql
    {
        internal static string[] ProductNames2(string categoryName, int pageSize, int pageIndex)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<string> query = adventureWorks
                    .Products
                    .Where(product => product.ProductSubcategory.ProductCategory.Name == categoryName)
                    .OrderBy(product => product.ListPrice)
                    .Select(product => product.Name)
                    .Skip(pageSize * checked(pageIndex - 1))
                    .Take(pageSize); // Define query.
                return query.ToArray(); // Execute query.
            }
        }
    }
}

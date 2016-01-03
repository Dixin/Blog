namespace Dixin.Linq.Fundamentals
{
    using System.Linq;

    using Dixin.Linq.LinqToSql;

    public static partial class LinqToSql
    {
        public static string[] ProductNames(string categoryName)
        {
            using (AdventureWorksDataContext adventureWorks = new AdventureWorksDataContext())
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

    public static partial class LinqToSql
    {
        public static string[] ProductNames(string categoryName, int pageSize, int pageIndex)
        {
            using (AdventureWorksDataContext adventureWorks = new AdventureWorksDataContext())
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

    public static partial class LinqToSql
    {
        public static string[] ProductNames2(string categoryName, int pageSize, int pageIndex)
        {
            using (AdventureWorksDataContext adventureWorks = new AdventureWorksDataContext())
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

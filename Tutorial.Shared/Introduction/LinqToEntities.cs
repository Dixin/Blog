namespace Tutorial.Introduction
{
    using System.Diagnostics;
    using System.Linq;

    using Tutorial.LinqToEntities;

    using Product = Tutorial.LinqToEntities.Product;

    internal static partial class LinqToEntities
    {
        internal static void QueryExpression()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products; // Get source.
                IQueryable<string> query = from product in source
                                           where product.ProductSubcategory.ProductCategory.Name == "Bikes"
                                           orderby product.ListPrice
                                           select product.Name; // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToEntities
    {
        internal static void QueryMethods()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products; // Get source.
                IQueryable<string> query = source
                    .Where(product => product.ProductSubcategory.ProductCategory.Name == "Bikes")
                    .OrderBy(product => product.ListPrice)
                    .Select(product => product.Name); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToEntities
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

    internal static partial class LinqToEntities
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

    internal static partial class LinqToEntities
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

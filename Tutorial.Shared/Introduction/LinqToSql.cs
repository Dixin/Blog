namespace Tutorial.Introduction
{
#if NETFX
    using System.Diagnostics;
    using System.Linq;

    using Tutorial.LinqToSql;
#endif

#if NETFX
    internal static partial class LinqToSql
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
#endif
}

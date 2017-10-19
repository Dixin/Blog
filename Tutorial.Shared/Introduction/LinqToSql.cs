#if NETFX
namespace Tutorial.Introduction
{
    using System.Diagnostics;
    using System.Linq;

    using Tutorial.LinqToSql;

    internal static partial class Linq
    {
        internal static void LinqToSql()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products; // Get source.
                IQueryable<string> query = from product in source
                    where product.ProductSubcategory.ProductCategory.Name == "Bikes"
                    orderby product.ListPrice
                    select product.Name; // Define query.
                // Equivalent to:
                // IQueryable<string> query = source
                //    .Where(product => product.ProductSubcategory.ProductCategory.Name == "Bikes")
                //    .OrderBy(product => product.ListPrice)
                //    .Select(product => product.Name);
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
}
#endif

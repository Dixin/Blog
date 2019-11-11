namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Linq;

    internal static partial class Performance
    {
        private static readonly Func<AdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((AdventureWorks adventureWorks, decimal listPrice) => adventureWorks
                .Products
                .Where(product => product.ListPrice == listPrice)
                .Select(product => product.Name));

        internal static IQueryable<string> GetProductNames
            (this AdventureWorks adventureWorks, decimal listPrice) =>
                GetProductNamesCompiled(adventureWorks, listPrice);
    }

    internal static partial class Performance
    {
        internal static void QueryPlanCache()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory[] where1 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("A")).ToArray();
                ProductCategory[] where2 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("B")).ToArray(); // Reused.
                ProductCategory[] where3 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("Bike")).ToArray(); // Reused.

                ProductSubcategory[] take1 = adventureWorks.ProductSubcategories
                    .Take(1).ToArray();
                ProductSubcategory[] take2 = adventureWorks.ProductSubcategories
                    .Take(2).ToArray(); // Not reused.
            }
        }
    }
}

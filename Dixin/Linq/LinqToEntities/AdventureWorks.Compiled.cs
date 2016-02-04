namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    internal static class CompiledQueries
    {
        private static readonly Func<ObjectContext, string, Product[]> getCategoryProducts =
            CompiledQuery.Compile((ObjectContext adventureWorks, string category) => adventureWorks
                .CreateObjectSet<Product>()
                .Where(product => product.ProductSubcategory.ProductCategory.Name == category)
                .ToArray());

        internal static Product[] GetCategoryProducts
            (this AdventureWorks adventureWorks, string category) =>
                getCategoryProducts(adventureWorks.ObjectContext(), category);
    }
}

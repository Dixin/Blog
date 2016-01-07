namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Linq;

    internal static class CompiledQueries
    {
        public static readonly Func<ObjectContext, string, Product[]> getCategoryProducts =
            CompiledQuery.Compile((ObjectContext adventureWorks, string category) => adventureWorks
                .CreateObjectSet<Product>()
                .Where(product => product.ProductSubcategory.ProductCategory.Name == category)
                .ToArray());

        public static Product[] GetCategoryProducts
            (this AdventureWorksDbContext adventureWorks, string category) =>
                getCategoryProducts(adventureWorks.ObjectContext(), category);
    }
}

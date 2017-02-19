namespace Tutorial.LinqToSql
{
    using System.Data.Linq;
    using System.Diagnostics;
    using System.Linq;

    internal static partial class Laziness
    {
        private static readonly AdventureWorks AdventureWorks = new AdventureWorks();

        internal static void DeferredExecution()
        {
            IQueryable<ProductSubcategory> subcategories = AdventureWorks.ProductSubcategories;
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $@"{subcategory.ProductCategory?.Name}/{subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}"));
        }

        internal static void EagerLoadingWithSelect()
        {
            var subcategories = AdventureWorks.ProductSubcategories
                .Select(subcategory => new
                {
                    Category = subcategory.ProductCategory.Name,
                    Subcategory = subcategory.Name,
                    Products = subcategory.Products.Select(product => product.Name)
                });
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $"{subcategory.Category}/{subcategory}: {string.Join(", ", subcategory.Products)}"));
        }

        internal static void EagerLoadingWithRelationship()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<ProductSubcategory>(subcategory => subcategory.Products);
                options.LoadWith<ProductSubcategory>(subcategory => subcategory.ProductCategory);
                adventureWorks.LoadOptions = options;
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $@"{subcategory.ProductCategory?.Name}/{subcategory.Name}: {string.Join(
                        ", ", subcategory.Products.Select(product => product.Name))}"));
            }
        }

        internal static void ConditionalEagerLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<ProductSubcategory>(subcategory => subcategory.Products);
                options.AssociateWith<ProductSubcategory>(subcategory => subcategory.Products.Where(
                    product => product.ListPrice > 0));
                adventureWorks.LoadOptions = options;
                IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories;
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $@"{subcategory.Name}: {string.Join(
                        ", ", subcategory.Products.Select(product => product.Name))}"));
            }
        }

        internal static void DisableDeferredLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.DeferredLoadingEnabled = false; // Default: true.
                IQueryable<ProductSubcategory> subcategories = AdventureWorks.ProductSubcategories;
                subcategories.ForEach(subcategory => Trace.WriteLine(
                    $@"{subcategory.ProductCategory?.Name}/{subcategory.Name}: {string.Join(
                        ", ", subcategory.Products.Select(product => product.Name))}"));
            }
        }
    }
}

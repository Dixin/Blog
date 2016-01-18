namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq;
    using System.Diagnostics;
    using System.Linq;

    public static class DataAccess
    {
        public static IQueryable<Product> QueryCategoryProducts(string category)
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                return adventureWorks.Products.Where(
                    product => product.ProductSubcategory.ProductCategory.Name == category);
            }
        }
    }

    public static class UI
    {
        public static void ViewCategoryProducts(string category) => DataAccess
            .QueryCategoryProducts(category)
            .Select(product => product.Name)
            .ForEach(name => Trace.WriteLine(name));
        // ObjectDisposedException: Cannot access a disposed object. Object name: 'DataContext accessed after Dispose.'.
    }

    public static partial class QueryMethods
    {
        public static void DeferredLoading()
        {
            IQueryable<ProductSubcategory> subcategories = AdventureWorks.ProductSubcategories;
            subcategories.ForEach(subcategory => Trace.WriteLine(
                $@"{subcategory.ProductCategory?.Name}/{subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}"));
        }

        public static void EagerLoadingWithSelect()
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

        public static void EagerLoadingWithAssociation()
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

        public static void ConditionalEagerLoading()
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

        public static void NoLoading()
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

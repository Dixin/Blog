namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    public class LegacyAdventureWorks : ObjectContext
    {
        private ObjectSet<Product> products;

        public LegacyAdventureWorks()
            : base(new AdventureWorks().ObjectContext().Connection as EntityConnection)
        {
        }

        public ObjectSet<Product> Products => this.products ?? (this.products = this.CreateObjectSet<Product>());
    }

    internal static class CompiledQueries
    {
        private static readonly Func<LegacyAdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((LegacyAdventureWorks adventureWorks, decimal listPrice) => adventureWorks
                .Products
                .Where(product => product.ListPrice == listPrice)
                .Select(product => product.Name));

        internal static IQueryable<string> GetProductNames
            (this LegacyAdventureWorks adventureWorks, decimal listPrice) =>
                GetProductNamesCompiled(adventureWorks, listPrice);
    }

    internal static partial class Performance
    {
        internal static void PrintViews()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                StorageMappingItemCollection mappingItemCollection = adventureWorks.ObjectContext().MetadataWorkspace
                .GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection;
                Trace.WriteLine(mappingItemCollection.ComputeMappingHashValue());
                mappingItemCollection.GenerateViews(new EdmSchemaError[0]).ForEach(view =>
                    {
                        Trace.WriteLine($"{view.Key.EntityContainer.Name}.{view.Key.Name}");
                        Trace.WriteLine(view.Value.EntitySql);
                    });
            }
        }

        internal static void ObjectCache()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product[] products = adventureWorks.Products
                    .Where(product => product.Name.StartsWith("Road-750 Black")).ToArray();
                Product cachedProduct = adventureWorks.Products.Find(999); // Reused.
            }
        }

        internal static void QueryPlanCache()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory[] where1 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("A")).ToArray();
                ProductCategory[] where2 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("B")).ToArray(); // Reused.
                ProductCategory[] where3 = adventureWorks.ProductCategories
                    .Where(category => category.Name.StartsWith("Bike")).ToArray(); // Not reused.

                ProductSubcategory[] take1 = adventureWorks.ProductSubcategories
                    .Take(1).ToArray();
                ProductSubcategory[] take2 = adventureWorks.ProductSubcategories
                    .Take(2).ToArray(); // Not reused.
            }
        }
    }

    internal static partial class Performance
    {
        internal static async Task Async()
        {
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) // BEGIN TRANSACTION.
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory category1 = await adventureWorks.ProductCategories
                    .FindAsync(1); // Async IO.
                ProductCategory category2 = await adventureWorks.ProductCategories
                    .SingleAsync(category => category.Name.StartsWith("A")); // Async IO.
                await adventureWorks.ProductSubcategories
                    .Where(subcategory => subcategory.ProductCategoryID == category1.ProductCategoryID)
                    .ForEachAsync(subcategory => subcategory.ProductCategoryID = category2.ProductCategoryID); // Async IO.
                Product[] products = await adventureWorks.Products
                    .OrderByDescending(product => product.ListPrice)
                    .Take(5)
                    .ToArrayAsync(); // Async IO.
                adventureWorks.ProductCategories.Remove(category1);
                await adventureWorks.SaveChangesAsync(); // Async IO.
            } // ROLLBACK TRANSACTION.
        }
    }
}

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
                    .Where(category => category.Name.StartsWith("Bik")).ToArray(); // Not reused.

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
                ProductCategory category = await adventureWorks.ProductCategories.FindAsync(1); // Async IO.
                ProductSubcategory[] subcategories = await adventureWorks.ProductSubcategories
                    .Where(subcategory => subcategory.ProductCategory == category).ToArrayAsync(); // Async IO.
                adventureWorks.ProductSubcategories.RemoveRange(subcategories);
                await adventureWorks.Products
                    .Where(product => product.ListPrice > 1000)
                    .ForEachAsync(product => product.ListPrice -= 50); // Async IO.
                await adventureWorks.SaveChangesAsync();
            } // ROLLBACK TRANSACTION.
        }
    }
}

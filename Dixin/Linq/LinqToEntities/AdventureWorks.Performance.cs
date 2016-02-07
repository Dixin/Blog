namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    public class LegacyAdventureWorks : ObjectContext
    {
        public LegacyAdventureWorks()
            : base(new AdventureWorks().ObjectContext().Connection as EntityConnection)
        {
        }

        public ObjectSet<Product> Products => this.CreateObjectSet<Product>();
    }

    internal static class CompiledQueries
    {
        private static readonly Func<LegacyAdventureWorks, decimal, IQueryable<string>> GetProductNamesCompiled =
            CompiledQuery.Compile((LegacyAdventureWorks adventureWorks, decimal maxListPrice) => adventureWorks
                .Products
                .Where(product => product.ListPrice <= maxListPrice)
                .Select(product => product.Name));

        internal static IQueryable<string> GetProductNames
            (this LegacyAdventureWorks adventureWorks, decimal maxListPrice) =>
                GetProductNamesCompiled(adventureWorks, maxListPrice);
    }

    internal class Performance
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

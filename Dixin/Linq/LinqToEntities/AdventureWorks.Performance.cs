namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    public class Performance
    {
        public static async Task Async()
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

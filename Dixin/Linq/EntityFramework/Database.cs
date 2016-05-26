namespace Dixin.Linq.EntityFramework
{
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    public partial class AdventureWorks : DbContext
    {
        public AdventureWorks()
            : base(ConnectionStrings.AdventureWorks)
        {
        }
    }

    internal static partial class Query
    {
        internal static void Dispose()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                // Operations.
            }
        }
    }

    internal static partial class Query
    {
        internal static void Table()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> allRowsInTable = adventureWorks.ProductCategories;
                allRowsInTable.ForEach(categoryRow => Trace.WriteLine(
                    $"{categoryRow.ProductCategoryID}:{categoryRow.Name}"));
                // 1:Bikes 2:Components 3:Clothing 4:Accessories 
            }
        }
    }

    public partial class AdventureWorks
    {
        static AdventureWorks()
        {
            Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>()); // Call once.
            // Equivalent to: Database.SetInitializer<AdventureWorks>(null);
        }
    }
}

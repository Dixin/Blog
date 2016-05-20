namespace Dixin.Linq.EntityFramework
{
    using System.Data.Entity;

    public class WomensProduct : Product
    {
    }

    public class MensProduct : Product
    {
    }

    public class UniversalProduct : Product
    {
    }

    public partial class Product
    {
        // public string Style { get; set; } causes an EntityCommandCompilationException: Condition member 'Product.Style' with a condition other than 'IsNull=False' is mapped. Either remove the condition on Product.Style or remove it from the mapping.
    }

    public enum Style
    {
        W,
        M,
        U
    }

    public partial class AdventureWorks
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Product>()
                .Map<WomensProduct>(mapping => mapping.Requires(nameof(Style)).HasValue(nameof(Style.W)))
                .Map<MensProduct>(mapping => mapping.Requires(nameof(Style)).HasValue(nameof(Style.M)))
                .Map<UniversalProduct>(mapping => mapping.Requires(nameof(Style)).HasValue(nameof(Style.U)));
        }
    }
}

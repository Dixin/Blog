namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;

    public partial class Product
    {
        public const string Style = nameof(Style);
        // public string Style { get; } works.
        // public string Style { get; set; } causes an EntityCommandCompilationException: error 3032: Problem in mapping fragments starting at line 23:Condition member 'Product.Style' with a condition other than 'IsNull=False' is mapped. Either remove the condition on Product.Style or remove it from the mapping.
    }

    public class WomenProduct : Product
    {
    }

    public class MenProduct : Product
    {
    }

    public class UniversalProduct : Product
    {
    }

    public partial class AdventureWorksDbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder) => modelBuilder
            .Entity<Product>()
            .Map<WomenProduct>(
                mappingConfiguration => mappingConfiguration.Requires(nameof(Product.Style)).HasValue("W "))
            .Map<MenProduct>(
                mappingConfiguration => mappingConfiguration.Requires(nameof(Product.Style)).HasValue("M "))
            .Map<UniversalProduct>(
                mappingConfiguration => mappingConfiguration.Requires(nameof(Product.Style)).HasValue("U "));
    }
}

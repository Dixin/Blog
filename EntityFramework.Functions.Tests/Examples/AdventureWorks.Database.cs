namespace EntityFramework.Functions.Tests.Examples
{
    using System.Data.Entity;

    using EntityFramework.Functions.Tests.Properties;

    public partial class AdventureWorksDbContext : DbContext
    {
        static AdventureWorksDbContext()
        {
            Database.SetInitializer<AdventureWorksDbContext>(null);
            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorksDbContext>());
        }

        public AdventureWorksDbContext()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
        }
    }
}

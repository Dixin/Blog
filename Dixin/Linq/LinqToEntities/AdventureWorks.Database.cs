namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;

    using Dixin.Properties;

    public partial class AdventureWorks : DbContext
    {
        static AdventureWorks()
        {
            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>());
            Database.SetInitializer<AdventureWorks>(null);
        }

        public AdventureWorks()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
        }
    }
}

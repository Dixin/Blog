namespace Dixin.Linq.LinqToEntities
{
    using System.Data.Entity;

    using Dixin.Common;
    using Dixin.Properties;

    public partial class AdventureWorksDbContext : DbContext
    {
        static AdventureWorksDbContext()
        {
            // Initializes |DataDirectory| in connection string.
            // <connectionStrings>
            //  <add name="EntityFramework.Functions.Tests.Properties.Settings.AdventureWorksConnectionString"
            //    connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"
            //    providerName="System.Data.SqlClient" />
            // </connectionStrings>
            AppDomainData.SetDefaultDataDirectory();

            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorksDbContext>());
            Database.SetInitializer<AdventureWorksDbContext>(null);
        }

        public AdventureWorksDbContext()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
        }
    }
}

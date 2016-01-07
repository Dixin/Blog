namespace Dixin.Linq.LinqToSql
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;

    using Dixin.Common;
    using Dixin.Properties;

    [Database(Name = "[AdventureWorks]")]
    public partial class AdventureWorksDataContext : DataContext
    {
        public AdventureWorksDataContext()
            : base(Settings.Default.AdventureWorksConnectionString)
        {
            // if (!this.DatabaseExists())
            // {
            //    this.CreateDatabase();
            // }
        }
    }

    public partial class AdventureWorksDataContext
    {
        static AdventureWorksDataContext()
        {
            // Initializes |DataDirectory| in connection string.
            // <connectionStrings>
            //  <add name="EntityFramework.Functions.Tests.Properties.Settings.AdventureWorksConnectionString"
            //    connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"
            //    providerName="System.Data.SqlClient" />
            // </connectionStrings>
            AppDomainData.SetDefaultDataDirectory();
        }
    }
}

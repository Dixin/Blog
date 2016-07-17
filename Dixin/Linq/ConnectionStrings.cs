namespace Dixin.Linq
{
    using System.Configuration;
    using System.Linq;

    using Dixin.Properties;

    internal static partial class ConnectionStrings
    {
        internal const string LocalDb = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30";
    }

    internal static partial class ConnectionStrings
    {
        internal static string AdventureWorks { get; } =
            ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().FirstOrDefault()?
                .ConnectionString ?? Settings.Default.AdventureWorksConnectionString;
            // @"Data Source=192.168.0.200;Initial Catalog=AdventureWorks;User ID=sa;Password=";
    }

#if DEMO
    internal static partial class ConnectionStrings
    {
        internal const string AdventureWorks = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30";
    }

    internal static partial class ConnectionStrings
    {
        static ConnectionStrings()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", @"D:\Dixin\GitHub\CodeSnippets\Data");
        }
    }
#endif
}

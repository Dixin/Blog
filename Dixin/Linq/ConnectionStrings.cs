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
            ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().FirstOrDefault()
            ?.ConnectionString ?? Settings.Default.AdventureWorksConnectionString;
    }
}

namespace Dixin.Linq
{
    using System.Configuration;
    using System.Linq;

    using Dixin.Properties;

    internal static class Connection
    {
        internal static string String { get; } =
            ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().FirstOrDefault()
            ?.ConnectionString ?? Settings.Default.AdventureWorksConnectionString;
    }
}

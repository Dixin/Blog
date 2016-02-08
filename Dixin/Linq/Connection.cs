namespace Dixin.Linq
{
    using System.Configuration;

    using Dixin.Properties;

    internal static class Connection
    {
        internal static string String { get; } =
            ConfigurationManager.ConnectionStrings[nameof(Settings.AdventureWorksConnectionString)]?.ConnectionString
            ?? Settings.Default.AdventureWorksConnectionString;
    }
}

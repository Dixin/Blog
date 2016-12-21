namespace Dixin.Linq
{
#if NETFX
    using System.Configuration;
    using System.Linq;
#endif

    internal static partial class ConnectionStrings
    {
        internal const string LocalDb = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30";
    }

    internal static partial class ConnectionStrings
    {
#if NETFX
        internal static string AdventureWorks { get; } =
            ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().FirstOrDefault()?.ConnectionString
            ?? @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30";
#else
        internal const string AdventureWorks = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\Dixin\GitHub\CodeSnippets\Data\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30";
#endif
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

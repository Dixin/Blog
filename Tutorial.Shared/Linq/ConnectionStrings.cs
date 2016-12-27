namespace Dixin.Linq
{
#if NETFX
    using System.Configuration;
    using System.Linq;
#endif
    using System.IO;
    using System.Reflection;

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
        internal static string AdventureWorks
        {
            get
            {
                string directory = Path.GetDirectoryName(typeof(ConnectionStrings).GetTypeInfo().Assembly.Location);
                string path = Path.Combine(directory, @"..\..\..\..\Data\AdventureWorks_Data.mdf");
                return $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={Path.GetFullPath(path)};Integrated Security=True;Connect Timeout=30";
            }
        }
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

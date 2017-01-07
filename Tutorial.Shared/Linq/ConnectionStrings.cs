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
            @"Server=dixinyan-t460s;Database=WideWorldImporters;Integrated Security=False;User ID=sa;Password=ftSq1@zure;";
#else
        internal static string AdventureWorks
        {
            get
            {
                // string directory = Path.GetDirectoryName(typeof(ConnectionStrings).GetTypeInfo().Assembly.Location);
                // string path = Path.Combine(directory, @"..\..\..\..\Data\AdventureWorks_Data.mdf");
                // return $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={Path.GetFullPath(path)};Integrated Security=True;Connect Timeout=30";
                // return "Server=dixinyan-t460s;Database=WideWorldImporters;Integrated Security=False;User ID=sa;Password=ftSq1@zure;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
                return "Server=dixinyan-t460s;Database=WideWorldImporters;Integrated Security=False;User ID=sa;Password=ftSq1@zure;";
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

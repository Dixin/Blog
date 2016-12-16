namespace Dixin.Linq
{
    internal static partial class ConnectionStrings
    {
        internal const string LocalDb = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30";
    }

    internal static partial class ConnectionStrings
    {
        internal const string AdventureWorks = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30";
    }

#if DEMO
    internal static partial class ConnectionStrings
    {
        static ConnectionStrings()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", @"D:\Dixin\GitHub\CodeSnippets\Data");
        }
    }
#endif
}

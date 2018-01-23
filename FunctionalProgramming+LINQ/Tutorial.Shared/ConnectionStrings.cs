namespace Tutorial
{
#if NETFX
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
#else
    using System.Data.SqlClient;
    using System.IO;

    using Microsoft.Extensions.Configuration;
#endif

    internal static class ConnectionStrings
    {
        internal static string AdventureWorks { get; } =
#if NETFX
            ConfigurationManager.ConnectionStrings[nameof(AdventureWorks)].ConnectionString.FormatFilePath();
#else
            new ConfigurationBuilder().AddJsonFile("app.json").Build()
                .GetConnectionString(nameof(AdventureWorks)).FormatFilePath();
#endif

        private static string FormatFilePath(this string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() { ConnectionString = connectionString };
            if (string.IsNullOrEmpty(builder.AttachDBFilename) || Path.IsPathRooted(builder.AttachDBFilename))
            {
                return connectionString;
            }
            string directory = Path.GetDirectoryName(typeof(ConnectionStrings).Assembly.Location);
            builder.AttachDBFilename = Path.GetFullPath(Path.Combine(directory, builder.AttachDBFilename));
            return builder.ConnectionString;
        }
    }
}

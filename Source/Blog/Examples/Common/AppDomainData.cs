namespace Examples.Common;

public static class AppDomainData
{
    // <connectionStrings>
    //  <add name="EntityFramework.Functions.Tests.Properties.Settings.AdventureWorksConnectionString"
    //    connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"
    //    providerName="System.Data.SqlClient" />
    // </connectionStrings>
    public static string DataDirectory
    {
        get => (string?)AppDomain.CurrentDomain.GetData(nameof(DataDirectory)) ?? string.Empty;

        set => AppDomain.CurrentDomain.SetData(
            nameof(DataDirectory),
            // .. in path does not work. so use new DirectoryInfo(path).FullName to remove .. in path.
            new DirectoryInfo(value).FullName);
    }
}
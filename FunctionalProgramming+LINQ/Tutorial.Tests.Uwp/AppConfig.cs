namespace Tutorial.Tests.Uwp
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LinqToEntities;

    [TestClass]
    public class AppConfig
    {
        [AssemblyInitialize]
        public static void ConnectionString(TestContext _)
        {
            // UWP does not support LocalDB.
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(AdventureWorks)] = "Server=tcp:dixin.database.windows.net,1433;Initial Catalog=AdventureWorks;Persist Security Info=False;User ID=dixinyan;Password=...;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }
    }
}

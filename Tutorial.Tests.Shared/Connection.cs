namespace Tutorial.Tests
{
    using System.Data.SqlClient;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void ConnectionStringsTest()
        {
            string connectionString = ConnectionStrings.AdventureWorks;
            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionString));
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
            {
                ConnectionString = ConnectionStrings.AdventureWorks
            };
            if (!string.IsNullOrWhiteSpace(builder.AttachDBFilename))
            {
                Assert.IsFalse(builder.AttachDBFilename.Contains(".."));
            }
        }
    }
}

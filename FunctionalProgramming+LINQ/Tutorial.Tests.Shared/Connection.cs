namespace Tutorial.Tests
{
    using System.Data.SqlClient;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void LoadConnectionStringsTest()
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

        [TestMethod]
        public void OpenConnectionStringsTest()
        {
            string connectionString = ConnectionStrings.AdventureWorks;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Assert.IsFalse(string.IsNullOrWhiteSpace(connection.ServerVersion));
            }
        }
    }
}

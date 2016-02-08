namespace Dixin.Tests.Linq
{
    using Dixin.Linq;
    using Dixin.Tests.Properties;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void ConnectionStringTest()
        {
            string connectionString = Connection.String;
            Assert.AreEqual(Settings.Default.AdventureWorksConnectionString, connectionString);
        }
    }
}

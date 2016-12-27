namespace Dixin.Tests.Linq
{
#if NETFX
    using System.Configuration;
    using System.Linq;

    using Dixin.Linq;
#endif

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void ConnectionStringsTest()
        {
#if NETFX
            string connectionString = ConnectionStrings.AdventureWorks;
            Assert.AreEqual(ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().First().ConnectionString, connectionString);
#endif
        }
    }
}

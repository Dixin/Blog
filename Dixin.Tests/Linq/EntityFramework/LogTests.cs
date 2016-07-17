namespace Dixin.Tests.Linq.EntityFramework
{
    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LogTests
    {
        [TestMethod]
        public void LogTest()
        {
            Log.DbQueryToString();
            Log.DatabaseLog();
            Log.DbCommandInterceptor();
        }
    }
}

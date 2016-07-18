namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LogsTests
    {
        [TestMethod]
        public void LogTest()
        {
            Log.DataQueryToString();
            Log.DataContextLog();
            Log.DataContexGetCommand();
        }
    }
}

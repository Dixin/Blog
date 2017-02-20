namespace Tutorial.Tests.LinqToSql
{
    using Tutorial.LinqToSql;

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

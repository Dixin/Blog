namespace Tutorial.Tests.LinqToEntities
{
    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LogTests
    {
        [TestMethod]
        public void LogTest()
        {
#if NETFX
            Log.DbQueryToString();
            Log.DatabaseLog();
            Log.DbCommandInterceptor();
#else
            Log.TraceLogger();
#endif
        }
    }
}

namespace Tutorial.Tests.LinqToEntities
{
    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TracingTests
    {
        [TestMethod]
        public void TracingTest()
        {
#if NETFX
            Tracing.DbQueryToString();
            Tracing.DatabaseLog();
            Tracing.Interceptor();
#else
            Tracing.TraceLogger();
#endif
        }
    }
}

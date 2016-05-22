namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class AdventureWorksTests
    {
        [TestMethod]
        public void LogTest()
        {
            Log.WhereWithLog();
        }
    }
}

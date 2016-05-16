namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.EntityFramework;

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

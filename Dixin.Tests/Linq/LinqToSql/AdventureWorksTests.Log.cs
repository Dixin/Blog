namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.LinqToEntities;

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

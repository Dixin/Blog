namespace Dixin.Tests.Linq.LinqToEntities
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

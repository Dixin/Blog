namespace Tutorial.Tests.LinqToSql
{
    using System.Data.Linq;
    using System.Diagnostics;

    using Tutorial.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void ConflictTest()
        {
            Concurrency.DefaultControl();
            try
            {
                Concurrency.CheckModifiedDate();
                Assert.Fail();
            }
            catch (ChangeConflictException exception)
            {
                Trace.WriteLine(exception);
            }
            Concurrency.DatabaseWins();
            Concurrency.ClientWins();
            Concurrency.MergeClientAndDatabase();
        }
    }
}

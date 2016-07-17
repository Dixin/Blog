namespace Dixin.Tests.Linq.EntityFramework
{
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Transactions;

    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void DetectConflictTest()
        {
            using (new TransactionScope())
            {
                Concurrency.NoCheck();
            }
            using (new TransactionScope())
            {
                try
                {
                    Concurrency.ConcurrencyCheck();
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
            using (new TransactionScope())
            {
                try
                {
                    Concurrency.RowVersion();
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
        }

        [TestMethod]
        public void UpdateConflictTest()
        {
            using (new TransactionScope())
            {
                Concurrency.UpdateProductDatabaseWins();
            }
            using (new TransactionScope())
            {
                Concurrency.UpdateProductClientWins();
            }
            using (new TransactionScope())
            {
                Concurrency.UpdateProductMergeClientAndDatabase();
            }
            using (new TransactionScope())
            {
                Concurrency.SaveChanges();
            }
        }
    }
}

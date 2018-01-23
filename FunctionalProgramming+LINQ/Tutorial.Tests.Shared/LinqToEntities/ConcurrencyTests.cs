namespace Tutorial.Tests.LinqToEntities
{
#if NETFX
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    using Tutorial.LinqToEntities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using System.Diagnostics;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    using static TransactionHelper;
#endif

    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void DetectConflictTest()
        {
#if NETFX
            using (new TransactionHelper())
            {
                Concurrency.NoCheck(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
            }
            using (new TransactionHelper())
            {
                try
                {
                    Concurrency.ConcurrencyCheck(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
            using (new TransactionHelper())
            {
                try
                {
                    Concurrency.RowVersion(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
#else
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => Concurrency.NoCheck(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.ConcurrencyCheck(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.RowVersion(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
#endif
        }

        [TestMethod]
        public void UpdateConflictTest()
        {
#if NETFX
            using (new TransactionHelper())
            {
                Concurrency.DatabaseWins(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
            }
            using (new TransactionHelper())
            {
                Concurrency.ClientWins(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
            }
            using (new TransactionHelper())
            {
                Concurrency.MergeClientAndDatabase(new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()), new DbReaderWriter(new AdventureWorks()));
            }
            using (new TransactionHelper())
            {
                Concurrency.SaveChanges(new AdventureWorks(), new AdventureWorks());
            }
#else
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.DatabaseWins(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.ClientWins(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => 
                Concurrency.MergeClientAndDatabase(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2) => 
                Concurrency.SaveChanges(adventureWorks1, adventureWorks2));
#endif
        }
    }
}

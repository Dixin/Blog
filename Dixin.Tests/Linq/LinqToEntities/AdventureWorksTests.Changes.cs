namespace Dixin.Tests.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Transactions;

    using Dixin.Linq.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class AdventureWorksTests
    {
        [TestMethod]
        public void TracingTest()
        {
            Tracking.EntitiesFromSameContext();
            Tracking.MappingsFromSameContext();
            Tracking.EntitiesFromContexts();
            Tracking.Changes();
            Tracking.Attach();
            Tracking.AssociationChanges();
        }

        [TestMethod]
        public void ChangesTest()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int subcategoryId = Changes.Insert();
                Changes.Update();
                Changes.UpdateWithNoChange();
                Changes.Delete();
                Changes.DeleteWithNoQuery(subcategoryId);
                Changes.DeleteWithAssociation();
                try
                {
                    Changes.UntrackedChanges();
                    Assert.Fail();
                }
                catch (InvalidOperationException exception)
                {
                    Trace.WriteLine(exception);
                }
                scope.Complete();
            }
        }

        [TestMethod]
        public void TransactionTest()
        {
            Transactions.Implicit();
            Transactions.ExplicitLocal();
            Transactions.ExplicitDistributable();
        }

        [TestMethod]
        public void ConflictTest()
        {
            Concurrency.DefaultControl();
            try
            {
                Concurrency.CheckModifiedDate();
                Assert.Fail();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                Trace.WriteLine(exception);
            }
            Concurrency.DatabaseWins();
            Concurrency.ClientWins();
            Concurrency.MergeClientAndDatabase();
        }
    }
}

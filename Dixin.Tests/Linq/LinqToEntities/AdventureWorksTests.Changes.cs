namespace Dixin.Tests.Linq.LinqToEntities
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;

    using Dixin.Linq.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChangesTests
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
            Changes.Insert();
            Changes.Update();
            Changes.UpdateWithNoChange();
            Changes.Delete();
            Changes.DeleteWithNoQuery();
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
        }

        [TestMethod]
        public void TransactionTest()
        {
            Transaction.Implicit();
            Transaction.ExplicitLocal();
            Transaction.ExplicitDistributable();
        }

        [TestMethod]
        public void ConflictTest()
        {
            Concurrency.LastWins().Wait();
            try
            {
                Concurrency.Check().Wait();
                Assert.Fail();
            }
            catch (AggregateException exception) when (exception.InnerExceptions.Single() is DbUpdateConcurrencyException)
            {
                Trace.WriteLine(exception);
            }
            Concurrency.DatabaseWins().Wait();
            Concurrency.ClientWins().Wait();
            Concurrency.MergeClientAndDatabase().Wait();
        }
    }
}

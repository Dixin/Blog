namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChangesTests
    {
        [TestMethod]
        public void TracingTest()
        {
            Tracking.EntitiesFromSameDbContext();
            Tracking.ObjectsFromSameDbContext();
            Tracking.EntitiesFromDbContexts();
            Tracking.EntityChanges();
            Tracking.Attach();
            Tracking.AssociationChanges();
            Tracking.AsNoTracking();
            Tracking.DetectChanges();
        }

        [TestMethod]
        public void ChangesTest()
        {
            ProductCategory category = Changes.Create();
            Changes.Update();
            Changes.SaveNoChanges();
            Changes.UpdateWithoutRead(category.ProductCategoryID);
            Changes.Delete();
            Changes.DeleteWithoutRead(category.ProductCategoryID);
            try
            {
                Changes.DeleteWithAssociation();
                Assert.Fail();
            }
            catch (DbUpdateException exception)
            {
                Trace.WriteLine(exception);
            }
            Changes.DeleteAllAssociated();
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
    }
}

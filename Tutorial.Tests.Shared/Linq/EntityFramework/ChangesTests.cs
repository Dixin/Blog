namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
#if NETFX
    using System.Data.Entity.Infrastructure;
#endif
    using System.Diagnostics;

    using Dixin.Linq.EntityFramework;

#if !NETFX
    using Microsoft.EntityFrameworkCore;
#endif
    using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !NETFX
    using static TransactionHelper;
#endif

    [TestClass]
    public class ChangesTests
    {
        [TestMethod]
        public void TrackingTest()
        {
            Tracking.EntitiesFromSameDbContext(new WideWorldImporters());
            Tracking.ObjectsFromSameDbContext(new WideWorldImporters());
            Tracking.EntitiesFromDbContexts();
            Tracking.EntityChanges(new WideWorldImporters());
            Tracking.Attach(new WideWorldImporters());
            Tracking.AssociationChanges(new WideWorldImporters());
            Tracking.AsNoTracking(new WideWorldImporters());
            Tracking.DetectChanges(new WideWorldImporters());
        }

        [TestMethod]
        public void ChangesTest()
        {
#if NETFX
            SupplierCategory category = Changes.Create(new WideWorldImporters());
            Changes.Update(new WideWorldImporters());
            Changes.SaveNoChanges(new WideWorldImporters());
            Changes.UpdateWithoutRead(new WideWorldImporters(), category.SupplierCategoryID);
            Changes.Delete(new WideWorldImporters());
            Changes.DeleteWithoutRead(new WideWorldImporters(), category.SupplierCategoryID);
            try
            {
                Changes.DeleteWithAssociation(new WideWorldImporters());
                Assert.Fail();
            }
            catch (DbUpdateException exception)
            {
                Trace.WriteLine(exception);
            }
            Changes.DeleteAllAssociated(new WideWorldImporters());
            try
            {
                Changes.UntrackedChanges(new WideWorldImporters());
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            Rollback((adventureWorks, adventureWorks2, adventureWorks3, adventureWorks4) =>
            {
                SupplierCategory category = Changes.Create(adventureWorks);
                StockGroup group = Changes.CreateWithRelationship(adventureWorks);
                Changes.Update(adventureWorks);
                Changes.SaveNoChanges(adventureWorks);
                Changes.UpdateWithoutRead(adventureWorks2, category.SupplierCategoryID);
                Changes.Delete(adventureWorks);
                Changes.DeleteWithoutRead(adventureWorks3, category.SupplierCategoryID);
                try
                {
                    Changes.DeleteWithAssociation(adventureWorks);
                    Assert.Fail();
                }
                catch (DbUpdateException exception)
                {
                    Trace.WriteLine(exception);
                }
                Changes.DeleteAllAssociated(adventureWorks4);
                try
                {
                    Changes.UntrackedChanges(adventureWorks);
                    Assert.Fail();
                }
                catch (DbUpdateException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
#endif
        }
    }
}

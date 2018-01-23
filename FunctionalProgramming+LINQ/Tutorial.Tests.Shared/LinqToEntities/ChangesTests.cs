namespace Tutorial.Tests.LinqToEntities
{
#if NETFX
    using System;
    using System.Linq;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;
#else
    using System;
    using System.Linq;
    using System.Diagnostics;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    using static TransactionHelper;
#endif

    [TestClass]
    public class ChangesTests
    {
        [TestMethod]
        public void TrackingTest()
        {
            Tracking.EntitiesFromSameDbContext(new AdventureWorks());
            Tracking.ObjectsFromSameDbContext(new AdventureWorks());
            Tracking.EntitiesFromMultipleDbContexts();
            Tracking.EntityChanges(new AdventureWorks());
            Tracking.Attach(new AdventureWorks());
            Tracking.RelationshipChanges(new AdventureWorks());
            Tracking.AsNoTracking(new AdventureWorks());
            Tracking.DetectChanges(new AdventureWorks());
        }

        [TestMethod]
        public void ChangesTest()
        {
#if NETFX
            ProductCategory category = Changes.Create();
            Changes.Update(category.ProductCategoryID, category.ProductSubcategories.Single().ProductSubcategoryID);
            Changes.SaveNoChanges(1);
            Changes.UpdateWithoutRead(category.ProductCategoryID);
            Changes.Delete(category.ProductSubcategories.Single().ProductSubcategoryID);
            Changes.DeleteWithoutRead(category.ProductCategoryID);
            try
            {
                Changes.DeleteWithRelationship(1);
                Assert.Fail();
            }
            catch (DbUpdateException exception)
            {
                Trace.WriteLine(exception);
            }
            category = Changes.Create();
            Changes.DeleteCascade(category.ProductCategoryID);
            try
            {
                Changes.UntrackedChanges();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            ProductCategory category = Changes.Create();
            Changes.Update(category.ProductCategoryID, category.ProductSubcategories.Single().ProductSubcategoryID);
            Changes.SaveNoChanges(1);
            Changes.UpdateWithoutRead(category.ProductCategoryID);
            Changes.Delete(category.ProductSubcategories.Single().ProductSubcategoryID);
            Changes.DeleteWithoutRead(category.ProductCategoryID);
            try
            {
                Changes.DeleteWithRelationship(1);
                Assert.Fail();
            }
            catch (DbUpdateException exception)
            {
                Trace.WriteLine(exception);
            }
            category = Changes.Create();
            Changes.DeleteCascade(category.ProductCategoryID);
            try
            {
                Changes.UntrackedChanges();
                Assert.Fail();
            }
            catch (DbUpdateException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
        }
    }
}

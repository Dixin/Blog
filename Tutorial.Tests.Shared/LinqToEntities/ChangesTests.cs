namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Linq;
#if NETFX
    using System.Data.Entity.Infrastructure;
#endif
    using System.Diagnostics;

    using Tutorial.LinqToEntities;

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
        public void TracingTest()
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

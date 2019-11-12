namespace Tutorial.Tests.LinqToEntities
{
    using System;
#if NETFX
    using System.Data.Entity.Core;
#endif
    using System.Diagnostics;

    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LoadingTests
    {
        [TestMethod]
        public void DisposableTest()
        {
            try
            {
                UI.RenderCategoryProducts("Bikes");
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void DeferredExecutionTest()
        {
            Loading.DeferredExecution(new AdventureWorks());
        }

        [TestMethod]
        public void ExplicitLoadingTest()
        {
            Loading.ExplicitLoading(new AdventureWorks());
            Loading.ExplicitLoadingWithQuery(new AdventureWorks());
        }

        [TestMethod]
        public void LazyLoadingTest()
        {
#if NETFX
            Loading.LazyLoading(new AdventureWorks());
#else
            try 
	        {	        
                Loading.LazyLoading(new AdventureWorks());
		        Assert.Fail();
	        }
	        catch (NullReferenceException exception)
	        {
                Trace.WriteLine(exception);
	        }
#endif
#if NETFX
            Loading.MultipleLazyLoading(new AdventureWorks());
            try
            {
                Loading.LazyLoadingAndDeferredExecution(new AdventureWorks());
            }
            catch (EntityCommandExecutionException exception)
            {
                Trace.WriteLine(exception);
            }
            Loading.LazyLoadingAndImmediateExecution(new AdventureWorks());
#endif
        }

        [TestMethod]
        public void EagerLoadingTest()
        {
            Loading.EagerLoadingWithInclude(new AdventureWorks());
            Loading.EagerLoadingMultipleLevels(new AdventureWorks());
#if NETFX
            Loading.EagerLoadingWithIncludeAndSelect(new AdventureWorks());
#endif
            Loading.EagerLoadingWithSelect(new AdventureWorks());
#if NETFX
            Loading.PrintSubcategoriesWithLazyLoading(new AdventureWorks());
            Loading.PrintSubcategoriesWithEagerLoading(new AdventureWorks());
            Loading.ConditionalEagerLoadingWithSelect(new AdventureWorks());
            try
            {
                Loading.ConditionalEagerLoadingWithInclude(new AdventureWorks());
            }
            catch (ArgumentException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
        }

        [TestMethod]
        public void DisableLazyLoadingTest()
        {
#if NETFX
            Loading.DisableLazyLoading(new AdventureWorks());
            Loading.DisableProxy(new AdventureWorks());
#endif
        }
    }
}

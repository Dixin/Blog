namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Data.Entity.Core;
    using System.Diagnostics;

    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LazinessTests
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
            Laziness.WhereAndSelect();
        }

        [TestMethod]
        public void LazyLoadingTest()
        {
            Laziness.ImplicitLazyLoading();
            Laziness.ExplicitLazyLoading();
            Laziness.ExplicitLazyLoadingWithQuery();
            try
            {
                Laziness.LazyLoadingAndDeferredExecution();
            }
            catch (EntityCommandExecutionException exception)
            {
                Trace.WriteLine(exception);
            }
            Laziness.LazyLoadingAndImmediateExecution();
        }

        [TestMethod]
        public void EagerLoadingTest()
        {
            Laziness.EagerLoadingWithInclude();
            Laziness.EagerLoadingWithIncludeAndSelect();
            Laziness.EagerLoadingWithSelect();
            Laziness.PrintSubcategoriesWithLazyLoading();
            Laziness.PrintSubcategoriesWithEagerLoading();
            Laziness.ConditionalEagerLoadingWithSelect();
            try
            {
                Laziness.ConditionalEagerLoadingWithInclude();
            }
            catch (ArgumentException exception)
            {
                Trace.WriteLine(exception);
            }
            Laziness.DisableLazyLoading();
            Laziness.DisableProxy();
        }
    }
}

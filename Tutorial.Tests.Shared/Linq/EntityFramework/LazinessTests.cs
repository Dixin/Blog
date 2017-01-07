namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
#if NETFX
    using System.Data.Entity.Core;
#endif
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
            Laziness.WhereAndSelect(new WideWorldImporters());
        }

        [TestMethod]
        public void LazyLoadingTest()
        {
#if NETFX
            Laziness.ImplicitLazyLoading(new WideWorldImporters());
#endif
            Laziness.ExplicitLazyLoading(new WideWorldImporters());
            Laziness.ExplicitLazyLoadingWithQuery(new WideWorldImporters());
#if NETFX
            try
            {
                Laziness.LazyLoadingAndDeferredExecution(new WideWorldImporters());
            }
            catch (EntityCommandExecutionException exception)
            {
                Trace.WriteLine(exception);
            }
            Laziness.LazyLoadingAndImmediateExecution(new WideWorldImporters());
#endif
        }

        [TestMethod]
        public void EagerLoadingTest()
        {
            Laziness.EagerLoadingWithInclude(new WideWorldImporters());
#if NETFX
            Laziness.EagerLoadingWithIncludeAndSelect(new WideWorldImporters());
#endif
            Laziness.EagerLoadingWithSelect(new WideWorldImporters());
#if NETFX
            Laziness.PrintSubcategoriesWithLazyLoading(new WideWorldImporters());
            Laziness.PrintSubcategoriesWithEagerLoading(new WideWorldImporters());
            Laziness.ConditionalEagerLoadingWithSelect(new WideWorldImporters());
            try
            {
                Laziness.ConditionalEagerLoadingWithInclude(new WideWorldImporters());
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
            Laziness.DisableLazyLoading(new WideWorldImporters());
            Laziness.DisableProxy(new WideWorldImporters());
#endif
        }
    }
}

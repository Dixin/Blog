namespace Dixin.Tests.Linq.EntityFramework
{
    using System.Linq;
    using System.Threading.Tasks;
#if NETFX
    using System.Transactions;
#endif

    using Dixin.Linq.EntityFramework;
    using Dixin.Linq.Tests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
#if NETFX
        [TestMethod]
        public void CompiedQueryTest()
        {
            using (LegacyAdventureWorks adventureWorks = new LegacyAdventureWorks())
            {
                string[] productNames = adventureWorks.GetProductNames(25M).ToArray();
                EnumerableAssert.Any(productNames);
            }
        }

        [TestMethod]
        public void ViewsTest()
        {
            //Performance.MappingViews();
        }
#endif

        [TestMethod]
        public void PerformanceTest()
        {
            Performance.AddRange();
            Performance.RemoveRange();
        }

        [TestMethod]
        public void CacheTest()
        {
            Performance.CachedEntity(new WideWorldImporters());
            Performance.UncachedEntity(new WideWorldImporters());
            Performance.Find(new WideWorldImporters());
            Performance.TranslationCache(new WideWorldImporters());
            Performance.UncachedTranslation(new WideWorldImporters());
            Performance.CachedTranslation(new WideWorldImporters());
            Performance.CompiledCachedTranslation(new WideWorldImporters());
            Performance.Translation();
            Performance.UncachedSkipTake(new WideWorldImporters());
#if NETFX
            Performance.CachedSkipTake(new WideWorldImporters());
#endif
        }

        [TestMethod]
        public async Task AsyncTest()
        {
#if NETFX
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Performance.Async(new WideWorldImporters());
            }
#endif
        }

        [TestMethod]
        public async Task AsyncConcurrencyTest()
        {
#if NETFX
            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Performance.SaveChangesAsync(new WideWorldImporters(), new WideWorldImporters());
            }
#endif
        }

        [TestMethod]
        public async Task AsyncTransactionTest()
        {
#if !NETFX
            await Performance.DbContextTransactionAsync(new WideWorldImporters());
            await Performance.DbTransactionAsync();
#endif
#if NETFX
            await Performance.TransactionScopeAsync();
#endif
        }
    }
}

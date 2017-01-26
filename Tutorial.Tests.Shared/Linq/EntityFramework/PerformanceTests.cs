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
                string[] productNames = adventureWorks.GetProductNames(539.99M).ToArray();
                EnumerableAssert.Any(productNames);
            }
        }

        [TestMethod]
        public void ViewsTest()
        {
            Performance.MappingViews();
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
            Performance.CachedEntity(new AdventureWorks());
            Performance.UncachedEntity(new AdventureWorks());
            Performance.Find(new AdventureWorks());
            Performance.TranslationCache(new AdventureWorks());
            Performance.UncachedTranslation(new AdventureWorks());
            Performance.CachedTranslation(new AdventureWorks());
            Performance.CompiledCachedTranslation(new AdventureWorks());
            Performance.Translation();
            Performance.UncachedSkipTake(new AdventureWorks());
#if NETFX
            Performance.CachedSkipTake(new AdventureWorks());
#endif
        }

        [TestMethod]
        public async Task AsyncTest()
        {
#if NETFX
            using (new TransactionHelper())
            {
                await Performance.Async(new AdventureWorks());
            }
#endif
        }

        [TestMethod]
        public async Task AsyncConcurrencyTest()
        {
#if NETFX
            using (new TransactionHelper())
            {
                await Performance.SaveChangesAsync();
            }
#endif
        }

        [TestMethod]
        public async Task AsyncTransactionTest()
        {
#if NETFX
            await Performance.TransactionScopeAsync();
#else
            await Performance.DbContextTransactionAsync(new AdventureWorks());
#endif
            await Performance.DbTransactionAsync();
        }
    }
}

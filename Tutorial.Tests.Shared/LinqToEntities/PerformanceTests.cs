namespace Tutorial.Tests.LinqToEntities
{
    using System.Linq;
    using System.Threading.Tasks;

    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;

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
            Performance.UnreusedTranslationCache(new AdventureWorks());
            Performance.ReusedTranslationCache(new AdventureWorks());
            Performance.CompiledReusedTranslationCache(new AdventureWorks());
            Performance.Translation();
            Performance.UnresuedSkipTakeTranslationCache(new AdventureWorks());
#if NETFX
            Performance.ResuedSkipTakeTranslationCache(new AdventureWorks());
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
            await Performance.DbContextTransactionAsync(new AdventureWorks());
            await Performance.DbTransactionAsync();
#if NETFX
            await Performance.TransactionScopeAsync();
#endif
        }
    }
}

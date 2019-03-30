namespace Tutorial.Tests.LinqToEntities
{
#if NETFX
    using System;
    using System.Diagnostics;

    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    [TestClass]
    public class TranslationTests
    {
        [TestMethod]
        public void WhereAndSelectTest()
        {
            Translation.WhereAndSelect(new AdventureWorks());
            Translation.WhereAndSelectLinqExpressions(new AdventureWorks());
            Translation.CompileWhereAndSelectExpressions(new AdventureWorks());
            Translation.WhereAndSelectDatabaseExpressions(new AdventureWorks());
            Translation.WhereAndSelectSql(new AdventureWorks());
        }

        [TestMethod]
        public void SelectAndFirstTest()
        {
            Translation.SelectAndFirst(new AdventureWorks());
            Translation.SelectAndFirstLinqExpressions(new AdventureWorks());
            Translation.CompileSelectAndFirstExpressions(new AdventureWorks());
            Translation.SelectAndFirstDatabaseExpressions(new AdventureWorks());
            Translation.SelectAndFirstSql(new AdventureWorks());
        }

        [TestMethod]
        public void ApiTranslationTest()
        {
#if NETFX
            try
            {
                Translation.WhereAndSelectWithCustomPredicate(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            Translation.WhereAndSelectWithCustomPredicate(new AdventureWorks());
#endif
            Translation.WhereAndSelectWithLocalPredicate(new AdventureWorks());
#if NETFX
            Translation.DbFunction(new AdventureWorks());
            Translation.SqlFunction(new AdventureWorks());
            Translation.DbFunctionSql(new AdventureWorks());
            Translation.SqlFunctionSql(new AdventureWorks());
#endif
        }
    }
}

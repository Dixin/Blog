namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Diagnostics;

    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TranslationTests
    {
        [TestMethod]
        public void WhereAndSelectTest()
        {
            Translation.WhereAndSelect(new WideWorldImporters());
            Translation.WhereAndSelectExpressions(new WideWorldImporters());
#if NETFX
            Translation.WhereAndSelectCompileExpressions(new WideWorldImporters());
#endif
            Translation.WhereAndSelectCompiledExpressions(new WideWorldImporters());
            Translation.WhereAndSelectGenerateSql(new WideWorldImporters());
        }

        [TestMethod]
        public void SelectAndFirstTest()
        {
            Translation.SelectAndFirst(new WideWorldImporters());
            Translation.SelectAndFirstExpressions(new WideWorldImporters());
#if NETFX
            Translation.SelectAndFirstQuery(new WideWorldImporters());
#endif
            Translation.SelectAndFirstCompiledExpressions(new WideWorldImporters());
            Translation.SelectAndFirstGenerateSql(new WideWorldImporters());
        }

        [TestMethod]
        public void ApiTranslationTest()
        {
            Translation.StringIsNullOrEmpty(new WideWorldImporters());
#if NETFX
            try
            {
                Translation.RemoteMethodCall(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            Translation.RemoteMethodCall(new WideWorldImporters());
#endif
            Translation.LocalMethodCall(new WideWorldImporters());
#if NETFX
            Translation.DbFunction(new WideWorldImporters());
            Translation.SqlFunction(new WideWorldImporters());
            Translation.StringIsNullOrEmptySql(new WideWorldImporters());
            Translation.DbFunctionSql(new WideWorldImporters());
            Translation.SqlFunctionSql(new WideWorldImporters());
#endif
        }
    }
}

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
            Translation.WhereAndSelect();
            Translation.WhereAndSelectExpressions();
            Translation.WhereAndSelectExpressionsToDbExpressions();
            Translation.WhereAndSelectDbExpressions();
            Translation.WhereAndSelectDbExpressionsToSql();
        }

        [TestMethod]
        public void SelectAndFirstTest()
        {
            Translation.SelectAndFirst();
            Translation.SelectAndFirstExpressions();
            Translation.SelectAndFirstQuery();
            Translation.SelectAndFirstDbExpressions();
            Translation.SelectAndFirstDbExpressionsToSql();
        }

        [TestMethod]
        public void ApiTranslationTest()
        {
            Translation.StringIsNullOrEmptyDbExpressions();
            try
            {
                Translation.RemoteMethodCall();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            Translation.LocalMethodCall();
            Translation.DbFunctionDbExpressions();
            Translation.SqlFunctionDbExpressions();
            Translation.StringIsNullOrEmptySql();
            Translation.DbFunctionSql();
            Translation.SqlFunctionSql();
        }
    }
}

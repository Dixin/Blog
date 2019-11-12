namespace Tutorial.Tests.LinqToSql
{
    using System;
    using System.Diagnostics;

    using Tutorial.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TranslationTests
    {
        [TestMethod]
        public void TranslationTest()
        {
            Translation.InlinePredicate();
            Translation.InlinePredicateCompiled();
            try
            {
                Translation.MethodPredicate();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                Translation.MethodPredicateCompiled();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            Translation.MethodSelector();
            Translation.LocalSelector();
            Translation.RemoteMethod();
        }
    }
}

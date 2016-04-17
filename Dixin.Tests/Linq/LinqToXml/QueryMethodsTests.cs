namespace Dixin.Tests.Linq.LinqToXml
{
    using System;
    using System.Diagnostics;
    using System.Xml.XPath;

    using Dixin.Linq.LinqToXml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void LinqTest()
        {
            QueryMethods.ParentAndAncestors();
            QueryMethods.InDocumentOrder();
            try
            {
                QueryMethods.CommonAncestor();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.Elements();
            QueryMethods.ChildrenAndDecendants();
            QueryMethods.ResultXObjects();
        }

        [TestMethod]
        public void XPathTest()
        {
            QueryMethods.XPathQuery();
            try
            {
                QueryMethods.XPathQueryWithNamespace();
                Assert.Fail();
            }
            catch (XPathException exception)
            {
                Trace.WriteLine(exception);
            }            
            QueryMethods.XPathEvaluateObject();
            QueryMethods.XPathEvaluateSequence();
            QueryMethods.XPathEvaluateSequenceWithNamespace();
        }
    }
}

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
        public void NavigationTest()
        {
            QueryMethods.ParentAndAncestors();
            QueryMethods.ChildElements();
            QueryMethods.ChildrenAndDecendants();
            QueryMethods.ResultObjects();
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.DocumentOrder();
            try
            {
                QueryMethods.CommonAncestor();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
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

namespace Dixin.Tests.Linq.Lambda
{
    using System;
    using System.Diagnostics;
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchListTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            ListNode<int> node1 = ChurchList<int>.Create(1)(ChurchList<int>.Null);
            ListNode<int> node2 = ChurchList<int>.Create(2)(node1);
            ListNode<int> node3 = ChurchList<int>.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(ChurchList<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1, node2.Next());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2, node3.Next());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.AreEqual(node1.Value(), node3.Next().Next().Value());
            Assert.AreEqual(ChurchList<int>.Null, node3.Next().Next().Next());
            try
            {
                ChurchList<object>.Null.Next();
                Assert.Fail();
            }
            catch (InvalidCastException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void IsNullTest()
        {
            ListNode<int> node1 = ChurchList<int>.Create(1)(ChurchList<int>.Null);
            ListNode<int> node2 = ChurchList<int>.Create(2)(node1);
            ListNode<int> node3 = ChurchList<int>.Create(3)(node2);
            Assert.IsTrue(ChurchList<object>.Null.IsNull().Unchurch());
            Assert.IsFalse(node1.IsNull().Unchurch());
            Assert.IsFalse(node2.IsNull().Unchurch());
            Assert.IsFalse(node3.IsNull().Unchurch());
            Assert.IsTrue(node1.Next().IsNull().Unchurch());
            Assert.IsFalse(node2.Next().IsNull().Unchurch());
            Assert.IsFalse(node3.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            ListNode<int> node1 = ChurchList<int>.Create(1)(ChurchList<int>.Null);
            ListNode<int> node2 = ChurchList<int>.Create(2)(node1);
            ListNode<int> node3 = ChurchList<int>.Create(3)(node2);
            Assert.AreEqual(node3, node3.NodeAt(0U.Church()));
            Assert.AreEqual(node2, node3.NodeAt(1U.Church()));
            Assert.AreEqual(node1, node3.NodeAt(2U.Church()));
            Assert.IsTrue(node3.NodeAt(3U.Church()).IsNull().Unchurch());
            try
            {
                node3.NodeAt(4U.Church());
                Assert.Fail();
            }
            catch (InvalidCastException exception)
            {
                Trace.WriteLine(exception);
            }
        }
    }
}

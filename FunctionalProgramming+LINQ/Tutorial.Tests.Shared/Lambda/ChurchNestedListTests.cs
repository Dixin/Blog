namespace Tutorial.Tests.LambdaCalculus
{
    using System;
    using System.Diagnostics;
    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchNestedListTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            NestedListNode<int> node1 = ChurchNestedList<int>.Create(1)(ChurchNestedList<int>.Null);
            NestedListNode<int> node2 = ChurchNestedList<int>.Create(2)(node1);
            NestedListNode<int> node3 = ChurchNestedList<int>.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(ChurchNestedList<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1, node2.Next());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2, node3.Next());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.AreEqual(node1.Value(), node3.Next().Next().Value());
            Assert.AreEqual(ChurchNestedList<int>.Null, node3.Next().Next().Next());
            try
            {
                ChurchNestedList<int>.Null.Next();
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
            NestedListNode<int> node1 = ChurchNestedList<int>.Create(1)(ChurchNestedList<int>.Null);
            NestedListNode<int> node2 = ChurchNestedList<int>.Create(2)(node1);
            NestedListNode<int> node3 = ChurchNestedList<int>.Create(3)(node2);
            Assert.IsTrue(ChurchNestedList<object>.Null.IsNull().Unchurch());
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
            NestedListNode<int> node1 = ChurchNestedList<int>.Create(1)(ChurchNestedList<int>.Null);
            NestedListNode<int> node2 = ChurchNestedList<int>.Create(2)(node1);
            NestedListNode<int> node3 = ChurchNestedList<int>.Create(3)(node2);
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
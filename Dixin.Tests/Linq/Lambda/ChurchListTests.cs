namespace Dixin.Tests.Linq.Lambda
{
    using System;

    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Boolean = Dixin.Linq.Lambda.Boolean;

    [TestClass]
    public class ChurchList2Tests
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
            Assert.IsTrue(ChurchNestedList<object>.Null.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            NestedListNode<int> node = ChurchNestedList<int>.Create(1)(ChurchNestedList<int>.Null);
            Assert.IsTrue(ChurchNestedList<object>.Null.IsNull().Unchurch());
            Assert.IsFalse(node.IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            NestedListNode<int> node1 = ChurchNestedList<int>.Create(1)(ChurchNestedList<int>.Null);
            NestedListNode<int> node2 = ChurchNestedList<int>.Create(2)(node1);
            NestedListNode<int> node3 = ChurchNestedList<int>.Create(3)(node2);
            Assert.AreEqual(node3, node3.Index(0U.Church()));
            Assert.AreEqual(node2, node3.Index(1U.Church()));
            Assert.AreEqual(node1, node3.Index(2U.Church()));
            Assert.IsTrue(node3.Index(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(5U.Church()).IsNull().Unchurch());
        }
    }

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
            Assert.IsTrue(ChurchList<object>.Null.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            Assert.IsTrue(ChurchList<object>.Null.IsNull().Unchurch());
            Assert.IsTrue(ChurchList<object>.Null.IsNull().Unchurch());
            Assert.IsTrue(new ListNode<object>(x => new Func<Boolean, Boolean>(y => y)).IsNull().Unchurch());
            Assert.IsFalse(ChurchList<int>.Create(1)(ChurchList<int>.Null).IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            ListNode<int> node1 = ChurchList<int>.Create(1)(ChurchList<int>.Null);
            ListNode<int> node2 = ChurchList<int>.Create(2)(node1);
            ListNode<int> node3 = ChurchList<int>.Create(3)(node2);
            Assert.AreEqual(node3, node3.Index(0U.Church()));
            Assert.AreEqual(node2, node3.Index(1U.Church()));
            Assert.AreEqual(node1, node3.Index(2U.Church()));
            Assert.IsTrue(node3.Index(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(5U.Church()).IsNull().Unchurch());
        }
    }
}

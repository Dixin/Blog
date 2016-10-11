namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

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
            Assert.AreEqual(node3, node3.NodeAt(0U.Church()));
            Assert.AreEqual(node2, node3.NodeAt(1U.Church()));
            Assert.AreEqual(node1, node3.NodeAt(2U.Church()));
            Assert.IsTrue(node3.NodeAt(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.NodeAt(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.NodeAt(5U.Church()).IsNull().Unchurch());
        }
    }
}
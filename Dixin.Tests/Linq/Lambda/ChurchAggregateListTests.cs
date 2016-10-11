namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchAggregateListTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            AggregateListNode<int> node1 = ChurchAggregateList<int>.Create(1)(ChurchAggregateList<int>.Null);
            AggregateListNode<int> node2 = ChurchAggregateList<int>.Create(2)(node1);
            AggregateListNode<int> node3 = ChurchAggregateList<int>.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(ChurchAggregateList<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1.Value(), node2.Next().Value());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.IsTrue(ChurchAggregateList<int>.Null.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            AggregateListNode<int> node = ChurchAggregateList<int>.Create(1)(ChurchAggregateList<int>.Null);
            Assert.IsTrue(ChurchAggregateList<int>.Null.IsNull().Unchurch());
            Assert.IsFalse(node.IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            AggregateListNode<int> node1 = ChurchAggregateList<int>.Create(1)(ChurchAggregateList<int>.Null);
            AggregateListNode<int> node2 = ChurchAggregateList<int>.Create(2)(node1);
            AggregateListNode<int> node3 = ChurchAggregateList<int>.Create(3)(node2);
            Assert.AreEqual(node3.Value(), node3.NodeAt(0U.Church()).Value());
            Assert.AreEqual(node2.Value(), node3.NodeAt(1U.Church()).Value());
            Assert.AreEqual(node1.Value(), node3.NodeAt(2U.Church()).Value());
            Assert.IsTrue(node3.NodeAt(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.NodeAt(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.NodeAt(5U.Church()).IsNull().Unchurch());
        }
    }
}

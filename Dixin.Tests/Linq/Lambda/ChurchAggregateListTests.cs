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
            AggregateListNode<int> node1 = AggregateListNode<int>.Create(1)(AggregateListNode<int>.Null);
            AggregateListNode<int> node2 = AggregateListNode<int>.Create(2)(node1);
            AggregateListNode<int> node3 = AggregateListNode<int>.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(AggregateListNode<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1.Value(), node2.Next().Value());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.IsTrue(AggregateListNode<int>.Null.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            AggregateListNode<int> node = AggregateListNode<int>.Create(1)(AggregateListNode<int>.Null);
            Assert.IsTrue(AggregateListNode<int>.Null.IsNull().Unchurch());
            Assert.IsFalse(node.IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            AggregateListNode<int> node1 = AggregateListNode<int>.Create(1)(AggregateListNode<int>.Null);
            AggregateListNode<int> node2 = AggregateListNode<int>.Create(2)(node1);
            AggregateListNode<int> node3 = AggregateListNode<int>.Create(3)(node2);
            Assert.AreEqual(node3.Value(), node3.Index(0U.Church()).Value());
            Assert.AreEqual(node2.Value(), node3.Index(1U.Church()).Value());
            Assert.AreEqual(node1.Value(), node3.Index(2U.Church()).Value());
            Assert.IsTrue(node3.Index(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(5U.Church()).IsNull().Unchurch());
        }
    }
}

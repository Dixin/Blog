namespace Tutorial.Tests.LambdaCalculus
{
    using Tutorial.LambdaCalculus;

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
            Assert.IsTrue(node1.Next().IsNull().Unchurch());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1.Value(), node2.Next().Value());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.AreEqual(node1.Value(), node3.Next().Next().Value());
            Assert.IsTrue(node3.Next().Next().Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void IsNullTest()
        {
            AggregateListNode<int> node1 = ChurchAggregateList<int>.Create(1)(ChurchAggregateList<int>.Null);
            AggregateListNode<int> node2 = ChurchAggregateList<int>.Create(2)(node1);
            AggregateListNode<int> node3 = ChurchAggregateList<int>.Create(3)(node2);
            Assert.IsTrue(ChurchAggregateList<int>.Null.IsNull().Unchurch());
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
            AggregateListNode<int> node1 = ChurchAggregateList<int>.Create(1)(ChurchAggregateList<int>.Null);
            AggregateListNode<int> node2 = ChurchAggregateList<int>.Create(2)(node1);
            AggregateListNode<int> node3 = ChurchAggregateList<int>.Create(3)(node2);
            Assert.AreEqual(node3.Value(), node3.ListNodeAt(0U.Church()).Value());
            Assert.AreEqual(node2.Value(), node3.ListNodeAt(1U.Church()).Value());
            Assert.AreEqual(node1.Value(), node3.ListNodeAt(2U.Church()).Value());
            Assert.IsTrue(node3.ListNodeAt(3U.Church()).IsNull().Unchurch());
        }
    }
}

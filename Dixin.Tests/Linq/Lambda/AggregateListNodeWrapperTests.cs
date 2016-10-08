namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AggregateListNodeWrapperTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            AggregateListNodeWrapper<int> node1 = AggregateListNodeWrapper<int>.Create(1)(AggregateListNodeWrapper<int>.Null);
            AggregateListNodeWrapper<int> node2 = AggregateListNodeWrapper<int>.Create(2)(node1);
            AggregateListNodeWrapper<int> node3 = AggregateListNodeWrapper<int>.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(AggregateListNodeWrapper<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1.Value(), node2.Next().Value());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.IsTrue(AggregateListNodeWrapper<int>.Null.Next().IsNull().Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            AggregateListNodeWrapper<int> node = AggregateListNodeWrapper<int>.Create(1)(AggregateListNodeWrapper<int>.Null);
            Assert.IsTrue(AggregateListNodeWrapper<int>.Null.IsNull().Unchurch());
            Assert.IsFalse(node.IsNull().Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            AggregateListNodeWrapper<int> node1 = AggregateListNodeWrapper<int>.Create(1)(AggregateListNodeWrapper<int>.Null);
            AggregateListNodeWrapper<int> node2 = AggregateListNodeWrapper<int>.Create(2)(node1);
            AggregateListNodeWrapper<int> node3 = AggregateListNodeWrapper<int>.Create(3)(node2);
            Assert.AreEqual(node3.Value(), node3.Index(0U.Church()).Value());
            Assert.AreEqual(node2.Value(), node3.Index(1U.Church()).Value());
            Assert.AreEqual(node1.Value(), node3.Index(2U.Church()).Value());
            Assert.IsTrue(node3.Index(3U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(4U.Church()).IsNull().Unchurch());
            Assert.IsTrue(node3.Index(5U.Church()).IsNull().Unchurch());
        }
    }
}

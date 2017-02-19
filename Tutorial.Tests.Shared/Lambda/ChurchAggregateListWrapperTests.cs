namespace Tutorial.Tests.LambdaCalculus
{
    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchAggregateListWrapperTests
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
        public void IsNullTest()
        {
            AggregateListNodeWrapper<int> node1 = AggregateListNodeWrapper<int>.Create(1)(AggregateListNodeWrapper<int>.Null);
            AggregateListNodeWrapper<int> node2 = AggregateListNodeWrapper<int>.Create(2)(node1);
            AggregateListNodeWrapper<int> node3 = AggregateListNodeWrapper<int>.Create(3)(node2);
            Assert.IsTrue(AggregateListNodeWrapper<object>.Null.IsNull().Unchurch());
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

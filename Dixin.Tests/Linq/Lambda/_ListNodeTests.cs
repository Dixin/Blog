namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class _ListNodeTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            _ListNode<int> node1 = _ListNodeExtensions.Create(1)(_ListNode<int>.Null);
            _ListNode<int> node2 = _ListNodeExtensions.Create(2)(node1);
            _ListNode<int> node3 = _ListNodeExtensions.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(_ListNode<int>.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1.Value(), node2.Next().Value());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2.Value(), node3.Next().Value());
            Assert.IsTrue(_ListNode<int>.Null.Next().IsNull()._Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            _ListNode<int> node = _ListNodeExtensions.Create(1)(_ListNode<int>.Null);
            Assert.IsTrue(_ListNode<int>.Null.IsNull()._Unchurch());
            Assert.IsFalse(node.IsNull()._Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            _ListNode<int> node1 = _ListNodeExtensions.Create(1)(_ListNode<int>.Null);
            _ListNode<int> node2 = _ListNodeExtensions.Create(2)(node1);
            _ListNode<int> node3 = _ListNodeExtensions.Create(3)(node2);
            Assert.AreEqual(node3.Value(), node3.Index(0U._Church()).Value());
            Assert.AreEqual(node2.Value(), node3.Index(1U._Church()).Value());
            Assert.AreEqual(node1.Value(), node3.Index(2U._Church()).Value());
            Assert.IsTrue(node3.Index(3U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(4U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(5U._Church()).IsNull()._Unchurch());
        }
    }
}

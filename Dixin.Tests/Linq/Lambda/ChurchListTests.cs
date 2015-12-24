namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchList2Tests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            ListNode2<int> node1 = ChurchList2.Create(1)(ChurchList2.Null);
            ListNode2<int> node2 = ChurchList2.Create(2)(node1);
            ListNode2<int> node3 = ChurchList2.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(ChurchList2.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1, node2.Next());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2, node3.Next());
            Assert.IsTrue(new ListNode2<object>(ChurchList2.Null).Next().IsNull()._Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            ListNode2<int> node = ChurchList2.Create(1)(ChurchList2.Null);
            Assert.IsTrue(ChurchList2.IsNull<object>(ChurchList2.Null)._Unchurch());
            Assert.IsFalse(node.IsNull()._Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            ListNode2<int> node1 = ChurchList2.Create(1)(ChurchList2.Null);
            ListNode2<int> node2 = ChurchList2.Create(2)(node1);
            ListNode2<int> node3 = ChurchList2.Create(3)(node2);
            Assert.AreEqual(node3, node3.Index(0U._Church()));
            Assert.AreEqual(node2, node3.Index(1U._Church()));
            Assert.AreEqual(node1, node3.Index(2U._Church()));
            Assert.IsTrue(node3.Index(3U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(4U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(5U._Church()).IsNull()._Unchurch());
        }
    }

    [TestClass]
    public class ChurchListTests
    {
        [TestMethod]
        public void CreateValueNextTest()
        {
            ListNode<int> node1 = ChurchList.Create(1)(ChurchList.Null);
            ListNode<int> node2 = ChurchList.Create(2)(node1);
            ListNode<int> node3 = ChurchList.Create(3)(node2);
            Assert.AreEqual(1, node1.Value());
            Assert.AreEqual(ChurchList.Null, node1.Next());
            Assert.AreEqual(2, node2.Value());
            Assert.AreEqual(node1, node2.Next());
            Assert.AreEqual(3, node3.Value());
            Assert.AreEqual(node2, node3.Next());
            Assert.IsTrue(ChurchList.GetNull<object>().Next().IsNull()._Unchurch());
        }

        [TestMethod]
        public void NullIsNullTest()
        {
            Assert.IsTrue(ChurchList.IsNull<object>(ChurchList.Null)._Unchurch());
            Assert.IsTrue(ChurchList.GetNull<object>().IsNull()._Unchurch());
            Assert.IsTrue(new ListNode<object>(ChurchBoolean.False<Boolean<object, ListNode<object>>, Boolean>).IsNull()._Unchurch());
            Assert.IsFalse(ChurchList.Create(1)(ChurchList.Null).IsNull()._Unchurch());
        }

        [TestMethod]
        public void IndexTest()
        {
            ListNode<int> node1 = ChurchList.Create(1)(ChurchList.Null);
            ListNode<int> node2 = ChurchList.Create(2)(node1);
            ListNode<int> node3 = ChurchList.Create(3)(node2);
            Assert.AreEqual(node3, node3.Index(0U._Church()));
            Assert.AreEqual(node2, node3.Index(1U._Church()));
            Assert.AreEqual(node1, node3.Index(2U._Church()));
            Assert.IsTrue(node3.Index(3U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(4U._Church()).IsNull()._Unchurch());
            Assert.IsTrue(node3.Index(5U._Church()).IsNull()._Unchurch());
        }
    }
}

namespace Dixin.Tests.Linq.Lambda
{
    using Dixin.Linq.Lambda;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Dixin.Linq.Lambda.ChurchBoolean;

    public partial class ChurchBooleanTests
    {
        [TestMethod]
        public void TrueTest()
        {
            Assert.AreEqual(1, True(1)("2"));
            Assert.AreEqual("a", True("a")(null));
            Assert.AreEqual(null, True(null)(1));
            object @object = new object();
            Assert.AreEqual(@object, True(@object)(null));
        }

        [TestMethod]
        public void FalseTest()
        {
            Assert.AreEqual(1, False("2")(1));
            Assert.AreEqual("a", False(null)("a"));
            Assert.AreEqual(null, False(1)(null));
            object @object = new object();
            Assert.AreEqual(@object, False(null)(@object));
        }
    }

    [TestClass]
    public partial class ChurchBooleanTests
    {
        [TestMethod]
        public void NotTest()
        {
            Assert.AreEqual(!true, True.Not().Unchurch());
            Assert.AreEqual(!false, False.Not().Unchurch());
        }

        [TestMethod]
        public void AndTest()
        {
            Assert.AreEqual(true && true, True.And(True).Unchurch());
            Assert.AreEqual(true && false, True.And(False).Unchurch());
            Assert.AreEqual(false && true, False.And(True).Unchurch());
            Assert.AreEqual(false && false, False.And(False).Unchurch());
        }

        [TestMethod]
        public void OrTest()
        {
            Assert.AreEqual(true || true, True.Or(True).Unchurch());
            Assert.AreEqual(true || false, True.Or(False).Unchurch());
            Assert.AreEqual(false || true, False.Or(True).Unchurch());
            Assert.AreEqual(false || false, False.Or(False).Unchurch());
        }

        [TestMethod]
        public void XorTest()
        {
            Assert.AreEqual(true ^ true, True.Xor(True).Unchurch());
            Assert.AreEqual(true ^ false, True.Xor(False).Unchurch());
            Assert.AreEqual(false ^ true, False.Xor(True).Unchurch());
            Assert.AreEqual(false ^ false, False.Xor(False).Unchurch());
        }
    }

    public partial class ChurchBooleanTests
    {
        [TestMethod]
        public void IfTest()
        {
            Assert.AreEqual(
                true ? true && false : true || false,
                ChurchBoolean<Boolean>.If(True)(_ => True.And(False))(_ => True.Or(False)).Unchurch());
            Assert.AreEqual(
                false ? true && false : true || false,
                ChurchBoolean<Boolean>.If(False)(_ => True.And(False))(_ => True.Or(False)).Unchurch());

            bool isTrueBranchExecuted = false;
            bool isFalseBranchExecuted = false;
            ChurchBoolean<object>.If(True)
                (_ => { isTrueBranchExecuted = true; return null; })
                (_ => { isFalseBranchExecuted = true; return null; });
            Assert.IsTrue(isTrueBranchExecuted);
            Assert.IsFalse(isFalseBranchExecuted);

            isTrueBranchExecuted = false;
            isFalseBranchExecuted = false;
            ChurchBoolean<object>.If(False)
                (_ => { isTrueBranchExecuted = true; return null; })
                (_ => { isFalseBranchExecuted = true; return null; });
            Assert.IsFalse(isTrueBranchExecuted);
            Assert.IsTrue(isFalseBranchExecuted);
        }
    }
}

namespace Dixin.Linq.Lambda.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchBooleanTests
    {
        [TestMethod]
        public void TrueTest()
        {
            Assert.AreEqual(1, ChurchBoolean.True(1)("2"));
            Assert.AreEqual("a", ChurchBoolean.True("a")(null));
            Assert.AreEqual(null, ChurchBoolean.True(null)(1));
            object @object = new object();
            Assert.AreEqual(@object, ChurchBoolean.True(@object)(null));
        }

        [TestMethod]
        public void FalseTest()
        {
            Assert.AreEqual(1, ChurchBoolean.False("2")(1));
            Assert.AreEqual("a", ChurchBoolean.False(null)("a"));
            Assert.AreEqual(null, ChurchBoolean.False(1)(null));
            object @object = new object();
            Assert.AreEqual(@object, ChurchBoolean.False(null)(@object));
        }

        static readonly Boolean True = ChurchBoolean.True;

        static readonly Boolean False = ChurchBoolean.False;

        [TestMethod]
        public void NotTest()
        {
            Assert.AreEqual(!true, True.Not()._Unchurch());
            Assert.AreEqual(!false, False.Not()._Unchurch());
        }

        [TestMethod]
        public void AndTest()
        {
            Assert.AreEqual(true && true, True.And(True)._Unchurch());
            Assert.AreEqual(true && false, True.And(False)._Unchurch());
            Assert.AreEqual(false && true, False.And(True)._Unchurch());
            Assert.AreEqual(false && false, False.And(False)._Unchurch());
        }

        [TestMethod]
        public void OrTest()
        {
            Assert.AreEqual(true || true, True.Or(True)._Unchurch());
            Assert.AreEqual(true || false, True.Or(False)._Unchurch());
            Assert.AreEqual(false || true, False.Or(True)._Unchurch());
            Assert.AreEqual(false || false, False.Or(False)._Unchurch());
        }

        [TestMethod]
        public void XorTest()
        {
            Assert.AreEqual(true ^ true, True.Xor(True)._Unchurch());
            Assert.AreEqual(true ^ false, True.Xor(False)._Unchurch());
            Assert.AreEqual(false ^ true, False.Xor(True)._Unchurch());
            Assert.AreEqual(false ^ false, False.Xor(False)._Unchurch());
        }

[TestMethod]
public void IfTest()
{
    Assert.AreEqual(
        true ? true && false : true || false,
        ChurchBoolean.If<Boolean>(True)(_ => True.And(False))(_ => True.Or(False))._Unchurch());
    Assert.AreEqual(
        false ? true && false : true || false,
        ChurchBoolean.If<Boolean>(False)(_ => True.And(False))(_ => True.Or(False))._Unchurch());

    bool isTrueBranchExecuted = false;
    bool isFalseBranchExecuted = false;
    ChurchBoolean.If<object>(True)
        (_ => { isTrueBranchExecuted = true; return null; })
        (_ => { isFalseBranchExecuted = true; return null; });
    Assert.IsTrue(isTrueBranchExecuted);
    Assert.IsFalse(isFalseBranchExecuted);

    isTrueBranchExecuted = false;
    isFalseBranchExecuted = false;
    ChurchBoolean.If<object>(False)
        (_ => { isTrueBranchExecuted = true; return null; })
        (_ => { isFalseBranchExecuted = true; return null; });
    Assert.IsFalse(isTrueBranchExecuted);
    Assert.IsTrue(isFalseBranchExecuted);
}
    }
}

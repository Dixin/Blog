namespace Tutorial.Tests.LambdaCalculus
{
    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Tutorial.LambdaCalculus.ChurchBoolean;

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
            Assert.AreEqual((!true).Church(), True.Not());
            Assert.AreEqual((!false).Church(), False.Not());
        }

        [TestMethod]
        public void AndTest()
        {
            Assert.AreEqual((true && true).Church(), True.And(True));
            Assert.AreEqual((true && false).Church(), True.And(False));
            Assert.AreEqual((false && true).Church(), False.And(True));
            Assert.AreEqual((false && false).Church(), False.And(False));
        }

        [TestMethod]
        public void OrTest()
        {
            Assert.AreEqual((true || true).Church(), True.Or(True));
            Assert.AreEqual((true || false).Church(), True.Or(False));
            Assert.AreEqual((false || true).Church(), False.Or(True));
            Assert.AreEqual((false || false).Church(), False.Or(False));
        }

        [TestMethod]
        public void XorTest()
        {
            Assert.AreEqual((true ^ true).Church(), True.Xor(True));
            Assert.AreEqual((true ^ false).Church(), True.Xor(False));
            Assert.AreEqual((false ^ true).Church(), False.Xor(True));
            Assert.AreEqual((false ^ false).Church(), False.Xor(False));
        }
    }

    public partial class ChurchBooleanTests
    {
        [TestMethod]
        public void IfTest()
        {
            Assert.AreEqual(
                (true ? true && false : true || false).Church(),
                If(True)(_ => True.And(False))(_ => True.Or(False)));
            Assert.AreEqual(
                (false ? true && false : true || false).Church(),
                If(False)(_ => True.And(False))(_ => True.Or(False)));

            bool isTrueBranchExecuted = false;
            bool isFalseBranchExecuted = false;
            If(True)
                (_ => { isTrueBranchExecuted = true; return null; })
                (_ => { isFalseBranchExecuted = true; return null; });
            Assert.IsTrue(isTrueBranchExecuted);
            Assert.IsFalse(isFalseBranchExecuted);

            isTrueBranchExecuted = false;
            isFalseBranchExecuted = false;
            If(False)
                (_ => { isTrueBranchExecuted = true; return null; })
                (_ => { isFalseBranchExecuted = true; return null; });
            Assert.IsFalse(isTrueBranchExecuted);
            Assert.IsTrue(isFalseBranchExecuted);
        }
    }
}

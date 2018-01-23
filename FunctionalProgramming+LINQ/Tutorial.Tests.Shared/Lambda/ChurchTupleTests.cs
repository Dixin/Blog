namespace Tutorial.Tests.LambdaCalculus
{
    using System;
    using Tutorial.LambdaCalculus;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchTupleTests
    {
        [TestMethod]
        public void CreateItem1Item2Test()
        {
            Tutorial.LambdaCalculus.Tuple<int, string> tuple1 = ChurchTuple<int, string>.Create(1)("a");
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Tutorial.LambdaCalculus.Tuple<string, int> tuple2 = ChurchTuple<string, int>.Create("a")(1);
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            object @object = new object();
            Tutorial.LambdaCalculus.Tuple<object, int> tuple3 = ChurchTuple<object, int>.Create(@object)(1);
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
        }

        [TestMethod]
        public void ShiftTest()
        {
            Tutorial.LambdaCalculus.Tuple<int, int> tuple1 = ChurchTuple<int, int>.Create(1)(2).Shift(_ => _);
            Assert.AreEqual(2, tuple1.Item1());
            Assert.AreEqual(2, tuple1.Item2());
            Tutorial.LambdaCalculus.Tuple<int, int> tuple2 = ChurchTuple<int, int>.Create(2)(3).Shift(value => value * 2);
            Assert.AreEqual(3, tuple2.Item1());
            Assert.AreEqual(6, tuple2.Item2());
            Tutorial.LambdaCalculus.Tuple<string, string> tuple3 = ChurchTuple<string, string>.Create("a")("b").Shift(value => value + "c");
            Assert.AreEqual("b", tuple3.Item1());
            Assert.AreEqual("bc", tuple3.Item2());
        }

        [TestMethod]
        public void SwapTest()
        {
            Tutorial.LambdaCalculus.Tuple<int, string> tuple1 = ChurchTuple<string, int>.Create("a")(1).Swap();
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Tutorial.LambdaCalculus.Tuple<string, int> tuple2 = ChurchTuple<int, string>.Create(1)("a").Swap();
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            object @object = new object();
            Tutorial.LambdaCalculus.Tuple<object, int> tuple3 = ChurchTuple<int, object>.Create(1)(@object).Swap();
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
        }

        [TestMethod]
        public void ReuseAnonymousTypeTest()
        {
            var anna = new { Name = "Anna", Age = 18 };
            var bill = new { Name = "Bill", Age = 19 };
            Assert.AreSame(anna.GetType(), bill.GetType()); // Passes.
        }
    }

    [TestClass]
    public class Church3TupleTests
    {
        [TestMethod]
        public void CreateItem1Item2Test()
        {
            Tutorial.LambdaCalculus.Tuple<int, string, bool> tuple1 = ChurchTuple<int, string, bool>.Create(1)("a")(true);
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Assert.AreEqual(true, tuple1.Item3());
            Tutorial.LambdaCalculus.Tuple<string, int, bool> tuple2 = ChurchTuple<string, int, bool>.Create("a")(1)(false);
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            Assert.AreEqual(false, tuple2.Item3());
            object @object = new object();
            Uri uri = new Uri("https://weblogs.asp.net/dixin");
            Tutorial.LambdaCalculus.Tuple<object, int, Uri> tuple3 = ChurchTuple<object, int, Uri>.Create(@object)(1)(uri);
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
            Assert.AreEqual(uri, tuple3.Item3());
        }
    }
}

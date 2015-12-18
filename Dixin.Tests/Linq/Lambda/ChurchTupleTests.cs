namespace Dixin.Linq.Lambda.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChurchTupleTests
    {
        [TestMethod]
        public void CreateItem1Item2Test()
        {
            Tuple<int, string> tuple1 = ChurchTuple.Create<int, string>(1)("a");
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Tuple<string, int> tuple2 = ChurchTuple.Create<string, int>("a")(1);
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            object @object = new object();
            Tuple<object, int> tuple3 = ChurchTuple.Create<object, int>(@object)(1);
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
        }

        [TestMethod]
        public void ShiftTest()
        {
            Tuple<int, int> tuple1 = ChurchTuple.Create<int, int>(1)(2).Shift(_ => _);
            Assert.AreEqual(2, tuple1.Item1());
            Assert.AreEqual(2, tuple1.Item2());
            Tuple<int, int> tuple2 = ChurchTuple.Create<int, int>(2)(3).Shift(value => value * 2);
            Assert.AreEqual(3, tuple2.Item1());
            Assert.AreEqual(6, tuple2.Item2());
            Tuple<string, string> tuple3 = ChurchTuple.Create<string, string>("a")("b").Shift(value => value + "c");
            Assert.AreEqual("b", tuple3.Item1());
            Assert.AreEqual("bc", tuple3.Item2());
        }

        [TestMethod]
        public void SwapTest()
        {
            Tuple<int, string> tuple1 = ChurchTuple.Create<string, int>("a")(1).Swap();
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Tuple<string, int> tuple2 = ChurchTuple.Create<int, string>(1)("a").Swap();
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            object @object = new object();
            Tuple<object, int> tuple3 = ChurchTuple.Create<int, object>(1)(@object).Swap();
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
        }

        [TestMethod]
        public void _CreateTest()
        {
            Tuple<int, string> tuple1 = ChurchTuple._Create(1, "a");
            Assert.AreEqual(1, tuple1.Item1());
            Assert.AreEqual("a", tuple1.Item2());
            Tuple<string, int> tuple2 = ChurchTuple._Create("a", 1);
            Assert.AreEqual("a", tuple2.Item1());
            Assert.AreEqual(1, tuple2.Item2());
            object @object = new object();
            Tuple<object, int> tuple3 = ChurchTuple._Create(@object, 1);
            Assert.AreEqual(@object, tuple3.Item1());
            Assert.AreEqual(1, tuple3.Item2());
        }

        [TestMethod]
        public void ReuseAnonymousType()
        {
            var anna = new { Name = "Anna", Age = 18 };
            var bill = new { Name = "Bill", Age = 19 };
            Assert.AreSame(anna.GetType(), bill.GetType()); // Passes.
        }

        [TestMethod]
        public void AnonymousObjectEquality()
        {
            Assert.AreEqual(
                new { Name = "Dixin", Age = 30 },
                new { Name = "Dixin", Age = 30 }); // Passes.
        }
    }
}

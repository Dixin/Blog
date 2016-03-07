namespace Dixin.Tests.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using Dixin.Linq;
    using Dixin.Linq.CategoryTheory;
    using Dixin.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using FuncExtensions = Dixin.Linq.CategoryTheory.FuncExtensions;
    using NullableInt32 = Dixin.Linq.CategoryTheory.Nullable<int>;

    [TestClass]
    public partial class MonoidalFunctorTests
    {
        [TestMethod]
        public void EnumerableTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            IEnumerable<int> numbers = new int[] { 0, 1, 2 };
            IEnumerable<int> query = addOne.Enumerable().Apply(numbers);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3 }, query); // Execution.
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            EnumerableAssert.AreSequentialEqual(addOne.Enumerable().Apply(numbers), numbers.Select(addOne));
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            EnumerableAssert.AreSequentialEqual(id.Enumerable().Apply(numbers), numbers);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            EnumerableAssert.AreSequentialEqual(
                o.Enumerable().Apply(addOne.Enumerable()).Apply(addTwo.Enumerable()).Apply(numbers),
                addOne.Enumerable().Apply(addTwo.Enumerable().Apply(numbers)));
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            EnumerableAssert.AreSequentialEqual(addOne.Enumerable().Apply(1.Enumerable()), addOne(1).Enumerable());
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            EnumerableAssert.AreSequentialEqual(
                addOne.Enumerable().Apply(1.Enumerable()),
                new Func<Func<int, int>, int>(f => f(1)).Enumerable().Apply(addOne.Enumerable()));
        }

        [TestMethod]
        public void EnumerableTest2()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            IEnumerable<int> numbers = new int[] { 0, 1, 2 };
            IEnumerable<Func<int, int>> addTwoAddOne = new Func<int, int>(
                x => { isExecuted2 = true; return x + 2; }).Enumerable().Concat(addOne.Enumerable());
            IEnumerable<int> query = addTwoAddOne.Apply(numbers);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 2, 3, 4, 1, 2, 3 }, query); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }
    }

    public partial class MonoidalFunctorTests
    {
        [TestMethod]
        public void EnumerableApplyWithZipTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            IEnumerable<int> numbers = new int[] { 0, 1, 2, 3 };
            IEnumerable<Func<int, int>> addTwoAddOne = new Func<int, int>(
                x => { isExecuted2 = true; return x + 2; }).Enumerable().Concat(addOne.Enumerable());
            IEnumerable<int> query = addTwoAddOne.ApplyWithZip(numbers);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 2, 3, 4, 5, 1, 2, 3, 4 }, query); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }

        [TestMethod]
        public void EnumerableApplyWithJoinTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            IEnumerable<int> numbers = new int[] { 0, 1, 2 };
            IEnumerable<Func<int, int>> addTwoAddOne = new Func<int, int>(
                x => { isExecuted2 = true; return x + 2; }).Enumerable().Concat(addOne.Enumerable());
            IEnumerable<int> query = addTwoAddOne.ApplyWithJoin(numbers);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 2, 3, 4, 1, 2, 3 }, query); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }

        [TestMethod]
        public void EnumerableApplyWithLinqJoinTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            IEnumerable<int> numbers = new int[] { 0, 1, 2 };
            IEnumerable<Func<int, int>> functions = new Func<int, int>(
                x => { isExecuted2 = true; return x + 2; }).Enumerable().Concat(addOne.Enumerable());
            IEnumerable<int> query = functions.ApplyWithLinqJoin(numbers);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 2, 3, 4, 1, 2, 3 }, query); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }
    }

    public partial class MonoidalFunctorTests
    {
        [TestMethod]
        public void LazyTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Lazy<int> query1 = addOne.Lazy().Apply(2.Lazy());
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual(2 + 1, query1.Value); // Execution.
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(addOne.Lazy().Apply(1.Lazy()).Value, 1.Lazy().Select(addOne).Value);
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(id.Lazy().Apply(1.Lazy()).Value, 1.Lazy().Value);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            Lazy<int> left1 = o.Lazy().Apply(addOne.Lazy()).Apply(addTwo.Lazy()).Apply(1.Lazy());
            Lazy<int> right1 = addOne.Lazy().Apply(addTwo.Lazy().Apply(1.Lazy()));
            Assert.AreEqual(left1.Value, right1.Value);
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(addOne.Lazy().Apply(1.Lazy()).Value, addOne(1).Lazy().Value);
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            Lazy<int> left2 = addOne.Lazy().Apply(1.Lazy());
            Lazy<int> right2 = new Func<Func<int, int>, int>(f => f(1)).Lazy().Apply(addOne.Lazy());
            Assert.AreEqual(left2.Value, right2.Value);
        }

        [TestMethod]
        public void FuncTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Func<int> query1 = FuncExtensions.Func(addOne).Apply(2.Func());
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual(addOne(2), query1()); // Execution.
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(FuncExtensions.Func(addOne).Apply(1.Func())(), 1.Func().Select(addOne)());
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(FuncExtensions.Func(id).Apply(1.Func())(), 1.Func()());
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            Func<int> left1 = FuncExtensions.Func(o).Apply(FuncExtensions.Func(addOne)).Apply(FuncExtensions.Func(addTwo)).Apply(1.Func());
            Func<int> right1 = FuncExtensions.Func(addOne).Apply(FuncExtensions.Func(addTwo).Apply(1.Func()));
            Assert.AreEqual(left1(), right1());
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(FuncExtensions.Func(addOne).Apply(1.Func())(), addOne(1).Func()());
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            Func<int> left2 = FuncExtensions.Func(addOne).Apply(1.Func());
            Func<int> right2 = FuncExtensions.Func(new Func<Func<int, int>, int>(f => f(1))).Apply(FuncExtensions.Func(addOne));
            Assert.AreEqual(left2(), right2());
        }

        [TestMethod]
        public void FuncTest2()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, string>> add = x => y =>
                { isExecuted1 = true; return (x + y).ToString(CultureInfo.InvariantCulture); };
            Func<string> query2 = FuncExtensions.Func(add).Apply(1.Func()).Apply(2.Func());
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual(add(1)(2), query2()); // Execution.
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(FuncExtensions.Func(add).Apply(1.Func())()(2), 1.Func().Select(add)()(2));
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(FuncExtensions.Func(id).Apply(1.Func())(), 1.Func()());
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<Func<int, string>, Func<int, int>> length = f => x => f(x).Length;
            Func<Func<Func<int, string>, Func<int, int>>, Func<Func<int, Func<int, string>>, Func<int, Func<int, int>>>> o =
                new Func<Func<Func<int, string>, Func<int, int>>, Func<int, Func<int, string>>, Func<int, Func<int, int>>>(FuncExtensions.o).Curry();
            Func<Func<int, int>> left1 = FuncExtensions.Func(o).Apply(FuncExtensions.Func(length)).Apply(FuncExtensions.Func(add)).Apply(1.Func());
            Func<Func<int, int>> right1 = FuncExtensions.Func(length).Apply(FuncExtensions.Func(add).Apply(1.Func()));
            Assert.AreEqual(left1()(2), right1()(2));
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(FuncExtensions.Func(add).Apply(1.Func())()(2), FuncExtensions.Func(add(1))()(2));
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            Func<Func<int, string>> left2 = FuncExtensions.Func(add).Apply(1.Func());
            Func<Func<int, string>> right2 = FuncExtensions.Func(new Func<Func<int, Func<int, string>>, Func<int, string>>(
                    f => f(1))).Apply(FuncExtensions.Func(add));
            Assert.AreEqual(left2()(2), right2()(2));

            bool isExecuted3 = false;
            Func<string> consoleReadLine1 = () => "a";
            Func<string> consoleReadLine2 = () => "b";
            Func<string, Func<string, string>> concat = x => y =>
                { isExecuted3 = true; return string.Concat(x, y); };
            Func<string> concatLines = FuncExtensions.Func(concat).Apply(consoleReadLine1).Apply(consoleReadLine2);
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.AreEqual(string.Concat(consoleReadLine1(), consoleReadLine2()), concatLines());
            Assert.IsTrue(isExecuted3);
        }

        [TestMethod]
        public void NullableTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            NullableInt32 query1 = addOne.Nullable().Apply(2.Nullable());
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsTrue(query1.HasValue); // Execution.
            Assert.AreEqual(addOne(2), query1.Value);
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(addOne.Nullable().Apply(1.Nullable()).Value, 1.Nullable().Select(addOne).Value);
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(id.Nullable().Apply(1.Nullable()).Value, 1.Nullable().Value);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            NullableInt32 left1 = o.Nullable().Apply(addOne.Nullable()).Apply(addTwo.Nullable()).Apply(1.Nullable());
            NullableInt32 right1 = addOne.Nullable().Apply(addTwo.Nullable().Apply(1.Nullable()));
            Assert.AreEqual(left1.Value, right1.Value);
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(addOne.Nullable().Apply(1.Nullable()).Value, addOne(1).Nullable().Value);
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            NullableInt32 left2 = addOne.Nullable().Apply(1.Nullable());
            NullableInt32 right2 = new Func<Func<int, int>, int>(f => f(1)).Nullable().Apply(addOne.Nullable());
            Assert.AreEqual(left2.Value, right2.Value);

            bool isExecuted2 = false;
            Func<int, int> addTwo2 = x => { isExecuted2 = true; return x + 2; };
            NullableInt32 query2 = addTwo2.Nullable().Apply(new NullableInt32());
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(query2.HasValue); // Execution.
            Assert.IsFalse(isExecuted2);
        }
    }

    public partial class MonoidalFunctorTests
    {
        [TestMethod]
        public void TupleTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Tuple<int> query1 = addOne.Tuple().Apply(2.Tuple());
            Assert.IsTrue(isExecuted1); // Immediate execution.
            Assert.AreEqual(2 + 1, query1.Item1); // Execution.
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(addOne.Tuple().Apply(1.Tuple()).Item1, 1.Tuple().Select(addOne).Item1);
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(id.Tuple().Apply(1.Tuple()).Item1, 1.Tuple().Item1);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            Tuple<int> left1 = o.Tuple().Apply(addOne.Tuple()).Apply(addTwo.Tuple()).Apply(1.Tuple());
            Tuple<int> right1 = addOne.Tuple().Apply(addTwo.Tuple().Apply(1.Tuple()));
            Assert.AreEqual(left1.Item1, right1.Item1);
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(addOne.Tuple().Apply(1.Tuple()).Item1, addOne(1).Tuple().Item1);
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            Tuple<int> left2 = addOne.Tuple().Apply(1.Tuple());
            Tuple<int> right2 = new Func<Func<int, int>, int>(f => f(1)).Tuple().Apply(addOne.Tuple());
            Assert.AreEqual(left2.Item1, right2.Item1);
        }

        [TestMethod]
        public void HotTaskTest()
        {
            bool isExecuted1 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Task<Func<int, int>> hotAddOne = Task.Run(() => addOne);
            Task<int> hotTwo = Task.Run(() => 2);
            Task<int> query1 = hotAddOne.Apply(hotTwo);
            Assert.AreEqual(2 + 1, query1.Result);
            Assert.IsTrue(isExecuted1);

            // f.Functor().Apply(F) == F.Select(f)
            Assert.AreEqual(addOne.Task().Apply(1.Task()).Result, 1.Task().Select(addOne).Result);
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Assert.AreEqual(id.Task().Apply(1.Task()).Result, 1.Task().Result);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            Func<int, int> addTwo = x => x + 2;
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            Task<int> left1 = o.Task().Apply(addOne.Task()).Apply(addTwo.Task()).Apply(1.Task());
            Task<int> right1 = addOne.Task().Apply(addTwo.Task().Apply(1.Task()));
            Assert.AreEqual(left1.Result, right1.Result);
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            Assert.AreEqual(addOne.Task().Apply(1.Task()).Result, addOne(1).Task().Result);
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            Task<int> left2 = addOne.Task().Apply(1.Task());
            Task<int> right2 = new Func<Func<int, int>, int>(f => f(1)).Task().Apply(addOne.Task());
            Assert.AreEqual(left2.Result, right2.Result);
        }

        [TestMethod]
        public void ColdTaskTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Task<Func<int, int>> coldAddOne = new Task<Func<int, int>>(() => addOne);
            Task<int> coldTwo = new Task<int>(() => { isExecuted2 = true; return 2; });
            Task<int> query2 = coldAddOne.Apply(coldTwo);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            coldAddOne.Start(); // Execution.
            coldTwo.Start(); // Execution.
            Assert.AreEqual(2 + 1, query2.Result);
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // f.Functor().Apply(F) == F.Select(f)
            coldAddOne = new Task<Func<int, int>>(() => addOne);
            coldTwo = new Task<int>(() => 2);
            Task<int> left = coldAddOne.Apply(coldTwo);
            Task<int> right = coldTwo.Select(addOne);
            coldAddOne.Start();
            coldTwo.Start();
            Assert.AreEqual(left.Result, right.Result);
            // id.Functor().Apply(F) == F
            Func<int, int> id = Functions.Id;
            Task<Func<int, int>> coldId = new Task<Func<int, int>>(() => id);
            coldTwo = new Task<int>(() => 2);
            left = coldId.Apply(coldTwo);
            right = coldTwo;
            coldId.Start();
            coldTwo.Start();
            Assert.AreEqual(left.Result, right.Result);
            // o.Functor().Apply(F1).Apply(F2).Apply(F3) == F1.Apply(F2.Apply(F3))
            coldAddOne = new Task<Func<int, int>>(() => addOne);
            Func<int, int> addTwo = x => x + 2;
            Task<Func<int, int>> coldAddTwo = new Task<Func<int, int>>(() => addTwo);
            Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>> o =
                new Func<Func<int, int>, Func<int, int>, Func<int, int>>(FuncExtensions.o).Curry();
            Task<Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>>> coldComposite =
                new Task<Func<Func<int, int>, Func<Func<int, int>, Func<int, int>>>>(() => o);
            coldTwo = new Task<int>(() => 2);
            left = coldComposite.Apply(coldAddOne).Apply(coldAddTwo).Apply(coldTwo);
            right = coldAddOne.Apply(coldAddTwo.Apply(coldTwo));
            coldComposite.Start();
            coldAddOne.Start();
            coldAddTwo.Start();
            coldTwo.Start();
            Assert.AreEqual(left.Result, right.Result);
            // f.Functor().Apply(a.Functor()) == f(a).Functor()
            coldAddOne = new Task<Func<int, int>>(() => addOne);
            coldTwo = new Task<int>(() => 2);
            left = coldAddOne.Apply(coldTwo);
            right = new Task<int>(() => addOne(2));
            coldAddOne.Start();
            coldTwo.Start();
            right.Start();
            Assert.AreEqual(left.Result, right.Result);
            // F.Apply(a.Functor()) == (f => f(a)).Functor().Apply(F)
            coldAddOne = new Task<Func<int, int>>(() => addOne);
            coldTwo = new Task<int>(() => 2);
            left = coldAddOne.Apply(coldTwo);
            Task<Func<Func<int, int>, int>> coldApplyTwo =
                new Task<Func<Func<int, int>, int>>(() => f => f(2));
            right = coldApplyTwo.Apply(coldAddOne);
            coldAddOne.Start();
            coldTwo.Start();
            coldApplyTwo.Start();
            Assert.AreEqual(left.Result, right.Result);
        }
    }
}

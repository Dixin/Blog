namespace Tutorial.Tests.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.CategoryTheory;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class FunctorTests
    {
        [TestMethod]
        public void EnumerableGeneralTest()
        {
            IEnumerable<int> functor = new int[] { 0, 1, 2 };
            Func<int, int> addOne = x => x + 1;

            // Functor law 1: F.Select(Id) == Id(F)
            EnumerableAssert.AreSequentialEqual(functor.Select(Functions.Id), Functions.Id(functor));
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addTwo = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            EnumerableAssert.AreSequentialEqual(
                EnumerableExtensions.Select(addTwo.o(addOne)).Invoke(functor),
                EnumerableExtensions.Select(addTwo).o(EnumerableExtensions.Select(addOne)).Invoke(functor));
        }

        [TestMethod]
        public void EnumerableCSharpTest()
        {
            bool isExecuted1 = false;
            IEnumerable<int> enumerable = new int[] { 0, 1, 2 };
            Func<int, int> f1 = x => { isExecuted1 = true; return x + 1; };

            IEnumerable<int> query1 = from x in enumerable select f1(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.

            EnumerableAssert.AreSequentialEqual(new int[] { 1, 2, 3 }, query1); // Execution.
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            EnumerableAssert.AreSequentialEqual(enumerable.Select(Functions.Id), Functions.Id(enumerable));
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> f2 = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            EnumerableAssert.AreSequentialEqual(
                enumerable.Select(f2.o(f1)),
                enumerable.Select(f1).Select(f2));
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            EnumerableAssert.AreSequentialEqual(
                from value in enumerable select f2.o(f1)(value),
                from value in enumerable select f1(value) into value select f2(value));
        }

        [TestMethod]
        public void TupleTest()
        {
            bool isExecuted1 = false;
            ValueTuple<int> tuple = new ValueTuple<int>(0);
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };

            ValueTuple<int> query1 = from x in tuple select addOne(x); // Execution.
            Assert.IsTrue(isExecuted1); // Immediate execution.

            Assert.AreEqual(0 + 1, query1.Item1);
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(tuple.Select(Functions.Id).Item1, Functions.Id(tuple).Item1);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addTwo = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            ValueTuple<string> query2 = tuple.Select(addTwo.o(addOne));
            ValueTuple<string> query3 = tuple.Select(addOne).Select(addTwo);
            Assert.AreEqual(query2.Item1, query3.Item1);
        }

        [TestMethod]
        public void Tuple2Test()
        {
            bool isExecuted1 = false;
            (string, int) tuple = ("a", 0);
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };

            (string, int) query1 = from x in tuple select addOne(x); // Execution.
            Assert.IsTrue(isExecuted1); // Immediate execution.

            Assert.AreEqual("a", query1.Item1);
            Assert.AreEqual(0 + 1, query1.Item2);
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(tuple.Select(Functions.Id).Item1, Functions.Id(tuple).Item1);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addTwo = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            (string, string) query2 = tuple.Select(addTwo.o(addOne));
            (string, string) query3 = tuple.Select(addOne).Select(addTwo);
            Assert.AreEqual(query2.Item1, query3.Item1);
        }

        [TestMethod]
        public void LazyTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Lazy<int> lazy = new Lazy<int>(() => { isExecuted1 = true; return 0; });
            Func<int, int> addOne = x => { isExecuted2 = true; return x + 1; };

            Lazy<int> query1 = from x in lazy select addOne(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.

            Assert.AreEqual(0 + 1, query1.Value); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(lazy.Select(Functions.Id).Value, Functions.Id(lazy).Value);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addTwo = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            Lazy<string> query2 = lazy.Select(addTwo.o(addOne));
            Lazy<string> query3 = lazy.Select(addOne).Select(addTwo);
            Assert.AreEqual(query2.Value, query3.Value);
        }

        [TestMethod]
        public void FuncTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int> zero = () => { isExecuted1 = true; return 0; };
            Func<int, int> addOne = x => { isExecuted2 = true; return x + 1; };

            Func<int> query1 = from x in zero select addOne(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.

            Assert.AreEqual(0 + 1, query1()); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(zero.Select(Functions.Id)(), Functions.Id(zero)());
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addTwo = x => (x + 2).ToString(CultureInfo.InvariantCulture);
            Func<string> query2 = zero.Select(addTwo.o(addOne));
            Func<string> query3 = zero.Select(addOne).Select(addTwo);
            Assert.AreEqual(query2(), query3());
        }

        [TestMethod]
        public void Func2Test()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<int, int> addOne = x => { isExecuted1 = true; return x + 1; };
            Func<int, int> addTwo = x => { isExecuted2 = true; return x + 2; };

            Func<int, int> query1 = from x in addOne select addTwo(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.

            Assert.AreEqual(0 + 1 + 2, query1(0)); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(addOne.Select(Functions.Id)(1), Functions.Id(addOne)(1));
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, string> addThree = x => (x + 3).ToString(CultureInfo.InvariantCulture);
            Func<int, string> query2 = addOne.Select(addThree.o(addTwo));
            Func<int, string> query3 = addOne.Select(addTwo).Select(addThree);
            Assert.AreEqual(query2(2), query3(2));
        }

        [TestMethod]
        public void OptionalWithoutValueTest()
        {
            bool isExecuted1 = false;
            Func<int, string> append = x => { isExecuted1 = true; return x + "b"; };
            Optional<int> optional = new Optional<int>();

            Optional<string> query1 = from x in optional select append(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.

            Assert.IsFalse(query1.HasValue); // Execution.
            Assert.IsFalse(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(query1.Select(Functions.Id).HasValue, Functions.Id(query1).HasValue);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<string, int> length = x => x.Length;
            Optional<int> query2 = optional.Select(length.o(append));
            Optional<int> query3 = optional.Select(append).Select(length);
            Assert.AreEqual(query2.HasValue, query3.HasValue);
        }

        [TestMethod]
        public void OptionalWithValueTest()
        {
            bool isExecuted1 = false;
            Func<int, string> append = x => { isExecuted1 = true; return x + "b"; };
            Optional<int> optional = new Optional<int>(() => (true, 1));

            Optional<string> query1 = from x in optional select append(x);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.

            Assert.IsTrue(query1.HasValue); // Execution.
            Assert.AreEqual("1b", query1.Value);
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(query1.Select(Functions.Id).HasValue, Functions.Id(query1).HasValue);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<string, int> length = x => x.Length;
            Optional<int> query2 = optional.Select(length.o(append));
            Optional<int> query3 = optional.Select(append).Select(length);
            Assert.AreEqual(query2.Value, query3.Value);
        }

        [TestMethod]
        public void ContinuationTest()
        {
            Func<int, Cps<int, int>> factorialCps = null; // Must have.
            factorialCps = x => x == 0
                ? 1.Cps<int, int>()
                : (from y in factorialCps(x - 1)
                   select x * y);
            Func<int, int> factorial = factorialCps.NoCps();
            Assert.AreEqual(3 * 2 * 1, factorial(3));

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(factorialCps(3).Select(Functions.Id).Invoke(), Functions.Id(factorialCps(3)).Invoke());
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<int, int> addOne = x => x + 1;
            Func<int, int> addTwo = x => x + 2;
            Cps<int, int> cps1 = factorialCps(3).Select(addTwo.o(addOne));
            Cps<int, int> cps2 = factorialCps(3).Select(addOne).Select(addTwo);
            Assert.AreEqual(cps1.Invoke(), cps2.Invoke());
        }

#if !ANDROID && !__IOS__
        [TestMethod]
#endif
        public void HotTaskTest()
        {
            bool isExecuted1 = false;
            Task<string> hotTask = Task.Run(() => "a"); // ANDROID and IOS: hotTask.Status is WaitingToRun.
            Func<string, string> append = x => { isExecuted1 = true; return x + "b"; };

            Task<string> query1 = from x in hotTask select append(x);
            Assert.AreEqual("a" + "b", query1.Result);
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(hotTask.Select(Functions.Id).Result, Functions.Id(hotTask).Result);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            Func<string, int> length = x => x.Length;
            Task<int> query2 = hotTask.Select(length.o(append));
            Task<int> query3 = hotTask.Select(append).Select(length);
            Assert.AreEqual(query2.Result, query3.Result);
        }

#if !ANDROID && !__IOS__
        [TestMethod]
#endif
        public void ColdTaskTest()
        {
            bool isExecuted2 = false;
            bool isExecuted1 = false;
            Task<string> coldTask = new Task<string>(() => { isExecuted2 = true; return "c"; });
            Func<string, string> append = x => { isExecuted1 = true; return x + "d"; };

            Task<string> query1 = from x in coldTask select append(x);
            Assert.IsFalse(isExecuted2);
            Assert.IsFalse(isExecuted1);

            coldTask.Start();
            Assert.AreEqual("c" + "d", query1.Result);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted1);

            // Functor law 1: F.Select(Id) == Id(F)
            Assert.AreEqual(coldTask.Select(Functions.Id).Result, Functions.Id(coldTask).Result);
            // Functor law 2: F.Select(f2.o(f1)) == F.Select(f1).Select(f2)
            coldTask = new Task<string>(() => "c");
            Func<string, int> length = x => x.Length;
            Task<int> query2 = coldTask.Select(length.o(append));
            Task<int> query3 = coldTask.Select(append).Select(length);
            coldTask.Start();
            Assert.AreEqual(query2.Result, query3.Result);
        }
    }
}

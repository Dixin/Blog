namespace Tutorial.Tests.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using Tutorial.CategoryTheory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Tutorial.CategoryTheory.Functions;

    using Enumerable = System.Linq.Enumerable;
    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;
    using FuncExtensions = Tutorial.CategoryTheory.FuncExtensions;
    using TaskExtensions = Tutorial.CategoryTheory.TaskExtensions;

    using static Tutorial.LinqToObjects.EnumerableX;

    [TestClass]
    public class MonadTests
    {
        [TestMethod]
        public void EnumerableMonoidTest()
        {
            IEnumerable<int> enumerable = new int[] { 0, 1, 2, 3, 4 };
            // Left unit preservation: Unit(f).Multiply() == f.
            EnumerableAssert.AreSequentialEqual(
                EnumerableExtensions.Unit(enumerable).Multiply(),
                enumerable);
            // Right unit preservation: f == f.Select(Unit).Multiply().
            EnumerableAssert.AreSequentialEqual(
                enumerable,
                enumerable.Select(EnumerableExtensions.Unit).Multiply());
            // Associativity preservation: f.Wrap().Multiply().Wrap().Multiply() == f.Wrap().Wrap().Multiply().Multiply().
            EnumerableAssert.AreSequentialEqual(
                enumerable.Enumerable().Multiply().Enumerable().Multiply(),
                enumerable.Enumerable().Enumerable().Multiply().Multiply());
        }

        [TestMethod]
        public void EnumerableTest()
        {
            bool isExecuted1 = false;
            IEnumerable<int> enumerable1 = new int[] { 0, 1 };
            IEnumerable<int> enumerable2 = new int[] { 1, 2 };
            Func<int, Func<int, int>> f = x => y => { isExecuted1 = true; return x + y; };
            IEnumerable<int> query1 = from x in enumerable1
                                      from y in enumerable2
                                      let z = f(x)(y)
                                      where z > 1
                                      select z;
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            EnumerableAssert.AreSequentialEqual(new int[] { 2, 2, 3 }, query1); // Execution.
            Assert.IsTrue(isExecuted1);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, IEnumerable<int>> addOne = x => (x + 1).Enumerable();
            EnumerableAssert.AreSequentialEqual(1.Enumerable().SelectMany(addOne, False), addOne(1));
            // Monad law 2: M.SelectMany(Monad) == M
            EnumerableAssert.AreSequentialEqual(enumerable1.SelectMany(EnumerableExtensions.Enumerable, False), enumerable1);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, IEnumerable<int>> addTwo = x => (x + 2).Enumerable();
            EnumerableAssert.AreSequentialEqual(
                enumerable2.SelectMany(addOne, False).SelectMany(addTwo, False),
                enumerable2.SelectMany(x => addOne(x).SelectMany(addTwo, False), False));
        }

        [TestMethod]
        public void LazyTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            bool isExecuted3 = false;
            Lazy<int> one = new Lazy<int>(() => { isExecuted1 = true; return 1; });
            Lazy<int> two = new Lazy<int>(() => { isExecuted2 = true; return 2; });
            Func<int, Func<int, int>> add = x => y => { isExecuted3 = true; return x + y; };
            Lazy<int> query = from x in one
                              from y in two
                              from _ in one
                              select add(x)(y);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.AreEqual(1 + 2, query.Value); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted3);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Lazy<int>> addOne = x => (x + 1).Lazy();
            Lazy<int> left = 1.Lazy().SelectMany(addOne, False);
            Lazy<int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 2: M.SelectMany(Monad) == M
            Lazy<int> M = 1.Lazy();
            left = M.SelectMany(LazyExtensions.Lazy, False);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Lazy<int>> addTwo = x => (x + 2).Lazy();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Value, right.Value);
        }

        [TestMethod]
        public void FuncTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            bool isExecuted3 = false;
            bool isExecuted4 = false;
            Func<int> f1 = () => { isExecuted1 = true; return 1; };
            Func<int> f2 = () => { isExecuted2 = true; return 2; };
            Func<int, int> f3 = x => { isExecuted3 = true; return x + 1; };
            Func<int, Func<int, int>> f4 = x => y => { isExecuted4 = true; return x + y; };
            Func<int> query1 = from x in f1
                               from y in f2
                               from z in f3.Partial(y)
                               from _ in "abc".Func()
                               let f4x = f4(x)
                               select f4x(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.IsFalse(isExecuted4); // Deferred and lazy.
            Assert.AreEqual(1 + 2 + 1, query1()); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted3);
            Assert.IsTrue(isExecuted4);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Func<int>> addOne = x => (x + 1).Func();
            Func<int> left = 1.Func().SelectMany(addOne, False);
            Func<int> right = addOne(1);
            Assert.AreEqual(left(), right());
            // Monad law 2: M.SelectMany(Monad) == M
            Func<int> M = 1.Func();
            left = M.SelectMany(FuncExtensions.Func, False);
            right = M;
            Assert.AreEqual(left(), right());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Func<int>> addTwo = x => (x + 2).Func();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left(), right());

            bool isExecuted5 = false;
            bool isExecuted6 = false;
            bool isExecuted7 = false;
            Func<int, int> f5 = x => { isExecuted5 = true; return x + 1; };
            Func<string, int> f6 = x => { isExecuted6 = true; return x.Length; };
            Func<int, Func<int, string>> f7 = x => y =>
            { isExecuted7 = true; return new string('a', x + y); };
            Func<int, Func<string, string>> query2 = a => b =>
                (from x in f5(a).Func()
                 from y in f6(b).Func()
                 from z in 0.Func()
                 select f7(x)(y))();
            Assert.IsFalse(isExecuted5); // Deferred and lazy.
            Assert.IsFalse(isExecuted6); // Deferred and lazy.
            Assert.IsFalse(isExecuted7); // Deferred and lazy.
            Assert.AreEqual(new string('a', 1 + 1 + "abc".Length), query2(1)("abc")); // Execution.
            Assert.IsTrue(isExecuted5);
            Assert.IsTrue(isExecuted6);
            Assert.IsTrue(isExecuted7);
        }

        [TestMethod]
        public void OptionalTest()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, string>> add = x => y =>
            { isExecuted1 = true; return (x + y).ToString(CultureInfo.InvariantCulture); };
            Optional<int> optional = new Optional<int>();
            Optional<int> optional2 = new Optional<int>();
            Optional<string> query1 = from x in optional
                                      from y in optional2
                                      from _ in optional
                                      select add(x)(y);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(query1.HasValue); // Execution.
            Assert.IsFalse(isExecuted1);

            bool isExecuted3 = false;
            bool isExecuted4 = false;
            bool isExecuted5 = false;
            add = x => y =>
            { isExecuted3 = true; return (x + y).ToString(CultureInfo.InvariantCulture); };
            Optional<int> one = new Optional<int>(() =>
                { isExecuted4 = true; return (true, 1); });
            Optional<int> two = new Optional<int>(() =>
                { isExecuted5 = true; return (true, 2); });
            Optional<string> query2 = from x in one
                                      from y in two
                                      from _ in one
                                      select add(x)(y);
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.IsFalse(isExecuted4); // Deferred and lazy.
            Assert.IsFalse(isExecuted5); // Deferred and lazy.
            Assert.IsTrue(query2.HasValue); // Execution.
            Assert.AreEqual("3", query2.Value);
            Assert.IsTrue(isExecuted3);
            Assert.IsTrue(isExecuted4);
            Assert.IsTrue(isExecuted5);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Optional<int>> addOne = x => (x + 1).Optional();
            Optional<int> left = 1.Optional().SelectMany(addOne, False);
            Optional<int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 2: M.SelectMany(Monad) == M
            Optional<int> M = 1.Optional();
            left = M.SelectMany(OptionalExtensions.Optional, False);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Optional<int>> addTwo = x => (x + 2).Optional();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Value, right.Value);
        }

        [TestMethod]
        public void TupleTest()
        {
            bool isExecuted = false;
            ValueTuple<int> one = new ValueTuple<int>(1);
            ValueTuple<int> two = new ValueTuple<int>(2);
            Func<int, Func<int, int>> add = x => y => { isExecuted = true; return x + y; };
            ValueTuple<int> query = from x in one
                                    from y in two
                                    from _ in one
                                    select add(x)(y);
            Assert.IsTrue(isExecuted); // Immediate execution.
            Assert.AreEqual(1 + 2, query.Item1); // Execution.

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, ValueTuple<int>> addOne = x => (x + 1).ValueTuple();
            ValueTuple<int> left = 1.ValueTuple().SelectMany(addOne, False);
            ValueTuple<int> right = addOne(1);
            Assert.AreEqual(left.Item1, right.Item1);
            // Monad law 2: M.SelectMany(Monad) == M
            ValueTuple<int> M = 1.ValueTuple();
            left = M.SelectMany(ValueTupleExtensions.ValueTuple, False);
            right = M;
            Assert.AreEqual(left.Item1, right.Item1);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, ValueTuple<int>> addTwo = x => (x + 2).ValueTuple();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Item1, right.Item1);
        }

        [TestMethod]
        public void HotTaskTest()
        {
            Task<string> a = Task.Run(() => "a");
            Task<string> b = Task.Run(() => "b");
            Func<string, Func<string, string>> concat = x => y => x + y;
            Task<string> query1 = from x in a
                                  from y in b
                                  from _ in a
                                  select concat(x)(y);
            Assert.AreEqual("a" + "b", query1.Result);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Task<int>> addOne = x => (x + 1).Task();
            Task<int> left = 1.Task().SelectMany(addOne, False);
            Task<int> right = addOne(1);
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 2: M.SelectMany(Monad) == M
            Task<int> M = 1.Task();
            left = M.SelectMany(TaskExtensions.Task, False);
            right = M;
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            M = 1.Task();
            Func<int, Task<int>> addTwo = x => (x + 2).Task();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Result, right.Result);
        }

        [TestMethod]
        public void ColdTaskTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            bool isExecuted3 = false;
            Task<string> a = new Task<string>(() => { isExecuted1 = true; return "a"; });
            Task<string> b = new Task<string>(() => { isExecuted2 = true; return "b"; });
            Func<string, Func<string, string>> concat = x => y => { isExecuted3 = true; return x + y; };
            Task<string> query = from x in a
                                 from y in b
                                 from _ in a
                                 select concat(x)(y);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            a.Start(); // Execution.
            b.Start(); // Execution.
            Assert.AreEqual("a" + "b", query.Result);
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted3);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            List<Task<int>> addOneTasks = new List<Task<int>>();
            Func<int, Task<int>> addOne = x =>
            {
                Task<int> task = new Task<int>(() => x + 1);
                addOneTasks.Add(task);
                return task;
            };
            Task<int> one = new Task<int>(() => 1);
            Task<int> left = one.SelectMany(addOne, False);
            Task<int> right = addOne(1);
            one.Start();
            while (addOneTasks.Count < 2) { }
            addOneTasks.ForEach(task => task.Start());
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 2: M.SelectMany(Monad) == M
            Task<int> M = new Task<int>(() => 1);
            left = M.SelectMany(TaskExtensions.Task, False);
            right = M;
            M.Start();
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            addOneTasks.Clear();
            List<Task<int>> addTwoTasks = new List<Task<int>>();
            M = new Task<int>(() => 1);
            Func<int, Task<int>> addTwo = x =>
            {
                Task<int> task = new Task<int>(() => x + 1);
                addTwoTasks.Add(task);
                return task;
            };
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            M.Start();
            while (addOneTasks.Count < 2) { }
            addOneTasks.ForEach(task => task.Start());
            while (addTwoTasks.Count < 2) { }
            addTwoTasks.ForEach(task => task.Start());
            Assert.AreEqual(left.Result, right.Result);
        }

        [TestMethod]
        public void IOTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            bool isExecuted3 = false;
            bool isExecuted4 = false;
            IO<int> one = () => { isExecuted1 = true; return 1; };
            IO<int> two = () => { isExecuted2 = true; return 2; };
            Func<int, IO<int>> addOne = x => { isExecuted3 = true; return (x + 1).IO(); };
            Func<int, Func<int, IO<int>>> add = x => y => { isExecuted4 = true; return (x + y).IO(); };
            IO<IO<int>> query1 = from x in one
                                 from y in two
                                 from z in addOne.Partial(y)()
                                 from _ in "abc".IO()
                                 let addOne2 = add(x)
                                 select addOne2(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.IsFalse(isExecuted4); // Deferred and lazy.
            Assert.AreEqual(1 + 2 + 1, query1()()); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted3);
            Assert.IsTrue(isExecuted4);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, IO<int>> addOne3 = x => (x + 1).IO();
            IO<int> left = 1.IO().SelectMany(addOne3, False);
            IO<int> right = addOne3(1);
            Assert.AreEqual(left(), right());
            // Monad law 2: M.SelectMany(Monad) == M
            IO<int> M = 1.IO();
            left = M.SelectMany(m => m.IO(), False);
            right = M;
            Assert.AreEqual(left(), right());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, IO<int>> addTwo = x => (x + 2).IO();
            left = M.SelectMany(addOne3, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne3(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left(), right());

            bool isExecuted5 = false;
            bool isExecuted6 = false;
            bool isExecuted7 = false;
            Func<int, IO<int>> addOne4 = x => { isExecuted5 = true; return (x + 1).IO(); };
            Func<string, IO<int>> length = x => { isExecuted6 = true; return x.Length.IO(); };
            Func<int, Func<int, IO<string>>> f7 = x => y =>
                { isExecuted7 = true; return new string('a', x + y).IO(); };
            Func<int, Func<string, IO<string>>> query2 = a => b => (from x in addOne4(a).IO()
                                                                    from y in length(b).IO()
                                                                    from z in 0.IO()
                                                                    select f7(x())(y()))();
            Assert.IsFalse(isExecuted5); // Deferred and lazy.
            Assert.IsFalse(isExecuted6); // Deferred and lazy.
            Assert.IsFalse(isExecuted7); // Deferred and lazy.
            Assert.AreEqual(new string('a', 1 + 1 + "abc".Length), query2(1)("abc")()); // Execution.
            Assert.IsTrue(isExecuted5);
            Assert.IsTrue(isExecuted6);
            Assert.IsTrue(isExecuted7);
        }

        [TestMethod]
        public void StateTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<State<string, int>> f1 = () => state => { isExecuted1 = true; return (1, state + "a"); };
            Func<int, Func<int, Func<string, int>>> f2 =
                x => y => z => { isExecuted2 = true; return x + y + z.Length; };
            State<string, int> query1 = from x in f1()
                                        from _ in StateExtensions.SetState(x.ToString(CultureInfo.InvariantCulture))
                                        from y in new State<string, int>(state => (2, "b" + state))
                                        from z in StateExtensions.GetState<string>()
                                        select f2(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            (int, string) result1 = query1("state"); // Execution.
            Assert.AreEqual(1 + 2 + ("b" + "1").Length, result1.Item1);
            Assert.AreEqual("b" + "1", result1.Item2);
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, State<string, int>> addOne = x => (x + 1).State<string, int>();
            State<string, int> left = 1.State<string, int>().SelectMany(addOne, False);
            State<string, int> right = addOne(1);
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
            // Monad law 2: M.SelectMany(Monad) == M
            State<string, int> M = 1.State<string, int>();
            left = M.SelectMany(StateExtensions.State<string, int>, False);
            right = M;
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, State<string, int>> addTwo = x => (x + 2).State<string, int>();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
        }

        [TestMethod]
        public void FactorialStateTest()
        {
            Func<uint, uint> factorial = null; // Must have to be compiled.
            factorial = x => x == 0 ? 1U : x * factorial(x - 1U);

            Assert.AreEqual(factorial(0), StateExtensions.Factorial(0));
            Assert.AreEqual(factorial(1), StateExtensions.Factorial(1));
            Assert.AreEqual(factorial(2), StateExtensions.Factorial(2));
            Assert.AreEqual(factorial(3), StateExtensions.Factorial(3));
            Assert.AreEqual(factorial(4), StateExtensions.Factorial(4));
            Assert.AreEqual(factorial(5), StateExtensions.Factorial(5));
            Assert.AreEqual(factorial(10), StateExtensions.Factorial(10));
        }

        [TestMethod]
        public void AggregateStateTest()
        {
            Assert.AreEqual(
                Enumerable.Aggregate(Enumerable.Range(0, 5), 0, (a, b) => a + b),
                StateExtensions.Aggregate(Enumerable.Range(0, 5), 0, (a, b) => a + b));
            Assert.AreEqual(
                Enumerable.Aggregate(Enumerable.Range(1, 5), 1, (a, b) => a + b),
                StateExtensions.Aggregate(Enumerable.Range(1, 5), 1, (a, b) => a + b));
        }

        [TestMethod]
        public void StateMachineTest()
        {
            IEnumerable<int> expected = Enumerable.Range(0, 5).Append(5).Skip(1);
            State<IEnumerable<int>, int> query = from unit in StateExtensions.PushState(5)
                                                 from value in StateExtensions.PopState<int>()
                                                 select value;
            EnumerableAssert.AreSequentialEqual(expected, query(Enumerable.Range(0, 5)).Item2);
        }

        [TestMethod]
        public void ReaderTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            bool isExecuted3 = false;
            bool isExecuted4 = false;
            Reader<int, int> f1 = x => { isExecuted1 = true; return x + 1; };
            Reader<int, string> f2 = x =>
                { isExecuted2 = true; return x.ToString(CultureInfo.InvariantCulture); };
            Func<string, Reader<int, int>> f3 = x => y => { isExecuted3 = true; return x.Length + y; };
            Func<int, Func<int, int>> f4 = x => y => { isExecuted4 = true; return x + y; };
            Reader<int, int> query1 = from x in f1
                                      from y in f2
                                      from z in f3(y)
                                      from _ in f1
                                      let f4x = f4(x)
                                      select f4x(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Assert.IsFalse(isExecuted3); // Deferred and lazy.
            Assert.IsFalse(isExecuted4); // Deferred and lazy.
            Assert.AreEqual(1 + 1 + 1 + 1, query1(1)); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
            Assert.IsTrue(isExecuted3);
            Assert.IsTrue(isExecuted4);

            Tuple<bool, string> config = Tuple.Create(true, "abc");
            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Reader<Tuple<bool, string>, int>> addOne = x => c => x + 1;
            Reader<Tuple<bool, string>, int> left = 1.Reader<Tuple<bool, string>, int>().SelectMany(addOne, False);
            Reader<Tuple<bool, string>, int> right = addOne(1);
            Assert.AreEqual(left(config), right(config));
            // Monad law 2: M.SelectMany(Monad) == M
            Reader<Tuple<bool, string>, int> M = c => 1 + c.Item2.Length;
            left = M.SelectMany(ReaderExtensions.Reader<Tuple<bool, string>, int>, False);
            right = M;
            Assert.AreEqual(left(config), right(config));
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Reader<Tuple<bool, string>, int>> addLength = x => c => x + c.Item2.Length;
            left = M.SelectMany(addOne, False).SelectMany(addLength, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addLength, False), False);
            Assert.AreEqual(left(config), right(config));
        }

        [TestMethod]
        public void WriterTest()
        {
            bool isExecuted1 = false;
            Func<int> f1 = () => 1;
            Func<int, Func<int, Func<string, int>>> f2 = x => y => z =>
                { isExecuted1 = true; return x + y + z.Length; };
            Writer<string, int> query = from x in f1().LogWriter("a")
                                        from y in 2.LogWriter("b")
                                        from z in "xyz".LogWriter("c")
                                        select f2(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual(1 + 2 + "xyz".Length, query.Value); // Execution.
            string[] logs = query.Content.ToArray();
            Assert.IsTrue(logs.Length == 3);
            Assert.IsTrue(logs[0].EndsWith("a"));
            Assert.IsTrue(logs[1].EndsWith("b"));
            Assert.IsTrue(logs[2].EndsWith("c"));
            Assert.IsTrue(isExecuted1);

            IMonoid<string> monoid = new StringConcatMonoid(); // (a, b) => string.Concat(a, b).
            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Writer<string, int>> addOne = x => (x + 1).LogWriter("a");
            Writer<string, int> left = 1.LogWriter("b").SelectMany(addOne, False);
            Writer<string, int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            EnumerableAssert.AreSequentialEqual(new string[] { "b", "a" }, left.Content.Select(log => log.Substring(log.Length - 1)));
            string rightLog = right.Content.Single();
            Assert.AreEqual("a", right.Content.Single().Split(' ').Last());
            // Monad law 2: M.SelectMany(Monad) == M
            Func<int, Writer<string, int>> Resturn = x => x.LogWriter("abc");
            Writer<string, int> M = Resturn(1);
            left = M.SelectMany(Resturn, False);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            EnumerableAssert.AreSequentialEqual(new string[] { "abc", "abc" }, left.Content.Select(log => log.Split(' ').Last()));
            Assert.AreEqual("abc", right.Content.Single().Split(' ').Last());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Writer<string, int>> addTwo = x => (x + 2).LogWriter("b");
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Value, right.Value);
            EnumerableAssert.AreSequentialEqual(new string[] { "abc", "a", "b" }, left.Content.Select(log => log.Split(' ').Last()));
            EnumerableAssert.AreSequentialEqual(new string[] { "abc", "a", "b" }, right.Content.Select(log => log.Split(' ').Last()));
        }

        [TestMethod]
        public void ContinuationTest()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, Func<string, string>>> f = x => y => z =>
                {
                    isExecuted1 = true;
                    return (x + y + z.Length).ToString(CultureInfo.InvariantCulture);
                };
            Cps<int, string> query = from x in 1.Cps<int, int>()
                                     from y in 2.Cps<int, int>()
                                     from z in "abc".Cps<int, string>()
                                     select f(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual((1 + 2 + "abc".Length).ToString(CultureInfo.InvariantCulture).Length, query(x => x.Length)); // Execution.
            Assert.IsTrue(isExecuted1);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Cps<int, int>> addOne = x => (x + 1).Cps<int, int>();
            Cps<int, int> left = 1.Cps<int, int>().SelectMany(addOne, False);
            Cps<int, int> right = addOne(1);
            Assert.AreEqual(left.Invoke(), right.Invoke());
            // Monad law 2: M.SelectMany(Monad) == M
            Cps<int, int> M = 1.Cps<int, int>();
            left = M.SelectMany(CpsExtensions.Cps<int, int>, False);
            right = M;
            Assert.AreEqual(left.Invoke(), right.Invoke());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Cps<int, int>> addTwo = x => (x + 2).Cps<int, int>();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Invoke(), right.Invoke());
        }

        [TestMethod]
        public void FibonacciContinuationTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.AreEqual(fibonacci(0U), CpsExtensions.FibonacciCps<uint>(0U)(Id));
            Assert.AreEqual(fibonacci(1U), CpsExtensions.FibonacciCps<uint>(1U)(Id));
            Assert.AreEqual(fibonacci(2U), CpsExtensions.FibonacciCps<uint>(2U)(Id));
            Assert.AreEqual(fibonacci(3U), CpsExtensions.FibonacciCps<uint>(3U)(Id));
            Assert.AreEqual(fibonacci(4U), CpsExtensions.FibonacciCps<uint>(4U)(Id));
            Assert.AreEqual(fibonacci(5U), CpsExtensions.FibonacciCps<uint>(5U)(Id));
            Assert.AreEqual(fibonacci(10U), CpsExtensions.FibonacciCps<uint>(10U)(Id));
        }

        [TestMethod]
        public void TryTest()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, Func<string, string>>> f = x => y => z =>
            {
                isExecuted1 = true;
                return (x + y + z.Length).ToString(CultureInfo.InvariantCulture);
            };
            Try<string> query = from x in 1.Try()
                                from y in 2.Try()
                                from z in "abc".Try()
                                select f(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual((1 + 2 + "abc".Length).ToString(CultureInfo.InvariantCulture), query.Value); // Execution.
            Assert.IsTrue(isExecuted1);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Try<int>> addOne = x => (x + 1).Try();
            Try<int> left = 1.Try().SelectMany(addOne, False);
            Try<int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 2: M.SelectMany(Monad) == M
            Try<int> M = 1.Try();
            left = M.SelectMany(TryExtensions.Try, False);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Try<int>> addTwo = x => (x + 2).Try();
            left = M.SelectMany(addOne, False).SelectMany(addTwo, False);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo, False), False);
            Assert.AreEqual(left.Value, right.Value);
        }

        [TestMethod]
        public void TryStrictFactorialTest()
        {
            Try<int> result = TryExtensions.TryStrictFactorial(null);
            Assert.IsTrue(result.HasException);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(ArgumentNullException));
            result = TryExtensions.TryStrictFactorial(0);
            Assert.IsTrue(result.HasException);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(ArgumentOutOfRangeException));
            result = TryExtensions.TryStrictFactorial(3);
            Assert.IsFalse(result.HasException);
            Assert.IsNull(result.Exception);
            Assert.AreEqual(6, result.Value);
            result = TryExtensions.TryStrictFactorial(-1);
            Assert.IsTrue(result.HasException);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(ArgumentOutOfRangeException));
        }

        [TestMethod]
        public void TryFactorialTest()
        {
            Try<string> result = TryExtensions.Factorial(null);
            Assert.IsFalse(result.HasException);
            Assert.IsNull(result.Exception);
            Assert.AreEqual("1", result.Value);
            result = TryExtensions.Factorial("0");
            Assert.IsFalse(result.HasException);
            Assert.IsNull(result.Exception);
            Assert.AreEqual("1", result.Value);
            result = TryExtensions.Factorial("3");
            Assert.IsFalse(result.HasException);
            Assert.IsNull(result.Exception);
            Assert.AreEqual("6", result.Value);
            result = TryExtensions.Factorial("-1");
            Assert.IsFalse(result.HasException);
            Assert.IsNull(result.Exception);
            StringAssert.Contains(result.Value, "Parameter name");
        }
    }
}
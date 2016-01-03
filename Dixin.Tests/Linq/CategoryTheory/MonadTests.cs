namespace Dixin.Linq.CategoryTheory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class MonadTests
    {
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
            EnumerableAssert.AreSequentialEqual(1.Enumerable().SelectMany(addOne), addOne(1));
            // Monad law 2: M.SelectMany(Monad) == M
            EnumerableAssert.AreSequentialEqual(enumerable1.SelectMany(EnumerableExtensions.Enumerable), enumerable1);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, IEnumerable<int>> addTwo = x => (x + 2).Enumerable();
            EnumerableAssert.AreSequentialEqual(
                enumerable2.SelectMany(addOne).SelectMany(addTwo),
                enumerable2.SelectMany(x => addOne(x).SelectMany(addTwo)));
        }
    }

    public partial class MonadTests
    {
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
            Lazy<int> left = 1.Lazy().SelectMany(addOne);
            Lazy<int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 2: M.SelectMany(Monad) == M
            Lazy<int> M = 1.Lazy();
            left = M.SelectMany(LazyExtensions.Lazy);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Lazy<int>> addTwo = x => (x + 2).Lazy();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
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
            Func<int> left = 1.Func().SelectMany(addOne);
            Func<int> right = addOne(1);
            Assert.AreEqual(left(), right());
            // Monad law 2: M.SelectMany(Monad) == M
            Func<int> M = 1.Func();
            left = M.SelectMany(FuncExtensions.Func);
            right = M;
            Assert.AreEqual(left(), right());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Func<int>> addTwo = x => (x + 2).Func();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
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
        public void NullableTest()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, string>> add = x => y =>
            { isExecuted1 = true; return (x + y).ToString(CultureInfo.InvariantCulture); };
            CategoryTheory.Nullable<int> nullable1 = new CategoryTheory.Nullable<int>();
            CategoryTheory.Nullable<int> nullable2 = new CategoryTheory.Nullable<int>();
            CategoryTheory.Nullable<string> query1 = from x in nullable1
                                                     from y in nullable2
                                                     from _ in nullable1
                                                     select add(x)(y);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(query1.HasValue); // Execution.
            Assert.IsFalse(isExecuted1);

            bool isExecuted3 = false;
            bool isExecuted4 = false;
            bool isExecuted5 = false;
            add = x => y =>
            { isExecuted3 = true; return (x + y).ToString(CultureInfo.InvariantCulture); };
            CategoryTheory.Nullable<int> one = new CategoryTheory.Nullable<int>(() =>
                { isExecuted4 = true; return Tuple.Create(true, 1); });
            CategoryTheory.Nullable<int> two = new CategoryTheory.Nullable<int>(() =>
                { isExecuted5 = true; return Tuple.Create(true, 2); });
            CategoryTheory.Nullable<string> query2 = from x in one
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
            Func<int, CategoryTheory.Nullable<int>> addOne = x => (x + 1).Nullable();
            CategoryTheory.Nullable<int> left = 1.Nullable().SelectMany(addOne);
            CategoryTheory.Nullable<int> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 2: M.SelectMany(Monad) == M
            CategoryTheory.Nullable<int> M = 1.Nullable();
            left = M.SelectMany(NullableExtensions.Nullable);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, CategoryTheory.Nullable<int>> addTwo = x => (x + 2).Nullable();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
            Assert.AreEqual(left.Value, right.Value);
        }
    }

    public partial class MonadTests
    {
        [TestMethod]
        public void TupleTest()
        {
            bool isExecuted = false;
            Tuple<int> one = new Tuple<int>(1);
            Tuple<int> two = new Tuple<int>(2);
            Func<int, Func<int, int>> add = x => y => { isExecuted = true; return x + y; };
            Tuple<int> query = from x in one
                               from y in two
                               from _ in one
                               select add(x)(y);
            Assert.IsTrue(isExecuted); // Immediate execution.
            Assert.AreEqual(1 + 2, query.Item1); // Execution.

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Tuple<int>> addOne = x => (x + 1).Tuple();
            Tuple<int> left = 1.Tuple().SelectMany(addOne);
            Tuple<int> right = addOne(1);
            Assert.AreEqual(left.Item1, right.Item1);
            // Monad law 2: M.SelectMany(Monad) == M
            Tuple<int> M = 1.Tuple();
            left = M.SelectMany(TupleExtensions.Tuple);
            right = M;
            Assert.AreEqual(left.Item1, right.Item1);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Tuple<int>> addTwo = x => (x + 2).Tuple();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
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
            Task<int> left = 1.Task().SelectMany(addOne);
            Task<int> right = addOne(1);
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 2: M.SelectMany(Monad) == M
            Task<int> M = 1.Task();
            left = M.SelectMany(CategoryTheory.TaskExtensions.Task);
            right = M;
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            M = 1.Task();
            Func<int, Task<int>> addTwo = x => (x + 2).Task();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
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
                Task<int> task = (x + 1).Task(true);
                addOneTasks.Add(task);
                return task;
            };
            Task<int> one = 1.Task(true);
            Task<int> left = one.SelectMany(addOne);
            Task<int> right = addOne(1);
            one.Start();
            while (addOneTasks.Count < 2) { }
            addOneTasks.ForEach(task => task.Start());
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 2: M.SelectMany(Monad) == M
            Task<int> M = 1.Task(true);
            left = M.SelectMany(CategoryTheory.TaskExtensions.Task);
            right = M;
            M.Start();
            Assert.AreEqual(left.Result, right.Result);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            addOneTasks.Clear();
            List<Task<int>> addTwoTasks = new List<Task<int>>();
            M = 1.Task(true);
            Func<int, Task<int>> addTwo = x =>
            {
                Task<int> task = (x + 1).Task(true);
                addTwoTasks.Add(task);
                return task;
            };
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
            M.Start();
            while (addOneTasks.Count < 2) { }
            addOneTasks.ForEach(task => task.Start());
            while (addTwoTasks.Count < 2) { }
            addTwoTasks.ForEach(task => task.Start());
            Assert.AreEqual(left.Result, right.Result);
        }
    }

    public partial class MonadTests
    {
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
            IO<int> left = 1.IO().SelectMany(addOne3);
            IO<int> right = addOne3(1);
            Assert.AreEqual(left(), right());
            // Monad law 2: M.SelectMany(Monad) == M
            IO<int> M = 1.IO();
            left = M.SelectMany(m => m.IO());
            right = M;
            Assert.AreEqual(left(), right());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, IO<int>> addTwo = x => (x + 2).IO();
            left = M.SelectMany(addOne3).SelectMany(addTwo);
            right = M.SelectMany(x => addOne3(x).SelectMany(addTwo));
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
    }

    public partial class MonadTests
    {
        [TestMethod]
        public void StateTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Func<State<int, string>> f1 = () => 1.State<int, string>(
                state => { isExecuted1 = true; return state + "a"; });
            Func<int, Func<int, Func<string, int>>> f2 =
                x => y => z => { isExecuted2 = true; return x + y + z.Length; };
            State<int, string> query1 = from x in f1()
                                        from _ in State.Set(x.ToString(CultureInfo.InvariantCulture))
                                        from y in 2.State<int, string>(state => "b" + state)
                                        from z in State.Get<string>()
                                        select f2(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.
            Lazy<int, string> result1 = query1("state"); // Execution.
            Assert.AreEqual(1 + 2 + ("b" + "1").Length, result1.Value1);
            Assert.AreEqual("b" + "1", result1.Value2);
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, State<int, string>> addOne = x => (x + 1).State<int, string>();
            State<int, string> left = 1.State<int, string>().SelectMany(addOne);
            State<int, string> right = addOne(1);
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
            // Monad law 2: M.SelectMany(Monad) == M
            State<int, string> M = 1.State<int, string>();
            left = M.SelectMany(StateExtensions.State<int, string>);
            right = M;
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, State<int, string>> addTwo = x => (x + 2).State<int, string>();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
            Assert.AreEqual(left.Value("a"), right.Value("a"));
            Assert.AreEqual(left.State("a"), right.State("a"));
        }
    }

    public partial class MonadTests
    {
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
            Reader<Tuple<bool, string>, int> left = 1.Reader<Tuple<bool, string>, int>().SelectMany(addOne);
            Reader<Tuple<bool, string>, int> right = addOne(1);
            Assert.AreEqual(left(config), right(config));
            // Monad law 2: M.SelectMany(Monad) == M
            Reader<Tuple<bool, string>, int> M = c => 1 + c.Item2.Length;
            left = M.SelectMany(ReaderExtensions.Reader<Tuple<bool, string>, int>);
            right = M;
            Assert.AreEqual(left(config), right(config));
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Reader<Tuple<bool, string>, int>> addLength = x => c => x + c.Item2.Length;
            left = M.SelectMany(addOne).SelectMany(addLength);
            right = M.SelectMany(x => addOne(x).SelectMany(addLength));
            Assert.AreEqual(left(config), right(config));
        }
    }
    public partial class MonadTests
    {
        [TestMethod]
        public void WriterTest()
        {
            bool isExecuted1 = false;
            Func<int> f1 = () => 1;
            Func<int, Func<int, Func<string, int>>> f2 = x => y => z =>
                { isExecuted1 = true; return x + y + z.Length; };
            Writer<int, IEnumerable<string>> query = from x in f1().WithLog("a")
                                                     from y in 2.WithLog("b")
                                                     from z in "xyz".WithLog("c")
                                                     select f2(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual(1 + 2 + "xyz".Length, query.Value); // Execution.
            string[] logs = query.Content.ToArray();
            Assert.IsTrue(logs.Length == 3);
            Assert.IsTrue(logs[0].EndsWith(" - a"));
            Assert.IsTrue(logs[1].EndsWith(" - b"));
            Assert.IsTrue(logs[2].EndsWith(" - c"));
            Assert.IsTrue(isExecuted1);

            IMonoid<string> monoid = string.Empty.Monoid(string.Concat); // (a, b) => string.Concat(a, b).
            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Writer<int, string>> addOne = x => (x + 1).Writer("a", monoid);
            Writer<int, string> left = 1.Writer("b", monoid).SelectMany(addOne);
            Writer<int, string> right = addOne(1);
            Assert.AreEqual(left.Value, right.Value);
            Assert.AreEqual("ba", left.Content);
            Assert.AreEqual("a", right.Content);
            // Monad law 2: M.SelectMany(Monad) == M
            Func<int, Writer<int, string>> Resturn = x => x.Writer("abc", monoid);
            Writer<int, string> M = Resturn(1);
            left = M.SelectMany(Resturn);
            right = M;
            Assert.AreEqual(left.Value, right.Value);
            Assert.AreEqual("abcabc", left.Content);
            Assert.AreEqual("abc", right.Content);
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Writer<int, string>> addTwo = x => (x + 2).Writer("b", monoid);
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
            Assert.AreEqual(left.Value, right.Value);
            Assert.AreEqual("abcab", left.Content);
            Assert.AreEqual("abcab", right.Content);
        }
    }
    public partial class MonadTests
    {
        [TestMethod]
        public void ContinuationTest()
        {
            bool isExecuted1 = false;
            Func<int, Func<int, Func<string, string>>> f = x => y => z =>
                {
                    isExecuted1 = true;
                    return (x + y + z.Length).ToString(CultureInfo.InstalledUICulture);
                };
            Cps<string, int> query = from x in 1.Cps<int, int>()
                                     from y in 2.Cps<int, int>()
                                     from z in "abc".Cps<string, int>()
                                     select f(x)(y)(z);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.AreEqual((1 + 2 + "abc".Length).ToString(CultureInfo.InstalledUICulture).Length, query(x => x.Length)); // Execution.
            Assert.IsTrue(isExecuted1);

            // Monad law 1: m.Monad().SelectMany(f) == f(m)
            Func<int, Cps<int, int>> addOne = x => (x + 1).Cps<int, int>();
            Cps<int, int> left = 1.Cps<int, int>().SelectMany(addOne);
            Cps<int, int> right = addOne(1);
            Assert.AreEqual(left.Invoke(), right.Invoke());
            // Monad law 2: M.SelectMany(Monad) == M
            Cps<int, int> M = 1.Cps<int, int>();
            left = M.SelectMany(CpsExtensions.Cps<int, int>);
            right = M;
            Assert.AreEqual(left.Invoke(), right.Invoke());
            // Monad law 3: M.SelectMany(f1).SelectMany(f2) == M.SelectMany(x => f1(x).SelectMany(f2))
            Func<int, Cps<int, int>> addTwo = x => (x + 2).Cps<int, int>();
            left = M.SelectMany(addOne).SelectMany(addTwo);
            right = M.SelectMany(x => addOne(x).SelectMany(addTwo));
            Assert.AreEqual(left.Invoke(), right.Invoke());
        }
    }
}
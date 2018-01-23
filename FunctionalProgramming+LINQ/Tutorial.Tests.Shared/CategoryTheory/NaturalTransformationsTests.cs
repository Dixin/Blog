namespace Tutorial.Tests.CategoryTheory
{
    using System;
    using System.Collections.Generic;

    using Tutorial.CategoryTheory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class NaturalTransformationsTests
    {
        [TestMethod]
        public void LazyToEnumerableTest()
        {
            Lazy<int> functor = new Lazy<int>(() => 1);
            IEnumerable<int> query1 = from x in functor.ToEnumerable()
                                      where x > 0
                                      select x;
            Assert.IsTrue(query1.Any());
            Assert.AreEqual(1, query1.Single());
            IEnumerable<int> query2 = from x in functor.ToEnumerable()
                                      where x < 0
                                      select x;
            Assert.IsFalse(query2.Any());
        }

        [TestMethod]
        public void FuncToEnumerableTest()
        {
            Func<int> functor = () => 1;
            IEnumerable<int> query1 = from x in functor.ToEnumerable()
                                      where x > 0
                                      select x;
            Assert.IsTrue(query1.Any());
            Assert.AreEqual(1, query1.Single());
            IEnumerable<int> query2 = from x in functor.ToEnumerable()
                                      where x < 0
                                      select x;
            Assert.IsFalse(query2.Any());
        }

        [TestMethod]
        public void OptionalToEnumerableTest()
        {
            Optional<int> functor = new Optional<int>(() => (true, 1));
            IEnumerable<int> query1 = from x in functor.ToEnumerable()
                                      where x > 0
                                      select x;
            Assert.IsTrue(query1.Any());
            Assert.AreEqual(1, query1.Single());
            IEnumerable<int> query2 = from x in functor.ToEnumerable()
                                      where x < 0
                                      select x;
            Assert.IsFalse(query2.Any());

            IEnumerable<int> query3 = from x in new Optional<int>().ToEnumerable()
                                      select x;
            Assert.IsFalse(query3.Any());
        }

        [TestMethod]
        public void CompositionTest()
        {
            Lazy<int> functor = new Lazy<int>(() => 1);
            Tuple<Func<Lazy<int>, IEnumerable<int>>, Func<Lazy<int>, IEnumerable<int>>> compositions = this.Compositions<int>();
            IEnumerable<int> x = compositions.Item1(functor);
            IEnumerable<int> y = compositions.Item2(functor);
            Assert.AreEqual(x.Single(), y.Single());
        }

        private Tuple<Func<Lazy<T>, IEnumerable<T>>, Func<Lazy<T>, IEnumerable<T>>> Compositions<T>()
        {
            Func<Lazy<T>, Func<T>> t1 = NaturalTransformations.ToFunc;
            Func<Func<T>, Optional<T>> t2 = NaturalTransformations.ToOptional;
            Func<Optional<T>, IEnumerable<T>> t3 = NaturalTransformations.ToEnumerable;
            Func<Lazy<T>, IEnumerable<T>> x = t3.o(t2).o(t1);
            Func<Lazy<T>, IEnumerable<T>> y = t3.o(t2.o(t1));
            return Tuple.Create(x, y);
        }
    }
}

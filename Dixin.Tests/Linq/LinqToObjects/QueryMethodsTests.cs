namespace Dixin.Tests.Linq.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.TeamFoundation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Dixin.Linq.LinqToObjects.QueryMethods;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void GenerationTest()
        {
            Empty();
            Range();
            LargeRange();
            DefaultIfEmpty();
            DefaultIfEmptyWithDefaultValue();
        }

        [TestMethod]
        public void Filtering()
        {
            Where();
            WhereWithIndex();
            OfType();
        }

        [TestMethod]
        public void MappingTest()
        {
            Select();
            SelectWithIndex();
            Let();
            SelectMany();
            SelectMany2();
            SelectManyFluent();
            SelectManyWithResultSelector2();
            SelectManyWithResultSelector();
        }

        [TestMethod]
        public void GroupingTest()
        {
            GroupBy();
            GroupByWithResultSelector();
            GroupBySelect();
            GroupBySelect2();
            GroupByWithElementSelector();
            GroupByWithElementAndResultSelector();
            GroupByWithEqualityComparer();
        }

        [TestMethod]
        public void JoinTest()
        {
            InnerJoin();
            InnerJoinWithSelectMany();
            InnerJoinWithMultipleKeys();
            LeftOuterJoin();
            LeftOuterJoinWithDefaultIfEmpty();
            LeftOuterJoinWithSelect();
            LeftOuterJoinWithSelect();
            CrossJoin();
            CrossJoinWithJoin();
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            Concat();
        }

        [TestMethod]
        public void SetTest()
        {
            Distinct();
            Union();
            Intersect();
            Except();
            DistinctWithComparer();
        }

        [TestMethod]
        public void Convolution()
        {
            Zip();
        }

        [TestMethod]
        public void PartioningTest()
        {
            SkipTake();
            TakeWhileSkipWhile();
            TakeWhileSkipWhileWithIndex();
        }

        [TestMethod]
        public void OrderingTest()
        {
            OrderBy();
            OrderByDescending();
            OrderByWithComparer();
            ThenBy();
            OrderByOrderBy();
            Reverse();
        }

        [TestMethod]
        public void ConversionTest()
        {
            try
            {
                CastNonGenericIEnumerable();
                Assert.Fail();
            }
            catch (TeamFoundationServerUnauthorizedException exception)
            {
                Trace.WriteLine(exception);
            }
            CastNonGenericIEnumerable2();
            CastGenericIEnumerable();
            try
            {
                CastGenericIEnumerableWithException();
                Assert.Fail();
            }
            catch (InvalidCastException exception)
            {
                Trace.WriteLine(exception);
            }
            CastWithJoin();
            AsEnumerable();
            AsEnumerableReverse();
            ToArrayToList();
            ToDictionaryToLookup();
            try
            {
                ToDictionaryWithException();
                Assert.Fail();
            }
            catch (ArgumentException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                DictionaryLookup();
                Assert.Fail();
            }
            catch (KeyNotFoundException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                DictionaryLookupNullKey();
                Assert.Fail();
            }
            catch (ArgumentNullException exception)
            {
                Trace.WriteLine(exception);
            }
            ToLookupWithComparer();
        }

        [TestMethod]
        public void ElementTest()
        {
            try
            {
                FirstLast();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                FirstLastWithPredicate();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            FirstOrDefaultLastOrDefault();
            try
            {
                ElementAt();
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Trace.WriteLine(exception);
            }
            ElementAtOrDefault();
            try
            {
                Single();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                SingleOrDefault();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void AggregationTest()
        {
            try
            {
                Aggregate();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            AggregateWithSeed();
            AggregateWithSeedAndResultSelector();
            Count();
            // OverflowException.
            // CountWithPredicate();
            // Long running.
            // LongCount();
            try
            {
                MinMax();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            MinMaxWithSelector();
            OrderByDescending2();
            AggregateWithSeed2();
            Except2();
            MinMaxGeneric();
            try
            {
                SumAverage();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            AverageWithSelector();
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            All();
            Any();
            AnyWithPredicate();
            Contains();
            ContainsWithComparer();
        }

        [TestMethod]
        public void EqualityTest()
        {
            SequentialEqual();
            SequentialEqualOfEmpty();
            SequentialEqualWithComparer();
        }
    }
}

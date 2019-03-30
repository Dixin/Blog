namespace Tutorial.Tests.LinqToObjects
{
#if NETFX
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net;

    using Microsoft.TeamFoundation;
    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Tutorial.LinqToObjects.QueryMethods;
#else
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Tutorial.LinqToObjects.QueryMethods;
#endif

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void GenerationTest()
        {
            Empty();
            Range();
            MaxRange();
            DefaultIfEmpty();
            DefaultIfEmptyWithDefaultValue();
        }

        [TestMethod]
        public void FilteringTest()
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
            CompiledLet();
            SelectMany();
            FluentSelectMany();
            SelectManyWithResultSelector();
            SelectManyWithResultSelector();
        }

        [TestMethod]
        public void GroupingTest()
        {
            GroupBy();
            GroupByWithResultSelector();
            GroupByAndSelect();
            FluentGroupByAndSelect();
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
        public void ConvolutionTest()
        {
            Zip();
        }

        [TestMethod]
        public void PartitioningTest()
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
            OrderByAndOrderBy();
            Reverse();
        }

        [TestMethod]
        public void ConversionTest()
        {
#if NETFX
            try
            {
                CastNonGeneric(new VssCredentials(new VssBasicCredential(new NetworkCredential("dixinyan@live.com", "***")))
                {
                    PromptType = CredentialPromptType.PromptIfNeeded
                });
                Assert.Fail();
            }
            catch (TeamFoundationServerUnauthorizedException exception)
            {
                Trace.WriteLine(exception);
            }
            CastMoreNonGeneric();
#endif
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
                LookupDictionary();
                Assert.Fail();
            }
            catch (KeyNotFoundException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                LookupDictionaryNullKey();
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
            MaxWithSelector();
            OrderByDescendingAndTakeWhile();
            AggregateWithAnonymousTypeSeed();
            Except2();
            MaxMinGeneric();
            MaxMinGenericWithSelector();
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

        private void Contract_ContractFailed(object sender, ContractFailedEventArgs e)
        {
            throw new NotImplementedException();
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

namespace Dixin.Tests.Linq.LinqToSql
{
    using System;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Dixin.Linq.LinqToSql.QueryMethods;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void GenerationTest()
        {
            DefaultIfEmpty();
        }

        [TestMethod]
        public void FilteringTest()
        {
            Where();
            WhereWithOr();
            WhereWithAnd();
            WhereAndWhere();
            WhereWithLike();
            WhereWithLikeMethod();
            WhereWithContains();
            WhereWithNull();
            OfType();
        }

        [TestMethod]
        public void MappingTest()
        {
            Select();
            SelectWithStringConcat();
            SelectAnonymousType();
            try
            {
                SelectEntity();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            SelectEntityObjects();
            SelectWithCase();
        }

        [TestMethod]
        public void GroupingTest()
        {
            Grouping();
            GroupBy();
            GroupByWithWhere();
        }

        [TestMethod]
        public void JoinTest()
        {
            InnerJoin();
            InnerJoinWithSelectMany();
            InnerJoinWithAssociation();
            InnerJoinWithMultipleKeys();
            LeftOuterJoin();
            LeftOuterJoinWithDefaultIfEmpty();
            LeftOuterJoinWithSelect();
            LeftOuterJoinWithAssociation();
            CrossJoin();
            CrossJoinWithSelectMany();
            CrossJoinWithJoin();
            SelfJoin();
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            Concat();
            ConcatWithSelect();
        }

        [TestMethod]
        public void SetTest()
        {
            Distinct();
            DistinctWithGroupByAndSelect();
            DistinctWithGroupByAndSelectMany();
            Intersect();
            Except();
        }

        [TestMethod]
        public void PartitioningTest()
        {
            Skip();
            OrderBySkip();
            Take();
            OrderBySkipTake();
        }

        [TestMethod]
        public void OrderingTest()
        {
            OrderBy();
            OrderByDescending();
            OrderByThenBy();
            OrderByOrderBy();
            try
            {
                Reverse();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void ConversionTest()
        {
            Cast();
        }

        [TestMethod]
        public void ElementTest()
        {
            First();
            FirstOrDefault();
            try
            {
                Last();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                LastOrDefault();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            Single();
            SingleOrDefault();
        }

        [TestMethod]
        public void AggregateTest()
        {
            Count();
            LongCount();
            Min();
            Max();
            Sum();
            Average();
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            All();
            Any();
            Contains();
        }
    }
}

namespace Tutorial.Tests.LinqToSql
{
    using System;
    using System.Diagnostics;

    using Tutorial.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void GenerationTest()
        {
            QueryMethods.DefaultIfEmpty();
            try
            {
                QueryMethods.DefaultIfEmptyWithPrimitive(); // TODO.
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.DefaultIfEmptyWithEntity();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void FilteringTest()
        {
            QueryMethods.Where();
            QueryMethods.WhereWithOr();
            QueryMethods.WhereWithAnd();
            QueryMethods.WhereAndWhere();
            QueryMethods.WhereWithIs();
            QueryMethods.OfTypeWithEntity();
            QueryMethods.OfTypeWithPrimitive(); // TODO.
        }

        [TestMethod]
        public void MappingTest()
        {
            QueryMethods.Select();
            QueryMethods.SelectWithStringConcat();
            QueryMethods.SelectAnonymousType();
        }

        [TestMethod]
        public void GroupingTest()
        {
            QueryMethods.GroupBy(); // TODO. N+!.
            QueryMethods.GroupByWithResultSelector();
            QueryMethods.GroupByAndSelect();
            QueryMethods.GroupByAndSelectMany();
            QueryMethods.GroupByMultipleKeys();
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoinWithJoin();
            QueryMethods.InnerJoinWithSelectMany();
            QueryMethods.InnerJoinWithGroupJoin();
            QueryMethods.InnerJoinWithSelect();
            QueryMethods.InnerJoinWithRelationship();
            QueryMethods.MultipleInnerJoinsWithRelationship();
            QueryMethods.InnerJoinWithMultipleKeys();
            QueryMethods.InnerJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithSelect();
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany();
            QueryMethods.LeftOuterJoinWithSelectAndSelectMany();
            QueryMethods.LeftOuterJoinWithRelationship();
            QueryMethods.CrossJoinWithSelectMany();
            QueryMethods.CrossJoinWithJoin();
            QueryMethods.SelfJoin();
        }

        [TestMethod]
        public void ApplyTest()
        {
            QueryMethods.CrossApplyWithGroupByAndTake();
            QueryMethods.CrossApplyWithGroupJoinAndTake();
            QueryMethods.CrossApplyWithRelationshipAndTake();
            QueryMethods.OuterApplyWithGroupByAndFirstOrDefault(); // TODO.N+1.
            QueryMethods.OuterApplyWithGroupJoinAndFirstOrDefault(); // TODO.N+1.
            QueryMethods.OuterApplyWithRelationshipAndFirstOrDefault(); // TODO.N+1.
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            QueryMethods.Concat();
            QueryMethods.ConcatWithSelect();
        }

        [TestMethod]
        public void SetTest()
        {
            QueryMethods.Distinct();
            QueryMethods.DistinctWithGroupBy();
            QueryMethods.DistinctMultipleKeys();
            QueryMethods.DistinctMultipleKeysWithGroupBy();
            QueryMethods.DistinctWithGroupByAndSelectAndFirstOrDefault();
            QueryMethods.Intersect();
            QueryMethods.Except();
        }

        [TestMethod]
        public void PartitioningTest()
        {
            QueryMethods.Skip(); // TODO.
            QueryMethods.OrderByAndSkip();
            QueryMethods.Take();
            QueryMethods.OrderByAndSkipAndTake();
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.OrderBy();
            QueryMethods.OrderByDescending();
            QueryMethods.OrderByAndThenBy();
            try
            {
                QueryMethods.OrderByAnonymousType();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.OrderByAndOrderBy();
            try
            {
                QueryMethods.Reverse();
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
            try
            {
                QueryMethods.CastPrimitive(); // TODO.
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.CastEntity(); // TODO.
            QueryMethods.AsEnumerableAsQueryable();
            try
            {
                QueryMethods.SelectEntities();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.SelectEntityObjects();
        }

        [TestMethod]
        public void ElementTest()
        {
            QueryMethods.First();
            QueryMethods.FirstOrDefault();
            try
            {
                QueryMethods.Last();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.LastOrDefault();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.Single();
            QueryMethods.SingleOrDefault();
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.Count();
            QueryMethods.LongCount();
            QueryMethods.Max();
            QueryMethods.Min();
            QueryMethods.Average();
            QueryMethods.Sum();
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            QueryMethods.Any();
            QueryMethods.AnyWithPredicate();
            QueryMethods.Contains();
            QueryMethods.AllNot();
            QueryMethods.NotAny();
        }
    }
}

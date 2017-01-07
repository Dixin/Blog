namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Diagnostics;

    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void QueryTable()
        {
            Query.Table();
        }

        [TestMethod]
        public void GenerationTest()
        {
            QueryMethods.DefaultIfEmpty(new WideWorldImporters());
            QueryMethods.DefaultIfEmptyWithPrimitive(new WideWorldImporters());
#if NETFX
            try
            {
                QueryMethods.DefaultIfEmptyWithEntity(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.DefaultIfEmptyWithEntity(new WideWorldImporters());
#endif
        }

        [TestMethod]
        public void FilteringTest()
        {
            QueryMethods.Where(new WideWorldImporters());
            QueryMethods.WhereWithOr(new WideWorldImporters());
            QueryMethods.WhereWithAnd(new WideWorldImporters());
            QueryMethods.WhereAndWhere(new WideWorldImporters());
            QueryMethods.WhereWithIs(new WideWorldImporters());
            QueryMethods.OfTypeWithEntity(new WideWorldImporters());
#if NETFX
            try
            {
                QueryMethods.OfTypeWithPrimitive(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.OfTypeWithPrimitive(new WideWorldImporters());
#endif
        }

        [TestMethod]
        public void MappingTest()
        {
            QueryMethods.Select(new WideWorldImporters());
            QueryMethods.SelectWithStringConcat(new WideWorldImporters());
            QueryMethods.SelectAnonymousType(new WideWorldImporters());
        }

        [TestMethod]
        public void GroupingTest()
        {
            QueryMethods.GroupBy(new WideWorldImporters());
            QueryMethods.GroupByWithResultSelector(new WideWorldImporters());
            QueryMethods.GroupByAndSelect(new WideWorldImporters());
            QueryMethods.GroupByAndSelectMany(new WideWorldImporters());
            QueryMethods.GroupByMultipleKeys(new WideWorldImporters());
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoinWithJoin(new WideWorldImporters());
            QueryMethods.InnerJoinWithSelectMany(new WideWorldImporters());
            QueryMethods.InnerJoinWithGroupJoin(new WideWorldImporters());
            QueryMethods.InnerJoinWithSelect(new WideWorldImporters());
            QueryMethods.InnerJoinWithAssociation(new WideWorldImporters());
            QueryMethods.MultipleInnerJoinsWithAssociations(new WideWorldImporters());
            QueryMethods.InnerJoinWithMultipleKeys(new WideWorldImporters());
            QueryMethods.InnerJoinWithGroupJoin(new WideWorldImporters());
            QueryMethods.LeftOuterJoinWithGroupJoin(new WideWorldImporters());
            QueryMethods.LeftOuterJoinWithSelect(new WideWorldImporters());
#if NETFX
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany(new WideWorldImporters());
#endif
            QueryMethods.LeftOuterJoinWithSelectAndSelectMany(new WideWorldImporters());
#if NETFX
            QueryMethods.LeftOuterJoinWithAssociation(new WideWorldImporters());
#endif
            QueryMethods.CrossJoinWithSelectMany(new WideWorldImporters());
#if NETFX
            QueryMethods.CrossJoinWithJoin(new WideWorldImporters());
#endif
            QueryMethods.SelfJoin(new WideWorldImporters());
        }

        [TestMethod]
        public void ApplyTest()
        {
            QueryMethods.CrossApplyWithGroupByAndTake(new WideWorldImporters());
            QueryMethods.CrossApplyWithGroupJoinAndTake(new WideWorldImporters());
#if NETFX
            QueryMethods.CrossApplyWithAssociationAndTake(new WideWorldImporters());
#endif
            QueryMethods.OuterApplyWithGroupByAndFirstOrDefault(new WideWorldImporters());
            QueryMethods.OuterApplyWithGroupJoinAndFirstOrDefault(new WideWorldImporters());
            QueryMethods.OuterApplyWithAssociationAndFirstOrDefault(new WideWorldImporters());
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            QueryMethods.Concat(new WideWorldImporters());
#if NETFX
            QueryMethods.ConcatWithSelect(new WideWorldImporters());
#endif
        }

        [TestMethod]
        public void SetTest()
        {
            QueryMethods.Distinct(new WideWorldImporters());
            QueryMethods.DistinctWithGroupBy(new WideWorldImporters());
            QueryMethods.DistinctMultipleKeys(new WideWorldImporters());
            QueryMethods.DistinctMultipleKeysWithGroupBy(new WideWorldImporters());
            QueryMethods.DistinctWithGroupByAndSelectAndFirstOrDefault(new WideWorldImporters());
            QueryMethods.Intersect(new WideWorldImporters());
            QueryMethods.Except(new WideWorldImporters());
        }

        [TestMethod]
        public void PartitioningTest()
        {
#if NETFX
            try
            {
                QueryMethods.Skip(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.Skip(new WideWorldImporters());
#endif
            QueryMethods.OrderByAndSkip(new WideWorldImporters());
            QueryMethods.Take(new WideWorldImporters());
            QueryMethods.OrderByAndSkipAndTake(new WideWorldImporters());
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.OrderBy(new WideWorldImporters());
            QueryMethods.OrderByDescending(new WideWorldImporters());
            QueryMethods.OrderByAndThenBy(new WideWorldImporters());
            QueryMethods.OrderByAnonymousType(new WideWorldImporters());
            QueryMethods.OrderByAndOrderBy(new WideWorldImporters());
            try
            {
                QueryMethods.Reverse(new WideWorldImporters());
                Assert.Fail();
            }
#if NETFX
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            catch (NotImplementedException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
        }

        [TestMethod]
        public void ConversionTest()
        {
#if NETFX
            QueryMethods.CastPrimitive(new WideWorldImporters());
#else
            try
            {
                QueryMethods.CastPrimitive(new WideWorldImporters());
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
#if NETFX
            try
            {
                QueryMethods.CastEntity(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.CastEntity(new WideWorldImporters());
#endif
            QueryMethods.AsEnumerableAsQueryable(new WideWorldImporters());
#if NETFX
            try
            {
                QueryMethods.SelectEntities(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.SelectEntities(new WideWorldImporters());
#endif
            QueryMethods.SelectEntityObjects(new WideWorldImporters());
        }

        [TestMethod]
        public void ElementTest()
        {
            QueryMethods.First(new WideWorldImporters());
            QueryMethods.FirstOrDefault(new WideWorldImporters());
#if NETFX
            try
            {
                QueryMethods.Last(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.LastOrDefault(new WideWorldImporters());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.Last(new WideWorldImporters());
            QueryMethods.LastOrDefault(new WideWorldImporters());
#endif
            QueryMethods.Single(new WideWorldImporters());
            QueryMethods.SingleOrDefault(new WideWorldImporters());
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.Count(new WideWorldImporters());
            QueryMethods.LongCount(new WideWorldImporters());
            QueryMethods.Max(new WideWorldImporters());
            QueryMethods.Min(new WideWorldImporters());
            QueryMethods.Average(new WideWorldImporters());
            QueryMethods.Sum(new WideWorldImporters());
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            QueryMethods.Any(new WideWorldImporters());
            QueryMethods.AnyWithPredicate(new WideWorldImporters());
            QueryMethods.Contains(new WideWorldImporters());
            QueryMethods.AllNot(new WideWorldImporters());
            QueryMethods.NotAny(new WideWorldImporters());
        }
    }
}

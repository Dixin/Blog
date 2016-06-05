namespace Dixin.Tests.Linq.EntityFramework
{
    using System;
    using System.Data.Entity.Core;
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
            QueryMethods.DefaultIfEmpty();
        }

        [TestMethod]
        public void FilteringTest()
        {
            QueryMethods.Where();
            QueryMethods.WhereWithOr();
            QueryMethods.WhereWithAnd();
            QueryMethods.WhereAndWhere();
            QueryMethods.WhereWithIs();
            QueryMethods.OfType();
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
            QueryMethods.GroupBy();
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
            QueryMethods.InnerJoinWithAssociation();
            QueryMethods.MultipleInnerJoinsWithAssociations();
            QueryMethods.InnerJoinWithMultipleKeys();
            QueryMethods.InnerJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithSelect();
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany();
            QueryMethods.LeftOuterJoinWithSelectAndSelectMany();
            QueryMethods.LeftOuterJoinWithAssociation();
            QueryMethods.CrossJoinWithSelectMany();
            QueryMethods.CrossJoinWithJoin();
            QueryMethods.SelfJoin();
        }

        [TestMethod]
        public void ApplyTest()
        {
            QueryMethods.CrossApplyWithGroupByAndTake();
            QueryMethods.CrossApplyWithGroupJoinAndTake();
            QueryMethods.CrossApplyWithAssociationAndTake();
            QueryMethods.OuterApplyWithGroupByAndFirstOrDefault();
            QueryMethods.OuterApplyWithGroupJoinAndFirstOrDefault();
            QueryMethods.OuterApplyWithAssociationAndFirstOrDefault();
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
            try
            {
                QueryMethods.Skip();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
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
            QueryMethods.CastPrimitive();
            try
            {
                QueryMethods.CastEntity();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
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

        [TestMethod]
        public void LocalRemoteMethodTest()
        {
            QueryMethods.WhereWithLike();
            try
            {
                QueryMethods.WhereWithLikeMethod();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.WhereWithContains();
            QueryMethods.WhereWithNull();
            QueryMethods.InlinePredicate();
            QueryMethods.InlinePredicateCompiled();
            try
            {
                QueryMethods.MethodPredicate();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.MethodPredicateCompiled();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.MethodSelector();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.LocalSelector();
            QueryMethods.RemoteMethod();
        }

        [TestMethod]
        public void LazinessTest()
        {
            try
            {
                UI.ViewCategoryProducts("Bikes");
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.LazyLoading();
            }
            catch (EntityCommandExecutionException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.LazyLoadingWithToArray();
            QueryMethods.EagerLoadingWithSelect();
            QueryMethods.EagerLoadingWithAssociation();
            try
            {
                QueryMethods.ConditionalEagerLoading();
            }
            catch (ArgumentException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.ConditionalEagerLoadingWithSelect();
            QueryMethods.NoLoading();
        }

    }
}

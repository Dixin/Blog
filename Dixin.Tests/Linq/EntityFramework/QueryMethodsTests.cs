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
        public void LocalRemoteMethodTest()
        {
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
            QueryMethods.OfType();
        }

        [TestMethod]
        public void MappingTest()
        {
            QueryMethods.Select();
            QueryMethods.SelectWithStringConcat();
            QueryMethods.SelectAnonymousType();
            try
            {
                QueryMethods.SelectEntity();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.SelectEntityObjects();
            QueryMethods.SelectWithCase();
        }

        [TestMethod]
        public void GroupingTest()
        {
            QueryMethods.GroupBy();
            QueryMethods.GroupByWithResultSelector();
            QueryMethods.GroupByAndSelect();
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoinWithJoin();
            QueryMethods.InnerJoinWithSelectMany();
            QueryMethods.InnerJoinWithAssociation();
            QueryMethods.MultipleInnerJoinsWithAssociations();
            QueryMethods.InnerJoinWithMultipleKeys();
            QueryMethods.InnerJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithGroupJoin();
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany();
            QueryMethods.LeftOuterJoinWithSelect();
            QueryMethods.LeftOuterJoinWithAssociation();
            QueryMethods.CrossJoinWithSelectMany();
            QueryMethods.CrossJoinWithJoin();
            QueryMethods.SelfJoin();
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
            QueryMethods.DistinctWithGroupByAndFirstOrDefault();
            QueryMethods.DistinctWithGroupByAndTake();
            QueryMethods.DistinctWithGroupByAndSelectAndFirstOrDefault();
            QueryMethods.DistinctWithGroupByAndSelectAndTake();
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
            try
            {
                QueryMethods.Cast();
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.AsEnumerableAsQueryable();
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
            QueryMethods.Min();
            QueryMethods.Max();
            QueryMethods.Sum();
            QueryMethods.Average();
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            QueryMethods.All();
            QueryMethods.Any();
            QueryMethods.Contains();
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

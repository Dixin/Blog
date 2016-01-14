namespace Dixin.Tests.Linq.LinqToSql
{
    using System;
    using System.Diagnostics;

    using Dixin.Linq.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void LazinessTest()
        {
            try
            {
                UI.ViewCategoryProducts("Bikes");
                Assert.Fail();
            }
            catch (ObjectDisposedException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.DeferredLoading();
            QueryMethods.EagerLoadingWithSelect();
            QueryMethods.EagerLoadingWithAssociation();
            QueryMethods.ConditionalEagerLoading();
            QueryMethods.NoLoading();
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
            QueryMethods.MethodSelector();
            QueryMethods.LocalSelector();
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
            QueryMethods.WhereWithLikeMethod();
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
            QueryMethods.Grouping();
            QueryMethods.GroupBy();
            QueryMethods.GroupByWithWhere();
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoin();
            QueryMethods.InnerJoinWithSelectMany();
            QueryMethods.InnerJoinWithAssociation();
            QueryMethods.InnerJoinWithMultipleKeys();
            QueryMethods.LeftOuterJoin();
            QueryMethods.LeftOuterJoinWithDefaultIfEmpty();
            QueryMethods.LeftOuterJoinWithSelect();
            QueryMethods.LeftOuterJoinWithAssociation();
            QueryMethods.CrossJoin();
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
            QueryMethods.DistinctWithGroupByAndSelect();
            QueryMethods.DistinctWithGroupByAndSelectMany();
            QueryMethods.Intersect();
            QueryMethods.Except();
        }

        [TestMethod]
        public void PartitioningTest()
        {
            QueryMethods.Skip();
            QueryMethods.OrderBySkip();
            QueryMethods.Take();
            QueryMethods.OrderBySkipTake();
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.OrderBy();
            QueryMethods.OrderByDescending();
            QueryMethods.OrderByThenBy();
            QueryMethods.OrderByOrderBy();
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
            QueryMethods.Cast();
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
    }
}

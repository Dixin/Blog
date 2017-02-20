namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Diagnostics;

    using Tutorial.LinqToEntities;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void QueryTableTests()
        {
            Performance.Initialize();
        }

        [TestMethod]
        public void GenerationTest()
        {
            QueryMethods.DefaultIfEmpty(new AdventureWorks());
#if NETFX
            try
            {
                QueryMethods.DefaultIfEmptyEntity(new AdventureWorks(), new ProductCategory());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.DefaultIfEmptyEntity(new AdventureWorks(), new ProductCategory());
#endif
            QueryMethods.DefaultIfEmptyPrimitive(new AdventureWorks());
        }

        [TestMethod]
        public void FilteringTest()
        {
            QueryMethods.Where(new AdventureWorks());
            QueryMethods.WhereWithOr(new AdventureWorks());
            QueryMethods.WhereWithAnd(new AdventureWorks());
            QueryMethods.WhereAndWhere(new AdventureWorks());
            QueryMethods.WhereWithIs(new AdventureWorks());
            QueryMethods.OfTypeEntity(new AdventureWorks());
#if NETFX
            try
            {
                QueryMethods.OfTypePrimitive(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.OfTypePrimitive(new AdventureWorks());
#endif
        }

        [TestMethod]
        public void MappingTest()
        {
            QueryMethods.Select(new AdventureWorks());
            QueryMethods.SelectWithStringConcat(new AdventureWorks());
#if NETFX
            try
            {
                QueryMethods.SelectEntity(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.SelectEntity(new AdventureWorks());
#endif
            QueryMethods.SelectAnonymousType(new AdventureWorks());
        }

        [TestMethod]
        public void GroupingTest()
        {
            QueryMethods.GroupBy(new AdventureWorks());
            QueryMethods.GroupByWithResultSelector(new AdventureWorks());
            QueryMethods.GroupByAndSelect(new AdventureWorks());
            QueryMethods.GroupByAndSelectMany(new AdventureWorks());
            QueryMethods.GroupByMultipleKeys(new AdventureWorks());
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoinWithJoin(new AdventureWorks());
            QueryMethods.InnerJoinWithSelect(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectMany(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectAndRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectManyAndRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithMultipleKeys(new AdventureWorks());
            QueryMethods.MultipleInnerJoinsWithRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithGroupJoinAndSelectMany(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithGroupJoin(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelect(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelectMany(new AdventureWorks());
#if NETFX
            QueryMethods.LeftOuterJoinWithSelectAndRelationship(new AdventureWorks());
#else
            try
            {
                QueryMethods.LeftOuterJoinWithSelectAndRelationship(new AdventureWorks());
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
#if NETFX
            QueryMethods.LeftOuterJoinWithSelectManyAndRelationship(new AdventureWorks());
#else
            try
            {
                QueryMethods.LeftOuterJoinWithSelectManyAndRelationship(new AdventureWorks());
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
            QueryMethods.CrossJoinWithSelectMany(new AdventureWorks());
            QueryMethods.CrossJoinWithJoin(new AdventureWorks());
            QueryMethods.SelfJoin(new AdventureWorks());
        }

        [TestMethod]
        public void ApplyTest()
        {
            QueryMethods.CrossApplyWithGroupByAndTake(new AdventureWorks());
            QueryMethods.CrossApplyWithGroupJoinAndTake(new AdventureWorks());
#if NETFX
            QueryMethods.CrossApplyWithRelationshipAndTake(new AdventureWorks());
#endif
            QueryMethods.OuterApplyWithGroupByAndFirstOrDefault(new AdventureWorks());
            QueryMethods.OuterApplyWithGroupJoinAndFirstOrDefault(new AdventureWorks());
            QueryMethods.OuterApplyWithRelationshipAndFirstOrDefault(new AdventureWorks());
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            QueryMethods.ConcatPrimitive(new AdventureWorks());
#if NETFX
            QueryMethods.ConcatEntity(new AdventureWorks());
#else
            try 
            {          
                QueryMethods.ConcatEntity(new AdventureWorks());
                Assert.Fail();
            }
            catch (ArgumentException exception)
            {
                Trace.WriteLine(exception);
            }
#endif
        }

        [TestMethod]
        public void SetTest()
        {
            //QueryMethods.DistinctEntity(new AdventureWorks());
            //QueryMethods.DistinctPrimitive(new AdventureWorks());
            //QueryMethods.DistinctEntityWithGroupBy(new AdventureWorks());
            //QueryMethods.DistinctWithGroupBy(new AdventureWorks());
            //QueryMethods.DistinctMultipleKeys(new AdventureWorks());
            //QueryMethods.DistinctMultipleKeysWithGroupBy(new AdventureWorks());
            //QueryMethods.DistinctWithGroupByAndFirstOrDefault(new AdventureWorks());
            QueryMethods.UnionEntity(new AdventureWorks());
            QueryMethods.UnionPrimitive(new AdventureWorks());
            QueryMethods.IntersectEntity(new AdventureWorks());
            QueryMethods.IntersectPrimitive(new AdventureWorks());
            QueryMethods.ExceptEntity(new AdventureWorks());
            QueryMethods.ExceptPrimitive(new AdventureWorks());
        }

        [TestMethod]
        public void ConvolutionTest()
        {
            try
            {
                QueryMethods.Zip(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void PartitioningTest()
        {
#if NETFX
            try
            {
                QueryMethods.Skip(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.Skip(new AdventureWorks());
#endif
            QueryMethods.OrderByAndSkip(new AdventureWorks());
            QueryMethods.Take(new AdventureWorks());
            QueryMethods.OrderByAndSkipAndTake(new AdventureWorks());
            try
            {
                QueryMethods.SkipWhile(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.TakeWhile(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.OrderBy(new AdventureWorks());
            QueryMethods.OrderByDescending(new AdventureWorks());
            QueryMethods.OrderByAndThenBy(new AdventureWorks());
            QueryMethods.OrderByMultipleKeys(new AdventureWorks());
            QueryMethods.OrderByAndOrderBy(new AdventureWorks());
            try
            {
                QueryMethods.Reverse(new AdventureWorks());
                Assert.Fail();
            }
#if NETFX
            catch (NotSupportedException exception)
#else
            catch (NotImplementedException exception)
#endif
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void ConversionTest()
        {
//#if NETFX
//            QueryMethods.CastPrimitive(new AdventureWorks());
//#else
//            try
//            {
//                QueryMethods.CastPrimitive(new AdventureWorks());
//                Assert.Fail();
//            }
//            catch (InvalidOperationException exception)
//            {
//                Trace.WriteLine(exception);
//            }
//#endif
//#if NETFX
//            try
//            {
//                QueryMethods.CastEntity(new AdventureWorks());
//                Assert.Fail();
//            }
//            catch (NotSupportedException exception)
//            {
//                Trace.WriteLine(exception);
//            }
//#else
//            QueryMethods.CastEntity(new AdventureWorks());
//#endif
            QueryMethods.AsEnumerableAsQueryable(new AdventureWorks());
            QueryMethods.SelectLocalEntity(new AdventureWorks());
        }

        [TestMethod]
        public void ElementTest()
        {
            QueryMethods.First(new AdventureWorks());
            QueryMethods.FirstOrDefault(new AdventureWorks());
#if NETFX
            try
            {
                QueryMethods.Last(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.Last(new AdventureWorks());
#endif
#if NETFX
            try
            {
                QueryMethods.LastOrDefault(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.LastOrDefault(new AdventureWorks());
#endif
            QueryMethods.Single(new AdventureWorks());
            QueryMethods.SingleOrDefault(new AdventureWorks());
            try
            {
                QueryMethods.ElementAt(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.ElementAtOrDefault(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.Count(new AdventureWorks());
            QueryMethods.LongCount(new AdventureWorks());
            QueryMethods.Max(new AdventureWorks());
            QueryMethods.Min(new AdventureWorks());
            QueryMethods.Average(new AdventureWorks());
            QueryMethods.Sum(new AdventureWorks());
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            QueryMethods.Any(new AdventureWorks());
            QueryMethods.All(new AdventureWorks());
            QueryMethods.ContainsPrimitive(new AdventureWorks());
#if NETFX
            try
            {
                QueryMethods.ContainsEntity(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
#else
            QueryMethods.ContainsEntity(new AdventureWorks());
#endif
            QueryMethods.AllNot(new AdventureWorks());
            QueryMethods.NotAny(new AdventureWorks());
        }

        [TestMethod]
        public void Equality()
        {
            try
            {
                QueryMethods.SequenceEqual(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }
    }
}

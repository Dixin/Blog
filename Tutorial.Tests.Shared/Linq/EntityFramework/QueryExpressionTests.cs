namespace Dixin.Tests.Linq.EntityFramework
{
    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueryExpressionTests
    {
        [TestMethod]
        public void JoinTest()
        {
            QueryExpressions.InnerJoinWithJoin(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelect(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectMany(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectAndRelationship(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectManyAndRelationship(new AdventureWorks());
            QueryExpressions.LeftOuterJoinWithSelectMany(new AdventureWorks());
        }
    }
}

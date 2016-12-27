namespace Dixin.Tests.Linq.LinqToSql
{
    using Dixin.Linq.LinqToSql;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void TransactionTest()
        {
            Transactions.Default();
            Transactions.DbTransaction();
            Transactions.TransactionScope();
        }
    }
}

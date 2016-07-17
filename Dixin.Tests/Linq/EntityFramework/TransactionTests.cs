namespace Dixin.Tests.Linq.EntityFramework
{
    using Dixin.Linq.EntityFramework;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void TransactionTest()
        {
            Transactions.Default();
            Transactions.DbContextTransaction();
            Transactions.DbTransaction();
            Transactions.TransactionScope();
        }
    }
}

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
            Transactions.Default(new AdventureWorks());
            Transactions.DbContextTransaction(new AdventureWorks());
            Transactions.DbTransaction();
#if NETFX
            Transactions.TransactionScope();
#endif
        }
    }
}

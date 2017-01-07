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
            Transactions.Default(new WideWorldImporters());
            Transactions.DbContextTransaction(new WideWorldImporters());
            Transactions.DbTransaction();
#if NETFX
            Transactions.TransactionScope();
#endif
        }
    }
}

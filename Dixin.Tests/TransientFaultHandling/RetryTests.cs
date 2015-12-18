namespace Dixin.Tests.TransientFaultHandling
{
    using System;

    using Dixin.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RetryTests
    {
        [TestMethod]
        public void ExecuteTest()
        {
            int retryCount = 0;
            try
            {
                int result1 = Retry.Execute(
                    () =>
                        {
                            retryCount++;
                            if (retryCount < 5)
                            {
                                throw new OperationCanceledException();
                            }

                            return retryCount;
                        },
                    3,
                    exception => exception is OperationCanceledException);
                Assert.Fail(result1.ToString());
            }
            catch (OperationCanceledException)
            {
                Assert.AreEqual(4, retryCount);
            }

            retryCount = 0;
            int result2 = Retry.Execute(() =>
                {
                    retryCount++;
                    if (retryCount < 5)
                    {
                        throw new OperationCanceledException();
                    }

                    return retryCount;
                },
                5,
                exception => exception is OperationCanceledException);
            Assert.AreEqual(5, retryCount);
            Assert.AreEqual(5, result2);

            retryCount = 0;
            int result3 = Retry.Execute(() =>
                {
                    retryCount++;
                    return retryCount;
                },
                0,
                exception => exception is OperationCanceledException);
            Assert.AreEqual(1, retryCount);
        }
    }
}

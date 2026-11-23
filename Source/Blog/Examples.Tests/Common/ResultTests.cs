namespace Examples.Tests.Common;

using Examples.Common;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void AwaitResultTest()
    {
        Assert.AreEqual(1, AwaitResult().Result);
    }

    public static async Task<int> AwaitResult()
    {
        Result<int> f1 = 1;
        int result1 = await f1;
        return result1;
    }

    [TestMethod]
    public void ReturnAsyncResultTest()
    {
        Assert.AreEqual(2, CallReturnAsyncResult().Result);
    }

    public static async Result<int> ReturnAsyncResult()
    {
        Result<int> f1 = 1;
        int result1 = await f1;
        Result<int> f2 = result1 + 1;
        int result2 = await f2;
        await Task.Yield();
        return result2;
    }

    public static async Task<int> CallReturnAsyncResult()
    {
        int result = await ReturnAsyncResult();
        return result;
    }
}
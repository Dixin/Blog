namespace Examples.Tests.Common;

using Examples.Threading.Tasks;

[TestClass]
public class FunTests
{
    [TestMethod]
    public void AwaitFunTest()
    {
        Assert.AreEqual(1, AwaitFun().Result);
    }

    public static async Task<int> AwaitFun()
    {
        Fun<int> f1 = () => 1;
        int result1 = await f1;
        return result1;
    }

    [TestMethod]
    public void ReturnAsyncFunTest()
    {
        Assert.AreEqual(2, CallReturnAsyncFun().Result);
    }

    public static async Fun<int> ReturnAsyncFun()
    {
        Fun<int> f1 = () => 1;
        int result1 = await f1;
        int F2() => result1 + 1;
        int result2 = await (Fun<int>)F2;
        return result2;
    }

    public static async Task<int> CallReturnAsyncFun()
    {
        int result = await ReturnAsyncFun();
        return result;
    }
}
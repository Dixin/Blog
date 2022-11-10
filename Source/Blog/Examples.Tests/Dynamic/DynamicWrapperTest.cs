namespace Examples.Tests.Dynamic;

using Examples.Dynamic;

using Microsoft.CSharp.RuntimeBinder;

[TestClass]
public class DynamicWrapperTest
{
    [TestMethod]
    public void StaticMemberTest()
    {
        Assert.AreEqual(0, StaticTest.Value);
        dynamic wrapper = new DynamicWrapper<StaticTest>();

        wrapper.value = 10;
        Assert.AreEqual(10, StaticTest.Value);
        Assert.AreEqual(10, wrapper.Value.ToStatic());

        Assert.AreEqual(2, wrapper.Method().ToStatic());
    }

    [TestMethod]
    public void GetSetIndexFromTypeTest()
    {
        BaseTest @base = new();
        Assert.AreEqual("0", @base[5, 5]);
        dynamic wrapper = @base.ToDynamic();
        wrapper[5, 5] = "10";
        Assert.AreEqual("10", @base[5, 5]);
        Assert.AreEqual("10", wrapper[5, 5]);
    }

    [TestMethod]
    public void GetInvokeMemberFromBaseTest()
    {
        Assert.AreEqual(0, new DerivedTest().ToDynamic().array[6, 6]);
        Assert.AreEqual(0, new DerivedTest().ToDynamic().array.ToStatic()[6, 6]);
    }

    [TestMethod]
    public void ValueTypeTest()
    {
        StructTest test = new(1);
        dynamic wrapper = test.ToDynamic();
        wrapper.value = 2;
        Assert.AreEqual(2, wrapper.value.ToStatic());
        Assert.AreNotEqual(2, test.Value);

        StructTest test2 = new(10);
        dynamic wrapper2 = new DynamicWrapper<StructTest>(ref test2);
        wrapper2.value = 20;
        Assert.AreEqual(20, wrapper2.value.ToStatic());
        Assert.AreNotEqual(20, test2.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(RuntimeBinderException))]
    public void ValueTypePropertyTest()
    {
        StructTest test2 = new(10);
        dynamic wrapper2 = new DynamicWrapper<StructTest>(ref test2);

        wrapper2.Value = 30;
    }
}
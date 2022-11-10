namespace Examples.Tests.Reflection;

using System.Collections.ObjectModel;

[TestClass]
public class TypeExtensionsTests
{
    [TestMethod]
    public void IsAssignableTests()
    {
        Assert.IsTrue(typeof(int).IsAssignableTo(typeof(object)));
        Assert.IsFalse(typeof(object).IsAssignableTo(typeof(int)));

        Assert.IsTrue(typeof(Uri).IsAssignableTo(typeof(object)));
        Assert.IsFalse(typeof(object).IsAssignableTo(typeof(Uri)));

        Assert.IsFalse(typeof(IEnumerable<int>).IsAssignableTo(typeof(IEnumerable<object>)));
        Assert.IsFalse(typeof(IEnumerable<object>).IsAssignableTo(typeof(IEnumerable<int>)));

        Assert.IsTrue(typeof(IEnumerable<Uri>).IsAssignableTo(typeof(IEnumerable<object>)));
        Assert.IsFalse(typeof(IEnumerable<object>).IsAssignableTo(typeof(IEnumerable<Uri>)));

        Assert.IsTrue(typeof(IEnumerable<object>).IsAssignableTo(typeof(IEnumerable<>)));
        Assert.IsFalse(typeof(IEnumerable<>).IsAssignableTo(typeof(IEnumerable<object>)));

        Assert.IsTrue(typeof(ICollection<object>).IsAssignableTo(typeof(IEnumerable<>)));
        Assert.IsFalse(typeof(IEnumerable<>).IsAssignableTo(typeof(ICollection<object>)));

        Assert.IsTrue(typeof(Collection<object>).IsAssignableTo(typeof(ICollection<>)));
        Assert.IsFalse(typeof(Collection<>).IsAssignableTo(typeof(ICollection<object>)));
        Assert.IsFalse(typeof(ICollection<>).IsAssignableTo(typeof(Collection<object>)));

        Assert.IsTrue(typeof(Collection<object>).IsAssignableTo(typeof(Collection<>)));
        Assert.IsFalse(typeof(Collection<>).IsAssignableTo(typeof(Collection<object>)));

        Assert.IsTrue(typeof(ICollection<object>).IsAssignableTo(typeof(IEnumerable<>)));
        Assert.IsFalse(typeof(IEnumerable<>).IsAssignableTo(typeof(ICollection<object>)));
    }
}
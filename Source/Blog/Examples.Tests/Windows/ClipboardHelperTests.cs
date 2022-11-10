namespace Examples.Tests.Windows;

using Examples.Windows;

[TestClass]
public class ClipboardHelperTests
{
    [TestMethod]
    public void TextTest()
    {
        string guid = Guid.NewGuid().ToString();
        ClipboardHelper.SetText(guid);
        Assert.AreEqual(guid, ClipboardHelper.GetText());
    }
}
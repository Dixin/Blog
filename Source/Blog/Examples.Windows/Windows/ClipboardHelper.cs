namespace Examples.Windows;

using System.Windows.Forms;
using Examples.Threading;

public static class ClipboardHelper
{
    public static string GetText
        (TextDataFormat format = TextDataFormat.UnicodeText) => ThreadHelper.Sta(() => Clipboard.GetText(format));

    public static void SetText(string text, TextDataFormat format = TextDataFormat.UnicodeText) =>
        ThreadHelper.Sta(() => Clipboard.SetText(text, format));

    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public static IDataObject? GetData() => ThreadHelper.Sta(Clipboard.GetDataObject);

    public static void SetDataObject(object data, bool copy = false) =>
        ThreadHelper.Sta(() => Clipboard.SetDataObject(data, copy));
}
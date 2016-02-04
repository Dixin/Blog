namespace Dixin.Windows
{
    using System.Windows;

    using Dixin.Threading;

    public static class ClipboardHelper
    {
        public static string GetText
            (TextDataFormat format = TextDataFormat.UnicodeText) => ThreadHelper.Sta(() => Clipboard.GetText(format));

        public static void SetText
            (string text, TextDataFormat format = TextDataFormat.UnicodeText) =>
                ThreadHelper.Sta(() => Clipboard.SetText(text, format));

        public static IDataObject GetData
            () => ThreadHelper.Sta(Clipboard.GetDataObject);

        public static void SetDataObject
            (object data, bool copy = false) =>
                ThreadHelper.Sta(() => Clipboard.SetDataObject(data, copy));
    }
}

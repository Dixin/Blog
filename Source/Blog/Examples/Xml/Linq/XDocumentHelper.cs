namespace Examples.Xml.Linq;

using Examples.Common;
using Examples.IO;

public static class XDocumentHelper
{
    public static XDocument Load(string file, LoadOptions options = LoadOptions.None) => 
        XDocument.Load(PathHelper.FromUrl(file.ThrowIfNullOrWhiteSpace()), options);
}
namespace Examples.Xml.Linq
{
    using System.Xml.Linq;

    using Examples.Common;
    using Examples.IO;

    public static class XDocumentHelper
    {
        public static XDocument Load(string file, LoadOptions options = LoadOptions.None)
        {
            file.NotNullOrWhiteSpace(nameof(file));

            return XDocument.Load(PathHelper.FromUrl(file), options);
        }
    }
}

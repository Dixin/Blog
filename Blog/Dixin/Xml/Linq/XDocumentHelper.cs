namespace Dixin.Xml.Linq
{
    using System.Xml.Linq;

    using Dixin.Common;
    using Dixin.IO;

    public static class XDocumentHelper
    {
        public static XDocument Load(string file, LoadOptions options = LoadOptions.None)
        {
            file.NotNullOrWhiteSpace(nameof(file));

            return XDocument.Load(PathHelper.FromUrl(file), options);
        }
    }
}

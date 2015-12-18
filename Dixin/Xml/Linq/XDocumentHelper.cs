namespace Dixin.Xml.Linq
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Xml.Linq;

    using Dixin.IO;

    public static class XDocumentHelper
    {
        public static XDocument Load(string file, LoadOptions options = LoadOptions.None)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(file));

            return XDocument.Load(PathHelper.FromUrl(file), options);
        }
    }
}

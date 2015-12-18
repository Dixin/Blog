namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;

    public static partial class QueryMethods
    {
        public static void EmptyIfNull()
        {
            Func<IEnumerable<int>, IEnumerable<int>> positive = source => source.EmptyIfNull().Where(@int => @int > 0);
            int count = positive(null).Count(); // 0.
        }

        public static void AppendPrepend()
        {
            IEnumerable<int> integers1 = Enumerable.Range(0, 5).Append(1).Prepend(-1);
            IEnumerable<int> integers2 = 1.PrependTo(integers1);
        }

        public static IEnumerable<Assembly> Libraries
            (string directory) => Directory.EnumerateFiles(directory, "*.dll").TrySelect(Assembly.LoadFrom);

        public static IEnumerable<byte[]> Download
            (IEnumerable<Uri> uris) => uris.Select(uri => new WebClient().DownloadData(uri)).Retry(3);
    }
}

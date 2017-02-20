namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using Mono.Cecil;

    internal static partial class QueryMethods
    {
        internal static void EmptyIfNull()
        {
            Func<IEnumerable<int>, IEnumerable<int>> positive = source => source.EmptyIfNull().Where(int32 => int32 > 0);
            int count = positive(null).Count(); // 0.
        }

        internal static void AppendPrepend()
        {
            IEnumerable<int> values1 = Enumerable.Range(0, 5).Append(1).Prepend(-1);
            IEnumerable<int> values2 = 1.PrependTo(values1);
        }

        internal static IEnumerable<AssemblyDefinition> Libraries(string directory) => 
            Directory.EnumerateFiles(directory, "*.dll").TrySelect(AssemblyDefinition.ReadAssembly);

        internal static IEnumerable<byte[]> ReadAll(IEnumerable<string> fileUris) =>
            fileUris
                .Select(File.ReadAllBytes)
                .Retry<byte[], Exception>(new FixedInterval(3, TimeSpan.FromSeconds(1)));
    }
}

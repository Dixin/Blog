namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using Mono.Cecil;

    internal static partial class QueryMethods
    {
        internal static void EmptyIfNull()
        {
            Func<IEnumerable<int>, IEnumerable<int>> positive = source => source.EmptyIfNull().Where(int32 => int32 > 0);
            int count = positive(null).Count(); // 0.
        }

        internal static IEnumerable<AssemblyDefinition> Libraries(string directory) => 
            Directory.EnumerateFiles(directory, "*.dll").TrySelect(AssemblyDefinition.ReadAssembly);

        internal static IEnumerable<byte[]> ReadAll(IEnumerable<string> fileUris) =>
            fileUris
                .Select(File.ReadAllBytes)
                .Retry<byte[], Exception>(new FixedInterval(3, TimeSpan.FromSeconds(1)));
    }
}

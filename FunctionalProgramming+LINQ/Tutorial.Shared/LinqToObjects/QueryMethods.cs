namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Mono.Cecil;

    internal static partial class QueryMethods
    {
        internal static void EmptyIfNull(IEnumerable<int> source1, IEnumerable<int> source2)
        {
            IEnumerable<int> positive = source1.EmptyIfNull()
                .Union(source2.EmptyIfNull())
                .Where(int32 => int32 > 0);
        }

        internal static IEnumerable<AssemblyDefinition> Libraries(string directory) => 
            Directory.EnumerateFiles(directory, "*.dll").TrySelect(AssemblyDefinition.ReadAssembly);

        internal static IEnumerable<byte[]> ReadAll(IEnumerable<string> fileUris) =>
            fileUris
                .Select(File.ReadAllBytes)
                .Retry<byte[], Exception>(new FixedInterval(3, TimeSpan.FromSeconds(1)));
    }
}

namespace Dixin.Linq.CSharp
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    internal static partial class Purity
    {
        [Pure]
        internal static bool IsPositive(int int32) => int32 > 0;

        internal static bool IsNegative(int int32) // Impure.
        {
            Trace.WriteLine(int32); // Side effect.
            return int32 < 0;
        }
    }

    [Pure]
    internal static class Pure
    {
        internal static int Increase(int int32) => int32 + 1;

        internal static int Decrease(int int32) => int32 - 1;
    }

    internal static partial class Purity
    {
        internal static int PureContracts(int int32)
        {
            Contract.Requires<ArgumentOutOfRangeException>(IsPositive(int32)); // Function precondition.
            Contract.Ensures(IsPositive(Contract.Result<int>())); // Function post condition.

            return int32 + 0; // Function logic.
        }

        internal static int ImpureContracts(int int32)
        {
            Contract.Requires<ArgumentOutOfRangeException>(IsNegative(int32)); // Function precondition.
            Contract.Ensures(IsNegative(Contract.Result<int>())); // Function post condition.

            return int32 + 0; // Function logic.
        }

        internal static void PureFunction(string contractsAssemblyDirectory)
        {
            string[] contractAssemblyFiles = Directory
                .EnumerateFiles(contractsAssemblyDirectory, "*.dll")
                .ToArray();
            string pureAttribute = typeof(PureAttribute).FullName;
            // Query the count of all public function members with [Pure] in all public class in all contract assemblies.
            int pureFunctionCount = contractAssemblyFiles
                .Select(assemblyContractFile => AssemblyDefinition.ReadAssembly(assemblyContractFile))
                .SelectMany(assemblyContract => assemblyContract.Modules)
                .SelectMany(moduleContract => moduleContract.GetTypes())
                .Where(typeContract => typeContract.IsPublic)
                .SelectMany(typeContract => typeContract.GetMethods())
                .Count(functionMemberContract => functionMemberContract.IsPublic
                    && functionMemberContract.CustomAttributes.Any(attribute =>
                        attribute.AttributeType.FullName.Equals(pureAttribute, StringComparison.Ordinal)));
            Trace.WriteLine(pureFunctionCount); // 2472

            string gacDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"Microsoft.Net\assembly");
            string[] assemblyFiles = new string[] { "GAC_64", "GAC_MSIL" }
                .Select(platformDirectory => Path.Combine(gacDirectory, platformDirectory))
                .SelectMany(assemblyDirectory => Directory
                    .EnumerateFiles(assemblyDirectory, "*.dll", SearchOption.AllDirectories))
                .ToArray();
            // Query the count of all public function members in all public class in all FCL assemblies.
            int functionCount = contractAssemblyFiles
                .Select(contractAssemblyFile => assemblyFiles.First(assemblyFile => Path.GetFileName(contractAssemblyFile)
                    .Replace(".Contracts", string.Empty)
                    .Equals(Path.GetFileName(assemblyFile), StringComparison.OrdinalIgnoreCase)))
                .Select(assemblyFile => AssemblyDefinition.ReadAssembly(assemblyFile))
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.GetTypes())
                .Where(type => type.IsPublic)
                .SelectMany(type => type.GetMethods())
                .Count(functionMember => functionMember.IsPublic);
            Trace.WriteLine(functionCount); // 74127
        }

        [Pure] // Incorrect.
        internal static ProcessStartInfo Initialize(ProcessStartInfo processStart)
        {
            processStart.UseShellExecute = true;
            processStart.ErrorDialog = true;
            processStart.WindowStyle = ProcessWindowStyle.Normal;
            return processStart;
        }
    }
}

#if DEMO
namespace System
{
    public static class Math
    {
        public static int Abs(int value)
        {
            if (value >= 0)
            {
                return value;
            }
            else
            {
                if (value == int.MinValue)
                {
                    throw new OverflowException("Negating the mimimum value of a twos compliment nuumber is invalid.");
                }
                return -value;
            }
        }
    }
}

namespace System
{
    using System.Diagnostics.Contracts;

    public static class Math
    {
        [Pure]
        public static int Abs(int value)
        {
            Contract.Requires(value != int.MinValue);
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures((value - Contract.Result<int>()) <= 0);

            return default(int);
        }
    }
}
#endif

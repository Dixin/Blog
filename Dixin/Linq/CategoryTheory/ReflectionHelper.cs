namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;

    public static partial class ReflectionHelper
    {
        public static IEnumerable<MethodDefinition> GetMethods
            (string assemblyPath, bool isPublicOnly) =>
                from module in AssemblyDefinition.ReadAssembly(assemblyPath).Modules
                from type in module.Types
                from method in type.Methods
                where !isPublicOnly || method.IsPublic
                select method;
    }

    public static partial class ReflectionHelper
    {
        public static IEnumerable<MethodDefinition> GetMethods<TAttribute>
            (string assemblyPath, bool isPublicOnly)
            where TAttribute : Attribute =>
                from method in GetMethods(assemblyPath, isPublicOnly)
                where method.CustomAttributes.Any(attribute => attribute.AttributeType.FullName.Equals(
                    typeof(TAttribute).FullName, StringComparison.Ordinal))
                select method;
    }

    public static partial class ReflectionHelper
    {
#if ERROR
        public static IEnumerable<MethodInfo> GetMethods<TAttribute>
            (string assemblyPath, bool isPublicOnly)
            where TAttribute : Attribute =>
                from type in Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath)).GetTypes()
                from method in type.GetMethods()
                where (!isPublicOnly || method.IsPublic)
                        && method.GetCustomAttributes(typeof(TAttribute), false).Any()
                select method;
#endif
    }
}

namespace Tutorial.Introduction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    internal static partial class Linq
    {
        internal static void Dynamic()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 }; // Get source.
            IEnumerable<dynamic> query =
                from dynamic value in source
                where value.ByPass.Compiler.Check > 0
                orderby value.ByPass().Compiler().Check()
                select value & new object(); // Define query.
            foreach (dynamic result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
        }
    }

    internal static partial class Linq
    {
        internal static void DelegateTypesQueryExpression()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            IEnumerable<IGrouping<string, Type>> delegateTypes =
                from type in coreLibrary.GetExportedTypes()
                where type.BaseType == typeof(MulticastDelegate)
                group type by type.Namespace into namespaceTypes
                orderby namespaceTypes.Count() descending, namespaceTypes.Key
                select namespaceTypes;
            foreach (IGrouping<string, Type> namespaceTypes in delegateTypes) // Output.
            {
                Trace.Write(namespaceTypes.Count() + " " + namespaceTypes.Key + ":");
                foreach (Type delegateType in namespaceTypes)
                {
                    Trace.Write(" " + delegateType.Name);
                }
                Trace.WriteLine(null);
            }
        }
    }

    internal static partial class Linq
    {
        internal static void DelegateTypesQueryMethods()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            IEnumerable<IGrouping<string, Type>> delegateTypes = coreLibrary.GetExportedTypes()
                .Where(type => type.BaseType == typeof(MulticastDelegate))
                .GroupBy(type => type.Namespace)
                .OrderByDescending(namespaceTypes => namespaceTypes.Count())
                .ThenBy(namespaceTypes => namespaceTypes.Key);
            foreach (IGrouping<string, Type> namespaceTypes in delegateTypes) // Output.
            {
                Trace.Write(namespaceTypes.Count() + " " + namespaceTypes.Key + ":");
                foreach (Type delegateType in namespaceTypes)
                {
                    Trace.Write(" " + delegateType.Name);
                }
                Trace.WriteLine(null);
            }
        }
    }

    internal partial class Linq
    {
        internal static void CompiledDelegateTypes()
        {
            Assembly coreLibrary = typeof(object).Assembly;

            Func<Type, bool> filterPredicateFunction = type => type.BaseType == typeof(MulticastDelegate);
            IEnumerable<Type> filterQuery = Enumerable.Where(coreLibrary.GetExportedTypes(), filterPredicateFunction);

            Func<Type, string> groupKeySelectorFunction = type => type.Namespace;
            IEnumerable<IGrouping<string, Type>> groupQuery = Enumerable.GroupBy(filterQuery, groupKeySelectorFunction);

            Func<IGrouping<string, Type>, int> orderKeySelectorFunction1 = namespaceTypes => namespaceTypes.Count();
            IOrderedEnumerable<IGrouping<string, Type>> orderQuery1 = Enumerable.OrderByDescending(
                groupQuery, orderKeySelectorFunction1);

            Func<IGrouping<string, Type>, string> orderKeySelectorFunction2 = namespaceTypes => namespaceTypes.Key;
            IEnumerable<IGrouping<string, Type>> orderQuery2 = Enumerable.ThenBy(orderQuery1, orderKeySelectorFunction2);

            foreach (IGrouping<string, Type> namespaceTypes in orderQuery2) // Output.
            {
                Trace.Write(namespaceTypes.Count() + " " + namespaceTypes.Key + ":");
                foreach (Type delegateType in namespaceTypes)
                {
                    Trace.Write(" " + delegateType.Name);
                }
                Trace.WriteLine(null);
            }
        }
    }
}

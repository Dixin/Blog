namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> AssignableTo<TType>(this Assembly assembly)
        {
            return from type in assembly.ExportedTypes
                   where typeof(TType).IsAssignableFrom(type)
                   select type;
        }
    }
}

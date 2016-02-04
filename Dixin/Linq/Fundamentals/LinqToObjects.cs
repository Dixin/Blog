namespace Dixin.Linq.Fundamentals
{
    using System.Collections.Generic;
    using System.Linq;

    internal partial class LinqToObjects
    {
        internal static IEnumerable<Person> FilterAndOrderByAge(IEnumerable<Person> source)
        {
            return source
                .Where(person => person.Age >= 18)
                .OrderByDescending(person => person.Age)
                .ThenBy(person => person.Name);
        }
    }

    internal static partial class LinqToObjects
    {
        internal static IEnumerable<int> Positive(IEnumerable<int> source)
        {
            return from value in source
                   where value > 0
                   select value;
        }
    }

    internal static partial class LinqToObjects
    {
        internal static IEnumerable<dynamic> Dynamic(IEnumerable<dynamic> source)
        {
            return from value in source
                   where value.NotChecked > new object()
                   select value.ByCompiler;
        }
    }

    internal partial class LinqToObjects
    {
        internal static IEnumerable<string> FilterAndSort(IEnumerable<Person> source)
        {
            return source
                .Where(person => person.Age >= 18)
                .OrderByDescending(person => person.Age)
                .ThenBy(person => person.Name)
                .Select(person => person.Name);
        }
    }
}

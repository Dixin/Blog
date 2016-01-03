namespace Dixin.Linq.Fundamentals
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class LinqToObjects
    {
        public static IEnumerable<Person> FilterAndOrderByAge(IEnumerable<Person> source)
        {
            return source
                .Where(person => person.Age >= 18)
                .OrderByDescending(person => person.Age)
                .ThenBy(person => person.Name);
        }
    }

    public static partial class LinqToObjects
    {
        public static IEnumerable<int> Positive(IEnumerable<int> source)
        {
            return from value in source
                   where value > 0
                   select value;
        }
    }

    public static partial class LinqToObjects
    {
        public static IEnumerable<dynamic> Dynamic(IEnumerable<dynamic> source)
        {
            return from value in source
                   where value.NotChecked > new object()
                   select value.ByCompiler;
        }
    }

    public partial class LinqToObjects
    {
        public static IEnumerable<string> FilterAndSort(IEnumerable<Person> source)
        {
            return source
                .Where(person => person.Age >= 18)
                .OrderByDescending(person => person.Age)
                .ThenBy(person => person.Name)
                .Select(person => person.Name);
        }
    }
}

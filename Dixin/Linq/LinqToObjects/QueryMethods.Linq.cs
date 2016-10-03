namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web.Profile;

    using Dixin.Linq.CSharp;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    internal class Person
    {
        internal Person(string name, string placeOfBirth)
        {
            this.Name = name;
            this.PlaceOfBirth = placeOfBirth;
        }

        internal string Name { get; }

        internal string PlaceOfBirth { get; }
    }

    internal static partial class QueryMethods
    {
        internal static IEnumerable<Person> Persons() => new Person[]
            {
                new Person(name: "Robert Downey Jr.", placeOfBirth: "US"),
                new Person(name:  "Tom Hiddleston", placeOfBirth: "UK"),
                new Person(name: "Chris Hemsworth", placeOfBirth: "AU"),
                new Person(name: "Chris Evans", placeOfBirth: "US"),
                new Person(name: "Paul Bettany", placeOfBirth:  "UK")
            };
    }

    internal partial class Character
    {
        internal Character(string name, string placeOfBirth, string starring)
        {
            this.Name = name;
            this.PlaceOfBirth = placeOfBirth;
            this.Starring = starring;
        }

        internal string Name { get; }

        internal string PlaceOfBirth { get; }

        internal string Starring { get; }
    }

    internal static partial class QueryMethods
    {
        internal static IEnumerable<Character> Characters() => new Character[]
            {
                new Character(name: "Tony Stark", placeOfBirth: "US", starring: "Robert Downey Jr."),
                new Character(name:  "Thor", placeOfBirth:  "Asgard" , starring: "Chris Hemsworth"),
                new Character(name:  "Steve Rogers", placeOfBirth:  "US", starring: "Chris Evans" ),
                new Character(name:  "Vision", placeOfBirth:  "KR", starring: "Paul Bettany" ),
                new Character(name:  "JARVIS", placeOfBirth: "US", starring: "Paul Bettany")
            };
    }

    internal static partial class QueryMethods
    {
        internal static void Empty()
        {
            IEnumerable<string> empty = Enumerable.Empty<string>(); // Define query.
            int count = 0;
            foreach (string result in empty) // Execute query by pulling the results.
            {
                count++; // Not executed.
            }
            Trace.WriteLine(count); // 0
        }
    }

    internal static partial class QueryMethods
    {
        #region Generation

        internal static void Range()
        {
            IEnumerable<int> range = Enumerable.Range(-1, 5); // Define query.
            foreach (int int32 in range) // Execute query.
            {
                Trace.WriteLine(int32); // -1 0 1 2 3
            }
        }

        internal static void LargeRange()
        {
            IEnumerable<int> range = Enumerable.Range(int.MinValue, int.MaxValue); // Define query.
        }

        internal static void Repeat()
        {
            IEnumerable<int> repeat = Enumerable.Repeat(1, 5); // Define query.
            foreach (int int32 in repeat) // Execute query.
            {
                Trace.WriteLine(int32); // 1 1 1 1 1
            }
        }

        internal static void DefaultIfEmpty()
        {
            IEnumerable<int> souce = Enumerable.Empty<int>();
            IEnumerable<int> singletonIfEmpty = souce.DefaultIfEmpty();
            foreach (int int32 in singletonIfEmpty)
            {
                Trace.WriteLine(int32); // 0
            }
        }

        internal static void DefaultIfEmptyWithDefaultValue()
        {
            IEnumerable<int> souce = Enumerable.Empty<int>();
            IEnumerable<int> singletonIfEmpty = souce.DefaultIfEmpty(1);
            foreach (int int32 in singletonIfEmpty)
            {
                Trace.WriteLine(int32); // 1
            }
        }

        #endregion

        #region Filtering

        private static readonly Assembly mscorlib = typeof(object).Assembly;

        internal static void Where()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<Type> primitives = source.Where(type => type.IsPrimitive); // Define query.
            foreach (Type primitive in primitives) // Execute query.
            {
                Trace.WriteLine(primitive);
                // System.Boolean 
                // System.Byte
                // System.Char
                // System.Double
                // ...
            }
        }

        internal static void WhereWithIndex()
        {
            IEnumerable<string> source = new string[] { "zero", "one", "two", "three", "four" };
            IEnumerable<string> evenNumbers = source.Where((value, index) => index % 2 == 0); // Define query.
            foreach (string even in evenNumbers) // Execute query.
            {
                Trace.WriteLine(even); // zero two four
            }
        }

        internal static void OfType()
        {
            IEnumerable<object> source = new object[] { 1, 2, 'a', 'b', "aa", "bb", new object() };
            IEnumerable<string> strings = source.OfType<string>();  // Define query.
            foreach (string @string in strings) // Execute query.
            {
                Trace.WriteLine(@string);  // aa bb
            }
        }

        #endregion

        #region Mapping

        internal static void Select()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5);
            IEnumerable<string> squareRoots = source.Select(int32 => $"{Math.Sqrt(int32):0.00}"); // Define query.
            foreach (string squareRoot in squareRoots) // Execute query.
            {
                Trace.WriteLine(squareRoot); // 0.00 1.00 1.41 1.73 2.00
            }
        }

        internal static IEnumerable<string> Words() => new string[] { "Zero", "one", "Two", "three", "four" };

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal static void SelectWithIndex()
        {
            IEnumerable<string> source = Words();
            var mapped = source.Select((value, index) => new
            {
                Index = index,
                Word = value.ToLowerInvariant()
            }); // Define query: IEnumerable<(string Word, int Index)>
            foreach (var value in mapped) // Execute query.
            {
                Trace.WriteLine($"{value.Index}:{value.Word}"); // 0:zero 1:one 2:two 3:three 4:four
            }
        }

        internal static void Let()
        {
            IEnumerable<int> source = Enumerable.Range(-2, 5);
            IEnumerable<string> absoluteValues = source
                .Select(int32 => new { int32 = int32, abs = Math.Abs(int32) })
                .Where(tuple => tuple.abs > 0)
                .Select(tuple => $"Math.Abs({tuple.int32}) == {tuple.abs}");
            foreach (string absoluteValue in absoluteValues)
            {
                Trace.WriteLine(absoluteValue);
                // Math.Abs(-2) == 2
                // Math.Abs(-1) == 1
                // Math.Abs(1) == 1
                // Math.Abs(2) == 2
            }
        }

        internal static MemberInfo[] GetDeclaredMembers(this Type type) => type.GetMembers(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        internal static bool IsObsolete(this MemberInfo member) => 
            Attribute.IsDefined(member, typeof(ObsoleteAttribute), false);

        internal static void SelectMany()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<MemberInfo> mapped = source.SelectMany(type => type.GetDeclaredMembers()); // Define query.
            IEnumerable<MemberInfo> filtered = mapped.Where(member => member.IsObsolete()); // Define query.
            foreach (MemberInfo obsoleteMember in filtered) // Execute query.
            {
                Trace.WriteLine($"{obsoleteMember.DeclaringType} - {obsoleteMember}");
                // ...
                // System.Enum - System.String ToString(System.String, System.IFormatProvider)
                // System.Enum - System.String ToString(System.IFormatProvider)
                // ...
            }
        }

        internal static void FluentSelectMany()
        {
            IEnumerable<MemberInfo> mappedAndFiltered = mscorlib
                .GetExportedTypes()
                .SelectMany(type => type.GetDeclaredMembers())
                .Where(member => member.IsObsolete()); // Define query.
            foreach (MemberInfo obsoleteMember in mappedAndFiltered) // Execute query.
            {
                Trace.WriteLine($"{obsoleteMember.DeclaringType} - {obsoleteMember}");
            }
        }

        internal static void SelectManyWithResultSelector()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<string> obsoleteMembers = source.SelectMany(
                collectionSelector: type => type.GetDeclaredMembers().Where(member => member.IsObsolete()),
                resultSelector: (type, obsoleteMember) => $"{type} - {obsoleteMember}"); // Define query.
            foreach (string obsoleteMember in obsoleteMembers) // Execute query.
            {
                Trace.WriteLine(obsoleteMember);
            }
        }

        internal static void CompiledSelectManyWithResultSelector()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<string> obsoleteMembers = mscorlib
                .GetExportedTypes() // IEnumerable<Type>
                .SelectMany(
                    collectionSelector: type => type.GetDeclaredMembers(),
                    resultSelector: (type, member) => new { Type = type, Member = member }) // IEnumerable<(Type Type, MemberInfo Member)>
                .Where(member => member.Member.IsObsolete()) // IEnumerable<(Type Type, MemberInfo Member)>
                .Select(member => $"{member.Type} - {member.Member}"); // IEnumerable<string>
        }

        #endregion

        #region Grouping

        internal static void GroupBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = source.GroupBy(person => person.PlaceOfBirth); // Define query.
            foreach (IGrouping<string, Person> group in groups) // Execute query.
            {
                Trace.Write($"{group.Key}:");
                foreach (Person person in group)
                {
                    Trace.Write($"{person.Name}, ");
                }
                Trace.WriteLine(null);
                // US: Robert Downey Jr., Chris Evans,
                // UK: Tom Hiddleston, Paul Bettany,
                // AU: Chris Hemsworth,
            }
        }

        internal static void GroupByWithResultSelector()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = source
                .GroupBy(
                    keySelector: person => person.PlaceOfBirth,
                    resultSelector: (key, group) => $"{key}: {group.Count()}"); // Define query.
            foreach (string group in groups) // Execute query.
            {
                Trace.WriteLine(group);
                // US: 2
                // UK: 2
                // AU: 1
            }
        }

        internal static void GroupByAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = source.GroupBy(person => person.PlaceOfBirth);
            IEnumerable<string> mapped = groups.Select(group => $"{group.Key}: {group.Count()}"); // Define query.
            foreach (string group in mapped) // Execute query.
            {
                Trace.WriteLine(group);
                // US: 2
                // UK: 2
                // AU: 1
            }
        }

        internal static void FluentGroupByAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = source
                .GroupBy(person => person.PlaceOfBirth)
                .Select(group => $"{group.Key}: {group.Count()}"); // Define query.
        }

        internal static void GroupByWithElementSelector()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, string>> groups = source
                .GroupBy(
                    keySelector: person => person.PlaceOfBirth,
                    elementSelector: person => person.Name); // Define query.
            foreach (IGrouping<string, string> group in groups) // Execute query.
            {
                Trace.Write($"{group.Key}:");
                foreach (string name in group)
                {
                    Trace.Write($"{name}, ");
                }
                Trace.WriteLine(null);
                // US: Robert Downey Jr., Chris Evans,
                // UK: Tom Hiddleston, Paul Bettany,
                // AU: Chris Hemsworth,
            }
        }

        internal static void GroupByWithElementAndResultSelector()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = source.GroupBy(
                keySelector: person => person.PlaceOfBirth,
                elementSelector: person => person.Name,
                resultSelector: (key, group) => $"{key}: {string.Join(", ", group)}"); // Define query.
            foreach (string group in groups) // Execute query.
            {
                Trace.WriteLine(group);
                // US: Robert Downey Jr., Chris Evans
                // UK: Tom Hiddleston, Paul Bettany
                // AU: Chris Hemsworth
            }
        }

        internal static void GroupByWithEqualityComparer()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = source.GroupBy(
                keySelector: person => person.PlaceOfBirth,
                elementSelector: person => person.Name,
                resultSelector: (key, group) => $"{key}: {string.Join(",", group)}",
                comparer: StringComparer.OrdinalIgnoreCase); // Define query.
            foreach (string group in groups) // Execute query.
            {
                Trace.WriteLine(group);
                // US: 2
                // UK: 2
                // AU: 1
            }
        }

        #endregion

        #region Join

        internal static void InnerJoin()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin = outer.Join(
                inner: inner,
                outerKeySelector: person => person.Name,
                innerKeySelector: character => character.Starring, // on person.Name equal character.Starring
                resultSelector: (person, character) => $"{person.Name} ({person.PlaceOfBirth}): {character.Name}"); // Define query.
            foreach (string value in innerJoin) // Execute query.
            {
                Trace.WriteLine(value);
                // Robert Downey Jr. (US): Tony Stark
                // Chris Hemsworth (AU): Thor
                // Chris Evans (US): Steve Rogers
                // Paul Bettany (UK): Vision
                // Paul Bettany (UK): JARVIS
            }
        }

        internal static void InnerJoinWithSelectMany()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin = outer
                .SelectMany(
                    collectionSelector: person => inner,
                    resultSelector: (person, character) => new { Person = person, Character = character })
                .Where(crossJoinValue => EqualityComparer<string>.Default.Equals(
                    crossJoinValue.Person.Name, crossJoinValue.Character.Starring))
                .Select(innerJoinValue =>
                    $"{innerJoinValue.Person.Name} ({innerJoinValue.Person.PlaceOfBirth}): {innerJoinValue.Character.Name}");
            // Define query.
            foreach (string value in innerJoin) // Execute query.
            {
                Trace.WriteLine(value);
                // Robert Downey Jr. (US): Tony Stark
                // Chris Hemsworth (AU): Thor
                // Chris Evans (US): Steve Rogers
                // Paul Bettany (UK): Vision
                // Paul Bettany (UK): JARVIS
            }
        }

        internal static IEnumerable<TResult> InnerJoinWithSelectMany<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            return outer
                .SelectMany(
                    collectionSelector: outerValue => inner,
                    resultSelector: (outerValue, innerValue) => new { OuterValue = outerValue, InnerValue = innerValue })
                .Where(
                    crossJoinValue => comparer.Equals(
                        outerKeySelector(crossJoinValue.OuterValue),
                        innerKeySelector(crossJoinValue.InnerValue)))
                .Select(innerJoinValue => resultSelector(innerJoinValue.OuterValue, innerJoinValue.InnerValue));
        }

        internal static void InnerJoinWithMultipleKeys()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin = outer.Join(
                inner: inner,
                outerKeySelector: person => new { Starring = person.Name, PlaceOfBirth = person.PlaceOfBirth },
                innerKeySelector: character => new { Starring = character.Starring, PlaceOfBirth = character.PlaceOfBirth },
                // on new { Starring = person.Name, PlaceOfBirth = person.PlaceOfBirth } equal new { Starring = character.Starring, PlaceOfBirth = character.PlaceOfBirth }
                resultSelector: (person, character) =>
                    $"{person.Name} ({person.PlaceOfBirth}): {character.Name} ({character.PlaceOfBirth})"); // Define query.
            foreach (string value in innerJoin) // Execute query.
            {
                Trace.WriteLine(value);
                // Robert Downey Jr. (US): Tony Stark (US)
                // Chris Evans (US): Steve Rogers (US)
            }
        }

        internal static void LeftOuterJoin()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin = outer.GroupJoin(
                inner: inner,
                outerKeySelector: person => person.Name,
                innerKeySelector: character => character.Starring, // on person.Name equal character.Starring
                resultSelector: (person, charactersGroup) => new { Person = person, Characters = charactersGroup }); // Define query.
            foreach (var result in leftOuterJoin) // Execute query.
            {
                Trace.Write($"{result.Person.Name} ({result.Person.PlaceOfBirth}): ");
                foreach (Character character in result.Characters)
                {
                    Trace.Write($"{character.Name} ({character.PlaceOfBirth}), ");
                }
                Trace.WriteLine(null);
                // Robert Downey Jr. (US): Tony Stark (US),
                // Tom Hiddleston (UK):
                // Chris Hemsworth (AU): Thor (Asgard),
                // Chris Evans (US): Steve Rogers (US),
                // Paul Bettany (UK): Vision (KR), JARVIS (US),
            }
        }

        internal static void LeftOuterJoinWithDefaultIfEmpty()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin = outer
                .GroupJoin(
                    inner: inner,
                    outerKeySelector: person => person.Name,
                    innerKeySelector: character => character.Starring,
                    resultSelector: (person, charactersGroup) => new { Person = person, Characters = charactersGroup })
                .SelectMany(
                    collectionSelector: group => group.Characters.DefaultIfEmpty(),
                    resultSelector: (group, character) => new { Person = group.Person, Character = character });
            foreach (var result in leftOuterJoin)
            {
                Trace.WriteLine($"{result.Person.Name}: {result.Character?.Name}");
                // Robert Downey Jr.: Tony Stark
                // Tom Hiddleston:
                // Chris Hemsworth: Thor
                // Chris Evans: Steve Rogers
                // Paul Bettany: Vision
                // Paul Bettany: JARVIS
            }
        }

        internal static void LeftOuterJoinWithSelect()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin = outer.Select(person => new
            {
                Person = person,
                Characters = inner.Where(character =>
                    EqualityComparer<string>.Default.Equals(person.Name, character.Starring))
            }); // Define query.
            foreach (var result in leftOuterJoin) // Execute query.
            {
                Trace.Write($"{result.Person.Name} ({result.Person.PlaceOfBirth}): ");
                foreach (Character character in result.Characters)
                {
                    Trace.Write($"{character.Name} ({character.PlaceOfBirth}), ");
                }
                Trace.WriteLine(null);
                // Robert Downey Jr. (US): Tony Stark (US),
                // Tom Hiddleston (UK):
                // Chris Hemsworth (AU): Thor (Asgard),
                // Chris Evans (US): Steve Rogers (US),
                // Paul Bettany (UK): Vision (KR), JARVIS (US),
            }
        }

        internal static IEnumerable<TResult> LeftOuterJoinWithSelect<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            return outer.Select(outerValue => resultSelector(
                outerValue,
                inner.Where(
                    innerValue => comparer.Equals(outerKeySelector(outerValue), innerKeySelector(innerValue)))));
        }

        private static readonly int[] rows = { 1, 2, 3 };

        private static readonly string[] columns = { "A", "B", "C", "D" };

        internal static void CrossJoin()
        {
            IEnumerable<string> cells = rows
                .SelectMany(row => columns, (row, column) => $"{column}{row}"); // Define query.

            int cellIndex = 0;
            int columnCount = columns.Length;
            foreach (string cell in cells) // Execute query.
            {
                Trace.Write($"{cell} ");
                if (cellIndex++ > 0 && cellIndex % columnCount == 0)
                {
                    Trace.WriteLine(null);
                }
                // A1 B1 C1 D1
                // A2 B2 C2 D2
                // A3 B3 C3 D3
            }
        }

        internal static void CrossJoinWithJoin()
        {
            IEnumerable<string> cells = rows.Join(
                inner: columns,
                outerKeySelector: row => true,
                innerKeySelector: column => true, // on true equal true
                resultSelector: (row, column) => $"{column}{row}"); // Define query.
            int cellIndex = 0;
            int columnCount = columns.Length;
            foreach (string cell in cells) // Execute query.
            {
                Trace.Write($"{cell} ");
                if (cellIndex++ > 0 && cellIndex % columnCount == 0)
                {
                    Trace.WriteLine(null);
                }
            }
        }

        internal static IEnumerable<TResult> CrossJoinWithJoin<TOuter, TInner, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TInner, TResult> resultSelector) =>
                outer.Join(
                    inner: inner,
                    outerKeySelector: outerValue => true,
                    innerKeySelector: innerValue => true, // on true equal true
                    resultSelector: resultSelector);

        #endregion

        #region Concatenation

        internal static int[] First() => new int[] { 1, 2, 3, 4, 4 };

        internal static int[] Second() => new int[] { 3, 4, 5, 6 };

        internal static void Concat()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> second = Second();
            IEnumerable<int> concat = first.Concat(second); // Define query.
            foreach (int int32 in concat) // Execute query.
            {
                Trace.WriteLine(int32);// 1 2 3 4 4 3 4 5 6
            }
        }

        #endregion

        #region Set

        internal static void Distinct()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> distinct = first.Distinct(); // Define query.
            foreach (int int32 in distinct) // Execute query.
            {
                Trace.WriteLine(int32); // 1 2 3 4
            }
        }

        internal static void Union()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> second = Second();
            IEnumerable<int> union = first.Union(second); // Define query.
            foreach (int int32 in union) // Execute query.
            {
                Trace.WriteLine(int32); // 1 2 3 4 5 6
            }
        }

        internal static void Intersect()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> second = Second();
            IEnumerable<int> intersect = first.Intersect(second); // Define query.
            foreach (int int32 in intersect) // Execute query.
            {
                Trace.WriteLine(int32); // 3 4
            }
        }

        internal static void Except()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> second = Second();
            IEnumerable<int> except = first.Except(second); // Define query.
            foreach (int int32 in except) // Execute query.
            {
                Trace.WriteLine(int32); // 1 2
            }
        }

        internal static void DistinctWithComparer()
        {
            IEnumerable<string> source = new string[] { "aa", "AA", "Aa", "aA", "bb" };
            IEnumerable<string> distinctWithComparer = source.Distinct(StringComparer.OrdinalIgnoreCase);
            // Define query.
            foreach (string @string in distinctWithComparer) // Execute query.
            {
                Trace.WriteLine(@string); // aa bb
            }
        }

        #endregion

        #region Convolution

        internal static void Zip()
        {
            IEnumerable<int> first = First();
            IEnumerable<int> second = Second();
            IEnumerable<int> zip = first.Zip(second, (a, b) => a + b); // Define query.
            foreach (int int32 in zip) // Execute query.
            {
                Trace.WriteLine(int32); // 4 6 8 10
            }
        }

        #endregion

        #region Partitioning

        internal static void SkipTake()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5); // Define query.

            IEnumerable<int> partition1 = source.Skip(2); // Define query.
            foreach (int int32 in partition1) // Execute query.
            {
                Trace.WriteLine(int32); // 2 3 4
            }

            IEnumerable<int> partition2 = source.Take(2); // Define query.
            foreach (int int32 in partition2) // Execute query.
            {
                Trace.WriteLine(int32); // 0 1
            }
        }

        internal static void TakeWhileSkipWhile()
        {
            IEnumerable<int> source = new int[] { 1, 2, 3, -1, 4, 5 };

            IEnumerable<int> partition1 = source.TakeWhile(int32 => int32 > 0); // Define query.
            foreach (int int32 in partition1) // Execute query.
            {
                Trace.WriteLine(int32); // 1 2 3
            }

            IEnumerable<int> partition2 = source.SkipWhile(int32 => int32 > 0); // Define query.
            foreach (int int32 in partition2) // Execute query.
            {
                Trace.WriteLine(int32); // -1 4 5
            }
        }

        internal static void TakeWhileSkipWhileWithIndex()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 5 };

            IEnumerable<int> partition1 = source.TakeWhile((int32, index) => int32 >= index); // Define query.
            foreach (int int32 in partition1) // Execute query.
            {
                Trace.WriteLine(int32); // 4 3 2
            }

            IEnumerable<int> partition2 = source.SkipWhile((int32, index) => int32 >= index); // Define query.
            foreach (int int32 in partition2) // Execute query.
            {
                Trace.WriteLine(int32); // 1 5
            }
        }

        #endregion

        #region Ordering

        internal static void OrderBy()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = source.OrderBy(word => word); // Define query.
            foreach (string word in ordered) // Execute query.
            {
                Trace.WriteLine(word); // four one three Two Zero
            }

            foreach (string word in source) // Original sequence.
            {
                Trace.WriteLine(word); // Zero one Two three four
            }
        }

        internal static void OrderByDescending()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = source.OrderByDescending(word => word); // Define query.
            foreach (string word in ordered) // Execute query.
            {
                Trace.WriteLine(word); // Zero Two three one four
            }

            foreach (string word in source) // Original sequence.
            {
                Trace.WriteLine(word); // Zero one Two three four
            }
        }

        internal static void OrderByWithComparer()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = source.OrderBy(
                keySelector: word => word, comparer: StringComparer.Ordinal); // Define query.
            foreach (string word in ordered) // Execute query.
            {
                Trace.WriteLine(word); // Two Zero four one three
            }
        }

        internal static void ThenBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = source // IEnumerable<Person>
                .OrderBy(person => person.PlaceOfBirth) // IOrderedEnumerable<Person>
                .ThenBy(person => person.Name); // IOrderedEnumerable<Person>
            foreach (Person person in ordered) // Execute query.
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
                // AU: Chris Hemsworth
                // UK: Paul Bettany
                // UK: Tom Hiddleston
                // US: Chris Evans
                // US: Robert Downey Jr.
            }
        }

        internal static void OrderByAndOrderBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = source
                .OrderBy(person => person.PlaceOfBirth)
                .OrderBy(person => person.Name); // Define query.
            foreach (Person person in ordered) // Execute query.
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
                // US: Chris Evans
                // AU: Chris Hemsworth
                // UK: Paul Bettany
                // US: Robert Downey Jr.
                // UK: Tom Hiddleston
            }
        }

        internal static void Reverse()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5);
            IEnumerable<int> reversed = source.Reverse(); // Define query.
            foreach (int int32 in reversed) // Execute query.
            {
                Trace.WriteLine(int32); // 4 3 2 1 0
            }
        }

        #endregion

        #region Conversion

        internal static void CastNonGenericIEnumerable(TfsClientCredentials credentials)
        {
            using (TfsTeamProjectCollection projectCollection = new TfsTeamProjectCollection(
                new Uri("https://dixin.visualstudio.com/DefaultCollection"), credentials))
            {
                const string wiql = "SELECT * FROM WorkItems WHERE [Work Item Type] = 'Bug' AND State != 'Closed'"; // WIQL does not support GROUP BY.
                WorkItemStore workItemStore = (WorkItemStore)projectCollection.GetService(typeof(WorkItemStore));
                WorkItemCollection workItems = workItemStore.Query(wiql); // WorkItemCollection implements IEnumerable.

                IEnumerable<WorkItem> genericWorkItems = workItems.Cast<WorkItem>(); // Define query.
                IEnumerable<IGrouping<string, WorkItem>> workItemGroups = genericWorkItems
                    .GroupBy(workItem => workItem.CreatedBy); // Group work items in local memory.
                // ...
            }
        }

        internal static void CastNonGenericIEnumerable2()
        {
            SettingsPropertyCollection properties = ProfileBase.Properties; // SettingsPropertyCollection implements IEnumerable.
            IEnumerable<SettingsProperty> genericProperties = properties.Cast<SettingsProperty>();
        }

        internal static void CastGenericIEnumerable()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Derived() };
            IEnumerable<Derived> casted = source.Cast<Derived>(); // Define query.
            foreach (Derived derived in casted) // Execute query.
            {
                Trace.WriteLine(derived.GetType().Name); // Derived Derived
            }
        }

        internal static void CastGenericIEnumerableWithException()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Base() };
            IEnumerable<Derived> casted = source.Cast<Derived>(); // Define query.
            foreach (Derived derived in casted) // Execute query.
            {
                Trace.WriteLine(derived.GetType().Name); // Derived InvalidCastException
            }
        }

        internal static void CastWithJoin()
        {
            IEnumerable outer = new int[] { 1, 2, 3 };
            IEnumerable inner = new string[] { string.Empty, "a", "bb", "ccc", "dddd" };
            IEnumerable<string> innerJoin = outer.Cast<int>().Join(
                inner: inner.Cast<string>(),
                outerKeySelector: int32 => int32,
                innerKeySelector: @string => @string.Length, // on int32 equal @string.Length
                resultSelector: (int32, @string) => $"{int32}:{@string}"); // Define query.
            foreach (string value in innerJoin) // Execute query.
            {
                Trace.WriteLine(value); // 1:a 2:bb 3:ccc
            }
        }

        internal static void AsEnumerable()
        {
            List<int> list = new List<int>();
            list.Add(0);
            IEnumerable<int> sequence = list.AsEnumerable(); // enumerable does not have Add method.
        }

        internal static void AsEnumerableReverse()
        {
            List<int> list = new List<int>();
            list.Reverse(); // List<T>.Reverse.
            list
                .AsEnumerable() // IEnumerable<T>.
                .Reverse(); // Enumerable.Reverse.

            SortedSet<int> sortedSet = new SortedSet<int>();
            sortedSet.Reverse(); // SortedSet<T>.Reverse.
            sortedSet.AsEnumerable().Reverse(); // Enumerable.Reverse.

            ReadOnlyCollectionBuilder<int> readOnlyCollection = new ReadOnlyCollectionBuilder<int>();
            readOnlyCollection.Reverse(); // ReadOnlyCollectionBuilder<T>.Reverse.
            readOnlyCollection.AsEnumerable().Reverse(); // Enumerable.Reverse.

            IQueryable<int> queryable = new EnumerableQuery<int>(Enumerable.Empty<int>());
            queryable.Reverse(); // Queryable.Reverse.
            queryable.AsEnumerable().Reverse(); // Enumerable.Reverse.

            ImmutableList<int> immutableList = ImmutableList.Create(0);
            immutableList.Reverse(); // ImmutableSortedSet<T>.Reverse.
            immutableList.AsEnumerable().Reverse(); // Enumerable.Reverse.

            ImmutableSortedSet<int> immutableSortedSet = ImmutableSortedSet.Create(0);
            immutableSortedSet.Reverse(); // ImmutableSortedSet<T>.Reverse.
            immutableSortedSet.AsEnumerable().Reverse(); // Enumerable.Reverse.
        }

        internal static void ToArrayToList()
        {
            int[] array = Enumerable
                .Range(0, 5) // Define query.
                .ToArray(); // Execute query.

            List<int> list = Enumerable
                .Range(0, 5) // Define query.
                .ToList(); // Execute query.
        }

        internal static void ToDictionaryToLookup()
        {
            Dictionary<int, string> dictionary = Enumerable
                .Range(0, 5) // Define query.
                .ToDictionary(
                    int32 => int32,
                    int32 => Math.Sqrt(int32).ToString("F", CultureInfo.InvariantCulture)); // Execute query.
            foreach (KeyValuePair<int, string> squareRoot in dictionary)
            {
                Trace.WriteLine($"√{squareRoot.Key}: {squareRoot.Value}");
                // √0: 0.00
                // √1: 1.00
                // √2: 1.41
                // √3: 1.73
                // √4: 2.00
            }

            ILookup<int, int> lookup = Enumerable.Range(-2, 5)
                .Select(int32 => new { Int32 = int32, Square = int32 * int32 }) // Define query.
                .ToLookup(pair => pair.Square, pair => pair.Int32); // Execute query.
            foreach (IGrouping<int, int> squareRoots in lookup)
            {
                Trace.Write($"√{squareRoots.Key}: ");
                foreach (int squareRoot in squareRoots)
                {
                    Trace.Write($"{squareRoot}, ");
                }
                Trace.WriteLine(null);
                // √4: -2, 2,
                // √1: -1, 1,
                // √0: 0,
            }
        }

        internal static void ToDictionaryWithException()
        {
            Dictionary<int, int> lookup = Enumerable.Range(-2, 5)
                .Select(int32 => new { Int32 = int32, Square = int32 * int32 }) // Define query.
                .ToDictionary(pair => pair.Square, pair => pair.Int32); // Execute query.
            // ArgumentException: An item with the same key has already been added.
        }

        internal static void LookupDictionary()
        {
            ILookup<int, int> lookup = Enumerable
                .Range(0, 5) // Define query.
                .ToLookup(int32 => int32); // Execute query.
            int count = 0;
            IEnumerable<int> group = lookup[10];
            foreach (int _ in group)
            {
                count++;
            }
            Trace.WriteLine(count); // 0

            Dictionary<int, int> dictionary = Enumerable
                .Range(0, 5) // Define query.
                .ToDictionary(int32 => int32); // Execute query.
            int value = dictionary[10];
            // KeyNotFoundException: The given key was not present in the dictionary.
        }

        internal static void LookupDictionaryNullKey()
        {
            ILookup<string, string> lookup = new string[] { "a", "b", null }.ToLookup(@string => @string);
            int count = 0;
            IEnumerable<string> group = lookup[null];
            foreach (string _ in group)
            {
                count++;
            }
            Trace.WriteLine(count); // 1

            Dictionary<string, string> dictionary =
                new string[] { "a", "b", null }.ToDictionary(@string => @string);
            // ArgumentNullException: Value cannot be null. Parameter name: key.
        }

        internal static void ToLookupWithComparer()
        {
            ILookup<string, string> lookup = new string[] { "aa", "AA", "Aa", "aA", "bb" }
                .ToLookup(@string => @string, StringComparer.OrdinalIgnoreCase);
            foreach (IGrouping<string, string> group in lookup)
            {
                Trace.Write($"{group.Key}: ");
                foreach (string @string in group)
                {
                    Trace.Write($"{@string}, ");
                }
                Trace.WriteLine(null);
                // aa: aa, AA, Aa, aA,
                // bb: bb,
            }
        }

        #endregion

        #region Element

        internal static IEnumerable<int> Int32Source() => new int[] { -1, 1, 2, 3, -4 };

        internal static IEnumerable<int> SingleInt32Source() => Enumerable.Repeat(5, 1);

        internal static IEnumerable<int> EmptyInt32Source() => Enumerable.Empty<int>();

        internal static IEnumerable<int?> NullableInt32Source() => new int?[] { -1, 1, 2, 3, -4, null };

        internal static void FirstLast()
        {
            int firstOfSource = Int32Source().First(); // -1.
            int lastOfSource = Int32Source().Last(); // -4.

            int firstOfSingleSOurce = SingleInt32Source().First(); // 5.
            int lastOfSingleSOurce = SingleInt32Source().Last(); // 5.

            int firstOfEmptySOurce = EmptyInt32Source().First(); // InvalidOperationException.
            int lastOfEmptySOurce = EmptyInt32Source().Last(); // InvalidOperationException.
        }

        internal static void FirstLastWithPredicate()
        {
            int firstPositiveOfSource = Int32Source().First(int32 => int32 > 0); // 1.
            int lastNegativeOfSource = Int32Source().Last(int32 => int32 < 0); // -4.

            int firstPositiveOfSingleSOurce = SingleInt32Source().First(int32 => int32 > 0); // 1.
            int lastNegativeOfSingleSOurce = SingleInt32Source().Last(int32 => int32 < 0); // InvalidOperationException.

            int firstPositiveOfEmptySOurce = EmptyInt32Source().First(int32 => int32 > 0); // InvalidOperationException.
            int lastNegativeOfEmptySOurce = EmptyInt32Source().Last(int32 => int32 < 0); // InvalidOperationException.
        }

        internal static void FirstOrDefaultLastOrDefault()
        {
            int firstOrDefaultOfEmptySOurce = EmptyInt32Source().FirstOrDefault(); // 0.
            int lastOrDefaultOfEmptySOurce = EmptyInt32Source().LastOrDefault(); // 0.

            int lastNegativeOrDefaultOfSingleSOurce = SingleInt32Source().LastOrDefault(int32 => int32 < 0); // 0.

            int firstPositiveOrDefaultOfEmptySOurce = EmptyInt32Source().FirstOrDefault(int32 => int32 > 0); // 0.
            int lastNegativeOrDefaultOfEmptySOurce = EmptyInt32Source().LastOrDefault(int32 => int32 < 0); // 0.

            Character lokiOrDefault = Characters().FirstOrDefault(
                character => "Loki".Equals(character.Name, StringComparison.Ordinal)); // null.
        }

        internal static void ElementAt()
        {
            int elementAt2OfSource = Int32Source().ElementAt(2); // 2.
            int elementAt9OfSource = Int32Source().ElementAt(9); // ArgumentOutOfRangeException.
            int elementAtNegativeIndex = Int32Source().ElementAt(-5); // ArgumentOutOfRangeException.

            int elementAt0OfSingleSource = SingleInt32Source().ElementAt(0); // 5.
            int elementAt1OfSingleSource = SingleInt32Source().ElementAt(1); // ArgumentOutOfRangeException.

            int elementAt0OfEmptySource = EmptyInt32Source().ElementAt(0); // ArgumentOutOfRangeException.
        }

        internal static void ElementAtOrDefault()
        {
            int elementAt9OrDefaultOfSource = Int32Source().ElementAtOrDefault(9); // 0.
            int elementAtNegativeIndexOrDefault = Int32Source().ElementAtOrDefault(-5); // 0.

            int elementAt1OrDefaultOfSingleSource = SingleInt32Source().ElementAtOrDefault(1); // 0.

            int elementAt0OrDefaultOfEmptySource = EmptyInt32Source().ElementAtOrDefault(0); // 0.

            Character characterAt5OrDefault = Characters().ElementAtOrDefault(5); // null.
        }

        internal static void Single()
        {
            int singleOfSource = Int32Source().Single(); // InvalidOperationException.
            int singleGreaterThan2OfSource = Int32Source().Single(int32 => int32 > 2); // 3.
            int singleNegativeOfSource = Int32Source().Single(int32 => int32 < 0); // InvalidOperationException.

            int singleOfSingleSource = SingleInt32Source().Single(); // 5.
            int singleNegativeOfSingleSource = SingleInt32Source().Single(int32 => int32 < 0); // InvalidOperationException.

            int singleOfEmptySource = EmptyInt32Source().Single(); // InvalidOperationException.
            int singlePositiveOfEmptySource = EmptyInt32Source().Single(int32 => int32 == 0);  // InvalidOperationException.

            Character singleCharacter = Characters().Single(); // InvalidOperationException.
            Character fromAsgard = Characters().Single(
                character => "Asgard".Equals(character.PlaceOfBirth, StringComparison.Ordinal));  // Thor.

            Character loki = Characters().Single(
                character => "Loki".Equals(character.Name, StringComparison.Ordinal)); // InvalidOperationException.

        }

        internal static void SingleOrDefault()
        {
            int singleOrDefaultOfSource = Int32Source().SingleOrDefault(); // InvalidOperationException.
            int singleNegativeOrDefaultOfSource = Int32Source().SingleOrDefault(int32 => int32 < 0); // InvalidOperationException.

            int singleNegativeOrDefaultOfSingleSource = SingleInt32Source().SingleOrDefault(int32 => int32 < 0); // 0.

            int singleOrDefaultOfEmptySource = EmptyInt32Source().SingleOrDefault(); // 0.
            int singlePositiveOrDefaultOfEmptySource = EmptyInt32Source().SingleOrDefault(int32 => int32 == 0); // 0.

            Character singleCharacterOrDefault = Characters().SingleOrDefault(); // InvalidOperationException.
            Character lokiOrDefault = Characters().SingleOrDefault(
                character => "Loki".Equals(character.Name, StringComparison.Ordinal)); // null.

        }

        #endregion

        #region Aggregation

        internal static void Aggregate()
        {
            int productOfSource = Int32Source().Aggregate((currentProduct, int32) => currentProduct * int32); // ((((-1 * 1) * 2) * 3) * -4) = 24.
            int productOfSingleSource = SingleInt32Source().Aggregate(
                (currentProduct, int32) => currentProduct * int32); // 5.
            int productOfEmptySource = EmptyInt32Source().Aggregate(
                (currentProduct, int32) => currentProduct * int32); // InvalidOperationException.

        }

        internal static void AggregateWithSeed()
        {
            int sumOfSquaresOfSource = Int32Source().Aggregate(
                seed: 0,
                func: (currentSumOfSquares, int32) => currentSumOfSquares + int32 * int32); // 31.
            int sumOfSquaresOfSingleSource = SingleInt32Source().Aggregate(
                seed: 0,
                func: (currentSumOfSquares, int32) => currentSumOfSquares + int32 * int32); // 25.
            int sumOfSquaresOfEmptySource = EmptyInt32Source().Aggregate(
                seed: 0,
                func: (currentSumOfSquares, int32) => currentSumOfSquares + int32 * int32); // 0.
        }

        internal static void AggregateWithSeedAndResultSelector()
        {
            string sumOfSquaresMessage = Int32Source().Aggregate(
                seed: 0,
                func: (currentSumOfSquares, int32) => currentSumOfSquares + int32 * int32,
                resultSelector: result => $"Sum of squares: {result}"); // Sum of squares: 31.
        }

        internal static void Count()
        {
            int countOfSource = Int32Source().Count(); // 5.
            int countOfSingleSource = SingleInt32Source().Count(); // 1.
            int countOfEmptySource = EmptyInt32Source().Count(); // 0.
            int countOfCharacters = Characters().Count(); // 5.
            int countOfTypesInMscorlib = mscorlib.GetExportedTypes().Count(); // 1523
        }

        internal static void CountWithPredicate()
        {
            int positiveCountOfSource = Int32Source().Count(int32 => int32 > 0); // 3.
            int positiveCountOfSingleSource = SingleInt32Source().Count(int32 => int32 > 0); // 1.
            int positiveCountOfEmptySource = EmptyInt32Source().Count(int32 => int32 > 0); // 0.
            int countOfConcat = Enumerable
                .Repeat(0, int.MaxValue)
                .Concat(Enumerable.Repeat(0, int.MaxValue))
                .Count(); // OverflowException.
            int countOfCharactersFromUS = Characters().Count(character => "US".Equals(character.PlaceOfBirth)); // 3.
        }

        internal static void LongCount()
        {
            long longCountOfSource = Int32Source().LongCount(); // 5L.
            long countOfConcat = Enumerable
                .Repeat(0, int.MaxValue)
                .Concat(Enumerable.Repeat(0, int.MaxValue))
                .LongCount(); // int.MaxValue + int.MaxValue = 4294967294L.
        }

        internal static void MinMax()
        {
            int minOfSource = Int32Source().Min(); // -4.
            int maxOfSource = Int32Source().Max(); // 3.

            int minOfSingleSource = SingleInt32Source().Min(); // 5.
            int maxOfSingleSource = SingleInt32Source().Max(); // 5.

            int minOfEmptySource = EmptyInt32Source().Min(); // InvalidOperationException.
            int maxOfEmptySource = EmptyInt32Source().Max(); // InvalidOperationException.
        }

        internal static void MaxWithSelector()
        {
            decimal mostDeclaredMembers = mscorlib.GetExportedTypes()
                .Max(type => type.GetDeclaredMembers().Length); // 311.
        }

        internal static void OrderByDescendingAndTakeWhile()
        {
            int maxDeclaredMemberCount = 0;
            var typesWithMaxDeclaredMemberCount = mscorlib.GetExportedTypes()
                .Select(type => new { Type = type, Count = type.GetDeclaredMembers().Length })
                .OrderByDescending(typeWithDeclaredMemberCount => typeWithDeclaredMemberCount.Count)
                .TakeWhile(typeWithDeclaredMemberCount =>
                    {
                        if (maxDeclaredMemberCount == 0)
                        {
                            // If maxDeclaredMemberCount is not initialized, initialize it.
                            maxDeclaredMemberCount = typeWithDeclaredMemberCount.Count;
                        }
                        return typeWithDeclaredMemberCount.Count == maxDeclaredMemberCount;
                    }); // If more multiple types have the same max declared member count, take all those types.
            foreach (var typeWithMaxDeclaredMemberCount in typesWithMaxDeclaredMemberCount)
            {
                Trace.WriteLine($"{typeWithMaxDeclaredMemberCount.Type.FullName}: {typeWithMaxDeclaredMemberCount.Count}"); // System.Convert: 311.
            }
        }

        internal static void AggregateWithAnonymousTypeSeed()
        {
            var typesWithMaxDeclaredMemberCount = mscorlib.GetExportedTypes().Aggregate(
                seed: new
                {
                    Types = new List<Type>(),
                    MemberCount = 0
                },
                func: (currentMax, type) =>
                {
                    List<Type> currentMaxTypes = currentMax.Types;
                    int currentMaxDeclaredMemberCount = currentMax.MemberCount;
                    int declaredMemberCount = type.GetDeclaredMembers().Length;
                    if (declaredMemberCount > currentMaxDeclaredMemberCount)
                    {
                        currentMaxTypes = new List<Type>() { type };
                        currentMaxDeclaredMemberCount = declaredMemberCount;
                    }
                    else if (declaredMemberCount == currentMaxDeclaredMemberCount)
                    {
                        currentMaxTypes.Add(type);
                    }
                    return new
                    {
                        Types = currentMaxTypes,
                        MemberCount = currentMaxDeclaredMemberCount
                    };
                });
            foreach (Type typeWithMaxDeclaredMemberCount in typesWithMaxDeclaredMemberCount.Types)
            {
                Trace.WriteLine($"{typeWithMaxDeclaredMemberCount.FullName}: {typesWithMaxDeclaredMemberCount.MemberCount}"); // System.Convert: 311.
            }
        }

        internal static void Except2()
        {
            IEnumerable<MemberInfo> inheritedMembers = typeof(Convert).GetMembers()
                .Except(typeof(Convert).GetDeclaredMembers());
            foreach (MemberInfo inheritedMember in inheritedMembers)
            {
                Trace.WriteLine(inheritedMember);
                // System.String ToString()
                // Boolean Equals(System.Object)
                // Int32 GetHashCode()
                // System.Type GetType()
            }
        }

        internal static void MinMaxGeneric()
        {
            Character min = Characters().Min(); // JAVIS.
            Character max = Characters().Max(); // Vision.
        }

        internal static void SumAverage()
        {
            int sumOfSource = Int32Source().Sum(); // 1.
            double averageOfSource = Int32Source().Average(); // 0.2.

            int sumOfSingleSource = SingleInt32Source().Sum(); // 5.
            double averageOfSingleSource = SingleInt32Source().Average(); // 5.0.

            int sumOfEmptySource = EmptyInt32Source().Sum(); // 0.
            double averageOfEmptySource = EmptyInt32Source().Average(); // InvalidOperationException.
        }

        internal static void AverageWithSelector()
        {
            IEnumerable<Type> publicTypes = mscorlib.GetExportedTypes();
            decimal averagePublicMemberCount = publicTypes.Average(type => type.GetMembers(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Length); // 21.046618516086671.
            decimal averageDeclaredPublicMemberCount = publicTypes.Average(type => type.GetMembers(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length); // 11.661851608667105.
        }

        #endregion

        #region Quantifiers

        internal static void All()
        {
            bool allNegative = Int32Source().All(int32 => int32 < 0); // false.
            bool allPositive = SingleInt32Source().All(int32 => int32 > 0); // true.
            bool allGreaterThanMax = EmptyInt32Source().All(int32 => int32 > int.MaxValue); // true.
        }

        internal static void Any()
        {
            bool anyInSource = Int32Source().Any(); // true.
            bool anyInSingleSource = SingleInt32Source().Any(); // true.
            bool anyInEmptySource = EmptyInt32Source().Any(); // false.
        }

        internal static void AnyWithPredicate()
        {
            bool anyNegative = Int32Source().Any(int32 => int32 < 0); // true.
            bool anyPositive = SingleInt32Source().Any(int32 => int32 > 0); // true.
            bool any0 = EmptyInt32Source().Any(_ => true); // false.
        }

        internal static void Contains()
        {
            bool contains5InSource = Int32Source().Contains(5); // false.
            bool contains5InSingleSource = SingleInt32Source().Contains(5); // true.
            bool contains5InEmptySource = EmptyInt32Source().Contains(5); // false.
        }

        internal static void ContainsWithComparer()
        {
            bool containsTwo = Words().Contains("two", StringComparer.Ordinal); // false.
            bool containsTwoIgnoreCase = Words().Contains("two", StringComparer.OrdinalIgnoreCase); // true.
        }

        #endregion

        #region Equality

        internal static void SequentialEqual()
        {
            IEnumerable<object> first = new object[] { null, 1, "2", mscorlib };
            IEnumerable<object> second = new List<object>() { null, 1, $"{1 + 1}", mscorlib };
            bool valueEqual = first.Equals(second); // false.
            bool referenceEqual = object.ReferenceEquals(first, second); // false.
            bool sequentialEqual = first.SequenceEqual(second.Concat(Enumerable.Empty<object>())); // true.
        }

        internal static void SequentialEqualOfEmpty()
        {
            IEnumerable<Derived> emptyfirst = new ConcurrentQueue<Derived>();
            IEnumerable<Base> emptysecond = ImmutableHashSet.Create<Base>();
            bool sequentialEqual = emptyfirst.SequenceEqual(emptysecond); // true.
        }

        [SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "Dixin.Linq.LinqToObjects.EnumerableExtensions.SequenceEqual<System.String>(System.Collections.Generic.IEnumerable`1<System.String>,System.Collections.Generic.IEnumerable`1<System.String>,System.Collections.Generic.IEqualityComparer`1<System.String>)")]
        internal static void SequentialEqualWithComparer()
        {
            IEnumerable<string> first = new string[] { null, string.Empty, "ss", };
            IEnumerable<string> second = new string[] { null, string.Empty, "ß", };
            bool sequentialEqual1 = first.SequenceEqual(second, StringComparer.InvariantCulture); // true.
            bool sequentialEqual2 = first.SequenceEqual(second, StringComparer.Ordinal); // false.
        }

        #endregion
    }

    internal partial class Character : IComparable<Character>
    {
        public int CompareTo(Character other) =>
            string.Compare(this.Name, other.Name, StringComparison.Ordinal);
    }

    internal partial class Character
    {
        public override bool Equals(object obj)
        {
            Character other = obj as Character;
            return other != null && string.Equals(this.Name, other.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode() => this.Name?.GetHashCode() ?? 0;

        public static bool operator ==(Character character1, Character character2)
        {
            if (object.ReferenceEquals(character1, character2))
            {
                return true;
            }
            if ((object)character1 == null || (object)character2 == null)
            {
                return false;
            }
            return string.Equals(character1.Name, character2.Name, StringComparison.Ordinal);
        }

        public static bool operator !=(Character character1, Character character2) => !(character1 == character2);

        public static bool operator <(Character character1, Character character2) =>
            (object)character1 != null
            && (object)character2 != null
            && string.Compare(character1.Name, character2.Name, StringComparison.Ordinal) < 0;

        public static bool operator >(Character character1, Character character2) =>
            (object)character1 != null
            && (object)character2 != null
            && string.Compare(character1.Name, character2.Name, StringComparison.Ordinal) > 0;
    }
}

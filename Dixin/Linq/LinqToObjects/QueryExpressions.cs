namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Web.Profile;

    using Dixin.Linq.Fundamentals;
    using Dixin.Properties;
    using Dixin.Reflection;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    internal static class QueryExpressions
    {
        private static readonly Assembly mscorlib = typeof(object).Assembly;

        internal static void Where()
        {
            IEnumerable<Type> source = mscorlib.ExportedTypes;
            IEnumerable<Type> primitives = from type in source
                                           where type.IsPrimitive
                                           select type;
            foreach (Type primitive in primitives)
            {
                Trace.WriteLine(primitive);
            }
        }

        internal static void Select()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5);
            IEnumerable<string> squareRoots = from @int in source
                                              select $"{Math.Sqrt(@int):0.00}";
            foreach (string squareRoot in squareRoots)
            {
                Trace.WriteLine(squareRoot);
            }
        }

        internal static void Let()
        {
            IEnumerable<int> source = Enumerable.Range(-2, 5);
            IEnumerable<string> absoluteValues = from @int in source
                                                 let abs = Math.Abs(@int)
                                                 where abs > 0
                                                 select $"Math.Abs({@int}) == {abs}";
            foreach (string absoluteValue in absoluteValues)
            {
                Trace.WriteLine(absoluteValue);
            }
        }

        internal static void SelectMany()
        {
            IEnumerable<MemberInfo> mappedAndFiltered =
                from type in mscorlib.GetExportedTypes()
                from member in type.GetPublicDeclaredMembers()
                where member.IsObsolete()
                select member;
            foreach (MemberInfo obsoleteMember in mappedAndFiltered)
            {
                Trace.WriteLine($"{obsoleteMember.DeclaringType} - {obsoleteMember}");
            }
        }
        internal static void SelectManyWithResultSelector3()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<string> obsoleteMembers =
                from type in mscorlib.GetExportedTypes()
                from member in type.GetPublicDeclaredMembers()
                where member.IsObsolete()
                select $"{type} - {member}";
            foreach (string obsoleteMember in obsoleteMembers)
            {
                Trace.WriteLine(obsoleteMember);
            }
        }

        internal static void SelectManyWithResultSelector()
        {
            IEnumerable<Type> source = mscorlib.GetExportedTypes();
            IEnumerable<string> obsoleteMembers =
                from type in source
                from member in (from member in type.GetPublicDeclaredMembers()
                                select member)
                where member.IsObsolete()
                select $"{type} - {member}";
            foreach (string obsoleteMember in obsoleteMembers)
            {
                Trace.WriteLine(obsoleteMember);
            }
        }

        internal static IEnumerable<string> Words() => QueryMethods.Words();

        internal static void OrderBy()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = from word in source
                                          orderby word ascending // ascending can be omitted.
                                          select word;
            foreach (string word in ordered)
            {
                Trace.WriteLine(word);
            } // four one three two Zero
        }

        internal static void OrderByDescending()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = from word in source
                                          orderby word descending
                                          select word;
            foreach (string word in ordered)
            {
                Trace.WriteLine(word);
            } // four one three two Zero
        }

        internal static IEnumerable<Person> Persons() => QueryMethods.Persons();

        internal static void ThenBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth, person.Name
                                          select person;
            foreach (Person person in ordered)
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
            }
        }

        internal static void OrderByOrderBy1()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth
                                          orderby person.Name
                                          select person;
            foreach (Person person in ordered)
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
            }
        }

        internal static void OrderByOrderBy2()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered1 = from person in source
                                           orderby person.PlaceOfBirth
                                           select person;
            IEnumerable<Person> ordered2 = from person in ordered1
                                           orderby person.Name
                                           select person;
            foreach (Person person in ordered2)
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
            }
        }

        internal static void OrderByOrderBy3()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in (from person in source
                                                          orderby person.PlaceOfBirth
                                                          select person)
                                          orderby person.Name
                                          select person;
            foreach (Person person in ordered)
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
            }
        }

        internal static void OrderByOrderBy4()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth
                                          select person into person
                                          orderby person.Name
                                          select person;
            foreach (Person person in ordered)
            {
                Trace.WriteLine($"{person.PlaceOfBirth}: {person.Name}");
            }
        }

        internal static void GroupBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = from person in source
                                                            group person by person.PlaceOfBirth;
            foreach (IGrouping<string, Person> group in groups)
            {
                Trace.Write($"{group.Key}: ");
                foreach (Person person in group)
                {
                    Trace.Write($"{person.Name}, ");
                }
                Trace.WriteLine(null);
            }
        }

        internal static void GroupBySelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = from person in source
                                                            group person by person.PlaceOfBirth;
            IEnumerable<string> mapped = from @group in groups
                                         select $"{@group.Key}: {@group.Count()}";
            foreach (string group in mapped)
            {
                Trace.WriteLine(group);
            }
        }

        internal static void GroupBySelect2()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from @group in (from person in source
                                                         group person by person.PlaceOfBirth)
                                         select $"{@group.Key}: {@group.Count()}";
            foreach (string group in groups)
            {
                Trace.WriteLine(group);
            }
        }

        internal static void GroupBySelect3()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from person in source
                                         group person by person.PlaceOfBirth into @group
                                         select $"{@group.Key}: {@group.Count()}";
            foreach (string group in groups)
            {
                Trace.WriteLine(group);
            }
        }

        internal static void GroupByWithElementSelector()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, string>> groups = from person in source
                                                            group person.Name by person.PlaceOfBirth;
            foreach (IGrouping<string, string> group in groups)
            {
                Trace.Write($"{group.Key}: ");
                foreach (string name in group)
                {
                    Trace.Write($"{name}, ");
                }
                Trace.WriteLine(null);
            }
        }

        internal static void GroupByWithElementSelectorAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from person in source
                                         group person.Name by person.PlaceOfBirth into @group
                                         select $"{@group.Key}: {string.Join(",", @group)}";
            foreach (string group in groups)
            {
                Trace.WriteLine(group);
            }
        }

        private static readonly int[] rows = { 1, 2, 3 };

        private static readonly string[] columns = { "A", "B", "C", "D" };

        internal static void CrossJoin()
        {
            IEnumerable<string> cells = from row in rows
                                        from column in columns
                                        select $"{column}{row}";
            int cellIndex = 0;
            int columnCount = columns.Length;
            foreach (string cell in cells)
            {
                Trace.Write($"{cell} ");
                if (cellIndex++ > 0 && cellIndex % columnCount == 0)
                {
                    Trace.WriteLine(null);
                }
            }
        }

        internal static void CrossJoinWithJoin()
        {
            IEnumerable<string> cells = from row in rows
                                        join column in columns on true equals true
                                        select $"{column}{row}";
            int cellIndex = 0;
            int columnCount = columns.Length;
            foreach (string cell in cells)
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
                from outerValue in outer
                join innerValue in inner on true equals true
                select resultSelector(outerValue, innerValue);

        internal static IEnumerable<Character> Characters() => QueryMethods.Characters();

        internal static void InnerJoin()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin =
                from person in outer
                join character in inner on person.Name equals character.Starring
                select $"{person.Name} ({person.PlaceOfBirth}): {character.Name}";
            foreach (string value in innerJoin)
            {
                Trace.WriteLine(value);
            }
        }

        internal static void InnerJoinWithSelectMany()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin =
                from person in outer
                from character in inner
                where EqualityComparer<string>.Default.Equals(person.Name, character.Starring)
                select $"{person.Name} ({person.PlaceOfBirth}): {character.Name}";
            foreach (string value in innerJoin)
            {
                Trace.WriteLine(value);
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
            return from outerValue in outer
                   from innerValue in inner
                   where comparer.Equals(outerKeySelector(outerValue), innerKeySelector(innerValue))
                   select resultSelector(outerValue, innerValue);
        }

        internal static void InnerJoinWithMultipleKeys()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            IEnumerable<string> innerJoin =
                from person in outer
                join character in inner
                    on new { Starring = person.Name, PlaceOfBirth = person.PlaceOfBirth }
                    equals new { Starring = character.Starring, PlaceOfBirth = character.PlaceOfBirth }
                select $"{person.Name} ({person.PlaceOfBirth}): {character.Name} ({character.PlaceOfBirth})";
            foreach (string value in innerJoin)
            {
                Trace.WriteLine(value);
            }
        }

        internal static void LeftOuterJoin()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin =
                from person in outer
                join character in inner on person.Name equals character.Starring into charactersGroup
                select new { Person = person, Characters = charactersGroup };
            foreach (var value in leftOuterJoin)
            {
                Trace.Write($"{value.Person.Name} ({value.Person.PlaceOfBirth}): ");
                foreach (Character character in value.Characters)
                {
                    Trace.Write($"{character.Name} ({character.PlaceOfBirth}); ");
                }
                Trace.WriteLine(null);
            }
        }

        internal static void LeftOuterJoinWithDefaultIfEmpty()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin =
                from person in outer
                join character in inner on person.Name equals character.Starring into charactersGroup
                from character in charactersGroup.DefaultIfEmpty()
                select new { Person = person, Character = character };
            foreach (var value in leftOuterJoin)
            {
                Trace.WriteLine($"{value.Person.Name}: {value.Character?.Name}");
            }
        }

        internal static void LeftOuterJoinWithSelectMany()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin =
                from person in outer
                from character in inner
                group character by person into charactersGroup
                select new
                {
                    Person = charactersGroup.Key,
                    Characters = from characterInGroup in charactersGroup
                                 where EqualityComparer<string>.Default.Equals(charactersGroup.Key.Name, characterInGroup.Starring)
                                 select characterInGroup
                };
            foreach (var value in leftOuterJoin)
            {
                Trace.Write($"{value.Person.Name} ({value.Person.PlaceOfBirth}): ");
                foreach (Character character in value.Characters)
                {
                    Trace.Write($"{character.Name} ({character.PlaceOfBirth}); ");
                }
                Trace.WriteLine(null);
            }
        }

        internal static void LeftOuterJoinWithSelect()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin =
                from person in outer
                select new
                {
                    Person = person,
                    Characters = from character in inner
                                 where EqualityComparer<string>.Default.Equals(person.Name, character.Starring)
                                 select character
                };
            foreach (var value in leftOuterJoin) // Execute query.
            {
                Trace.Write($"{value.Person.Name} ({value.Person.PlaceOfBirth}): ");
                foreach (Character character in value.Characters)
                {
                    Trace.Write($"{character.Name} ({character.PlaceOfBirth}), ");
                }
                Trace.WriteLine(null);
            }
            // Robert Downey Jr. (US): Tony Stark (US),
            // Tom Hiddleston (UK):
            // Chris Hemsworth (AU): Thor (Asgard),
            // Chris Evans (US): Steve Rogers (US),
            // Paul Bettany (UK): Vision (KR), JARVIS (US),
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
            return from outerValue in outer
                   select resultSelector(
                       outerValue,
                       from innerValue in inner
                       where comparer.Equals(outerKeySelector(outerValue), innerKeySelector(innerValue))
                       select innerValue);
        }

        internal static void CastNonGenericIEnumerable()
        {
            using (TfsTeamProjectCollection projectCollection = new TfsTeamProjectCollection(
                new Uri("https://dixin.visualstudio.com/DefaultCollection"),
                new TfsClientCredentials(new BasicAuthCredential(new NetworkCredential(
                    Settings.Default.TfsUserName, Settings.Default.TfsPassword)))
                { AllowInteractive = false }))
            {
                const string wiql = "SELECT * FROM WorkItems WHERE [Work Item Type] = 'Bug' AND State != 'Closed'"; // WIQL does not support GROUP BY.
                WorkItemStore workItemStore = (WorkItemStore)projectCollection.GetService(typeof(WorkItemStore));

                WorkItemCollection workItems = workItemStore.Query(wiql); // WorkItemCollection: IEnumerable.
                IEnumerable<IGrouping<string, WorkItem>> workItemGroups =
                        from WorkItem workItem in workItems
                        group workItem by workItem.CreatedBy; // Group work items in local memory.
                                                              // ...
            }
        }

        internal static void CastNonGenericIEnumerable2()
        {
            IEnumerable<SettingsProperty> genericProperties =
                from SettingsProperty property in ProfileBase.Properties // SettingsPropertyCollection: IEnumerable.
                select property;
        }

        internal static void CastGenericIEnumerable()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Derived() };
            IEnumerable<Derived> casted = from Derived derived in source
                                          select derived;
            foreach (Derived derived in casted)
            {
                Trace.WriteLine(derived.GetType().Name);
            }
            // Derived Derived
        }

        internal static void CastGenericIEnumerableWithException()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Base() };
            IEnumerable<Derived> casted = from Derived derived in source
                                          select derived;
            foreach (Derived derived in casted)
            {
                Trace.WriteLine(derived.GetType().Name);
            }
            // Derived InvalidCastException
        }

        internal static void CastWithJoin()
        {
            IEnumerable outer = new int[] { 1, 2, 3 };
            IEnumerable inner = new string[] { "a", "bb", "ccc" };
            IEnumerable<string> innerJoin = from int @int in outer
                                            join string @string in inner on @int equals @string.Length
                                            select $"{@int}: {@string}";
            foreach (string value in innerJoin)
            {
                Trace.WriteLine(value);
            }
        }

        internal static void CastGenericIEnumerableWithRestriction()
        {
            object[] source = { 1, 2, 'a', 'b', "aa", "bb", new object(), 3 };
            IEnumerable<int> casted = from int value in (from value in source
                                                         where value is int
                                                         select value)
                                      select value;
            foreach (int integer in casted)
            {
                Trace.WriteLine(integer);
            }
            // 1 2 3
        }

        internal static void CastGenericIEnumerableWithRestriction2()
        {
            object[] source = { 1, 2, 'a', 'b', "aa", "bb", new object(), 3 };
            IEnumerable<int> casted = from value in source
                                      where value is int
                                      select (int)value;
            foreach (int integer in casted)
            {
                Trace.WriteLine(integer);
            }
            // 1 2 3
        }
    }
}
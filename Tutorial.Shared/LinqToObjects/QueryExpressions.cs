namespace Tutorial.LinqToObjects
{
#if NETFX
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Web.Profile;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    using Tutorial.Functional;
#else
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Tutorial.Functional;
#endif

    internal static class QueryExpressions
    {
        private static readonly Assembly CoreLibrary = typeof(object).GetTypeInfo().Assembly;

        internal static void Where()
        {
            IEnumerable<Type> source = CoreLibrary.GetExportedTypes();
            IEnumerable<Type> primitives = from type in source
                                           where type.GetTypeInfo().IsPrimitive
                                           select type;
        }

        internal static void Select()
        {
            IEnumerable<int> source = Enumerable.Range(0, 5);
            IEnumerable<string> squareRoots = from int32 in source
                                              select $"{Math.Sqrt(int32):0.00}";
        }

        internal static void Let()
        {
            IEnumerable<int> source = Enumerable.Range(-2, 5);
            IEnumerable<string> absoluteValues = from int32 in source
                                                 let abs = Math.Abs(int32)
                                                 where abs > 0
                                                 select $"Math.Abs({int32}) == {abs}";
        }

        internal static void SelectMany()
        {
            IEnumerable<MemberInfo> mappedAndFiltered =
                from type in CoreLibrary.GetExportedTypes()
                from member in type.GetDeclaredMembers()
                where member.IsObsolete()
                select member;
        }

        internal static void SelectManyWithResultSelector()
        {
            IEnumerable<Type> source = CoreLibrary.GetExportedTypes();
            IEnumerable<string> obsoleteMembers =
                from type in CoreLibrary.GetExportedTypes()
                from member in type.GetDeclaredMembers()
                where member.IsObsolete()
                select $"{type} - {member}";
        }

        internal static void GroupBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = from person in source
                                                            group person by person.PlaceOfBirth;
        }

        internal static void GroupByAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, Person>> groups = from person in source
                                                            group person by person.PlaceOfBirth;
            IEnumerable<string> mapped = from @group in groups
                                         select $"{@group.Key}: {@group.Count()}";
        }

        internal static void FluentGroupByAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from @group in (from person in source
                                                         group person by person.PlaceOfBirth)
                                         select $"{@group.Key}: {@group.Count()}";
        }

        internal static void FluentGroupBySelectWithInto()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from person in source
                                         group person by person.PlaceOfBirth into @group
                                         select $"{@group.Key}: {@group.Count()}";
        }

        internal static void GroupByWithElementSelector()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<IGrouping<string, string>> groups = from person in source
                                                            group person.Name by person.PlaceOfBirth;
        }

        internal static void GroupByWithElementSelectorAndSelect()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<string> groups = from person in source
                                         group person.Name by person.PlaceOfBirth into @group
                                         select $"{@group.Key}: {string.Join(",", @group)}";
        }

        internal static IEnumerable<string> Words() => QueryMethods.Words();

        internal static void OrderBy()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = from word in source
                                          orderby word ascending // ascending can be omitted.
                                          select word;
        }

        internal static void OrderByDescending()
        {
            IEnumerable<string> source = Words();
            IEnumerable<string> ordered = from word in source
                                          orderby word descending
                                          select word;
        }

        internal static IEnumerable<Person> Persons() => QueryMethods.Persons();

        internal static void ThenBy()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth, person.Name
                                          select person;
        }

        internal static void OrderByOrderBy1()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth
                                          orderby person.Name
                                          select person;
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
        }

        internal static void OrderByOrderBy3()
        {
            IEnumerable<Person> source = Persons();
            IEnumerable<Person> ordered = from person in source
                                          orderby person.PlaceOfBirth
                                          select person into person
                                          orderby person.Name
                                          select person;
        }

        private static readonly int[] rows = { 1, 2, 3 };

        private static readonly string[] columns = { "A", "B", "C", "D" };

        internal static void CrossJoin()
        {
            IEnumerable<string> cells = from row in rows
                                        from column in columns
                                        select $"{column}{row}";
        }

        internal static void CrossJoinWithJoin()
        {
            IEnumerable<string> cells = from row in rows
                                        join column in columns on true equals true
                                        select $"{column}{row}";
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
        }

        internal static void LeftOuterJoin()
        {
            IEnumerable<Person> outer = Persons();
            IEnumerable<Character> inner = Characters();
            var leftOuterJoin =
                from person in outer
                join character in inner on person.Name equals character.Starring into charactersGroup
                select new { Person = person, Characters = charactersGroup };
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

#if NETFX
        internal static void CastNonGenericIEnumerable(TfsClientCredentials credentials)
        {
            using (TfsTeamProjectCollection projectCollection = new TfsTeamProjectCollection(
                new Uri("https://dixin.visualstudio.com/DefaultCollection"), credentials))
            {
                const string wiql = "SELECT * FROM WorkItems WHERE [Work Item Type] = 'Bug' AND State != 'Closed'"; // WIQL does not support GROUP BY.
                WorkItemStore workItemStore = (WorkItemStore)projectCollection.GetService(typeof(WorkItemStore));
                WorkItemCollection workItems = workItemStore.Query(wiql); // WorkItemCollection implements IEnumerable.

                IEnumerable<IGrouping<string, WorkItem>> workItemGroups =
                        from WorkItem workItem in workItems // Cast.
                        group workItem by workItem.CreatedBy; // Group work items in local memory.
                // ...
            }
        }
#endif

#if NETFX
        internal static void CastNonGenericIEnumerable2()
        {
            SettingsPropertyCollection properties = ProfileBase.Properties; // SettingsPropertyCollection implements IEnumerable.
            IEnumerable<SettingsProperty> genericProperties = from SettingsProperty property in properties // Cast.
                                                              select property;
        }
#endif

        internal static void CastGenericIEnumerable()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Derived() };
            IEnumerable<Derived> casted = from Derived derived in source
                                          select derived;
        }

        internal static void CastGenericIEnumerableWithException()
        {
            IEnumerable<Base> source = new Base[] { new Derived(), new Base() };
            IEnumerable<Derived> casted = from Derived derived in source
                                          select derived;
        }

        internal static void CastWithJoin()
        {
            IEnumerable outer = new int[] { 1, 2, 3 };
            IEnumerable inner = new string[] { "a", "bb", "ccc" };
            IEnumerable<string> innerJoin = from int int32 in outer
                                            join string @string in inner on int32 equals @string.Length
                                            select $"{int32}: {@string}";
        }

        internal static void CastGenericIEnumerableWithRestriction()
        {
            object[] source = { 1, 2, 'a', 'b', "aa", "bb", new object(), 3 };
            IEnumerable<int> casted = from int value in (from value in source
                                                         where value is int
                                                         select value)
                                      select value;
        }

        internal static void CastGenericIEnumerableWithRestriction2()
        {
            object[] source = { 1, 2, 'a', 'b', "aa", "bb", new object(), 3 };
            IEnumerable<int> casted = from value in source
                                      where value is int
                                      select (int)value;
        }
    }
}
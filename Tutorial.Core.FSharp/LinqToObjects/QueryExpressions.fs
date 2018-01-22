namespace Tutorial.LinqToObjects

    open System
    open System.Linq
    open System.Reflection

    open Microsoft.FSharp.Linq

    module FSharpQueryExpressions =

        let nullableInt32Source: unit -> Nullable<int>[] = fun () -> [| new Nullable<int>(-1); new Nullable<int>(1); new Nullable<int>(2); new Nullable<int>(3); new Nullable<int>(-4); new Nullable<int>() |];

        // #region Filtering

        let where: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                where (int > 0)
            }

        // #endregion

        // #region Mapping

        let select: seq<int> = 
            query {
                for int in QueryMethods.Int32Source() do
                select (int * int)
            }

        let selectMany: seq<MemberInfo> = 
            query {
                for type' in typeof<Object>.Assembly.GetExportedTypes() do
                for member' in type'.GetMembers() do
                where (QueryMethods.IsObsolete member')
                select member'                
            }

        // #endregion

        // #region Grouping

        let groupBy: seq<string * int> =
            query {
                for person in QueryMethods.Persons() do
                groupBy person.PlaceOfBirth into group
                select (group.Key, group.Count())
            }

        let groupValBy: seq<string * int> =
            query {
                for person in QueryMethods.Persons() do
                groupValBy person.Name person.PlaceOfBirth into group
                select (group.Key, group.Count())
            }

        // #endregion

        // #region Join

        let join: seq<string * string * string> =
            query {
                for person in QueryMethods.Persons() do 
                join character in QueryMethods.Characters() on (person.Name = character.Starring)
                select (person.Name, person.PlaceOfBirth, character.Name)
            }

        let groupJoin: seq<Person * seq<Character>> =
            query {
                for person in QueryMethods.Persons() do 
                groupJoin character in QueryMethods.Characters() on (person.Name = character.Starring) into charactersGroup
                select (person, charactersGroup)
            }

        let leftOuterJoin: seq<Person * Character> =
            query {
                for person in QueryMethods.Persons() do 
                leftOuterJoin character in QueryMethods.Characters() on (person.Name = character.Starring) into charactersGroup
                for character in charactersGroup.DefaultIfEmpty() do
                select (person, character)
            }

        let crossJoin: seq<int * int> = 
            query {
                for int1 in QueryMethods.Int32Source() do
                for int2 in QueryMethods.Int32Source() do
                select (int1, int2)
            }

        // #endregion

        // #region Set

        let distinct: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                distinct        
            }

        // #endregion

        // #region Convolution
#if ERROR
        let zip: seq<int * int> = 
            query {
                for first in QueryMethods.First() do
                zip second in QueryMethods.Second()
                select (first * second)
            }
#endif
        // #endregion

        // #region Partitioning

        let skip: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                skip 1
            }

        let skipWhile: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                skipWhile (int < 0)
            }

        let take: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                take 1
           }

        let takeWhile: seq<int> =
            query {
                for int in QueryMethods.Int32Source() do
                takeWhile (int < 0)
            }

        // #endregion

        // #region Ordering

        let sortBy: seq<string> =
            query {
                for word in QueryMethods.Words() do
                sortBy word
            }

        let sortByDescending: seq<string> =
            query {
                for word in QueryMethods.Words() do
                sortByDescending word
            }
    
        let thenBy: seq<Person> =
            query {
                for person in QueryMethods.Persons() do
                sortBy person.PlaceOfBirth
                thenBy person.Name
            }

        let thenByDescending: seq<Person> =
            query {
                for person in QueryMethods.Persons() do
                sortBy person.PlaceOfBirth
                thenByDescending person.Name
            }
            
        let sortByNullable: seq<Nullable<int>> =
            query {
                for int' in nullableInt32Source() do
                sortByNullable int'
            }

        let sortByNullableDescending: seq<Nullable<int>> =
            query {
                for int' in nullableInt32Source() do
                sortByNullableDescending int'
            }

        let thenByNullable: seq<Nullable<int>> =
            query {
                for int' in nullableInt32Source() do
                sortByNullable int'
                thenByNullable int'
            }

        let thenByNullableDescending: seq<Nullable<int>> =
            query {
                for int' in nullableInt32Source() do
                sortByNullable int'
                thenByNullableDescending int'
            }

        // #endregion

        // #region Element
            
        let head: int =
            query {
                for int in QueryMethods.Int32Source() do
                head
            }

        let headOrDefault: int =
            query {
                for int in QueryMethods.Int32Source() do
                headOrDefault
            }

        let last: int = 
            query {
                for int in QueryMethods.Int32Source() do
                last
            }

        let lastOrDefault: int =
            query {
                for int in QueryMethods.EmptyInt32Source() do
                lastOrDefault
            }
        
        let exactlyOne: int =
            query {
                for int in QueryMethods.SingleInt32Source() do
                exactlyOne
            }

        let exactlyOneOrDefault: int =
            query {
                for int in QueryMethods.Int32Source() do
                where (int <= 0)
                exactlyOneOrDefault
            }

        let find: int =
            query {
                for int in QueryMethods.Int32Source() do
                find (int > 0)
            }

        let nth: int =
            query {
                for int in QueryMethods.Int32Source() do
                nth 3
            }

        // #endregion

        // #region Aggregation

        let count: int = 
            query {
                for int in QueryMethods.Int32Source() do
                count
            }

        let minBy: string =
            query {
                for character in QueryMethods.Characters() do
                minBy character.Name
            }

        let maxBy: string =
            query {
                for character in QueryMethods.Characters() do
                maxBy character.Name
            }
    
        let averageBy: float =
            query {
                for int in QueryMethods.Int32Source() do
                averageBy (float int)
            }
            
        let sumBy: int =
            query {
               for person in QueryMethods.Persons() do
               sumBy person.Name.Length
           }

        let minByNullable: Nullable<int> =
            query {
                for int' in nullableInt32Source() do
                minByNullable int'
            }

        let maxByNullable: Nullable<int> =
            query {
                for int' in nullableInt32Source() do
                maxByNullable int'
            }

        let averageByNullable: Nullable<float> =
            query {
                for int' in nullableInt32Source() do
                averageByNullable (Nullable.float int')
            }

        let sumByNullable: Nullable<int> =
            query {
                for int' in nullableInt32Source() do
                sumByNullable int'
            }

        // #endregion

        // #region Quantifiers

        let exists: bool =
            query {
                for int in QueryMethods.Int32Source() do
                exists (int < 0)
            }

        let all: bool =
            query {
                for int in QueryMethods.Int32Source() do
                all (int > 0)
            }

        let contains: bool = 
            query {
                for int in QueryMethods.Int32Source() do
                contains 0
            }

        // #endregion

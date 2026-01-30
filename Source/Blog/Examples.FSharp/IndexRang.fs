module Examples.FSharp.IndexRange

open System
open System.Collections.Generic
open System.Linq
open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Linq.RuntimeHelpers

// [<Extension>]
// type IEnumerableExtensions =
//    [<Extension>]
//    static member GetSlice(sequence: IEnumerable<'T>, startIdx: int option, endIdx: int option) =
//        let s = defaultArg startIdx 0
//        let e = if endIdx.IsSome then Index(endIdx.Value) else Index.FromEnd 1
//        sequence.Take(Range(s, e))

//    [<Extension>]
//    static member GetReverseIndex(sequence: IEnumerable<'T>, dimension: int, offset: int) =
//        sequence.Count() - offset

let sequnce = seq {1..100}
let list = [1..100]
let array = [|1..100|]

let arrayWithRange = array[10..^20]
let arrayWithIndex = array[^5]

let listWithRange = list[10..^20]
let listWithIndex = list[^5]


// Current:
let range = Range(10, Index(20, fromEnd = true))
let index = Index(5, fromEnd = true)
let sequenceWithRangeIndex = sequnce.Take(Range(10, Index(20, fromEnd = true))).ElementAt(Index(5, fromEnd = true))
// Proposal:
// let range = 10..^20
// let index = ^5
// let sequenceWithRangeIndex = sequnce.Take(10..^20).ElementAt(^5)

// Current:
// Type extension for IEnumerable<T> works in .NET 8, but no longer works in .NET 9, see https://github.com/dotnet/fsharp/issues/18001
// type IEnumerable<'T> with
//    member sequence.Item
//        with get(index: Index) = sequence.ElementAt index

//    member sequence.Item
//        with get(range: Range) = sequence.Take range

// let sequenceWithRange2 = sequnce[Range(10, Index(20, fromEnd = true))]
// let sequenceWithIndex2 = sequnce[Index(5, fromEnd = true)]
// Proposal:
// let sequenceWithRange2 = sequnce[10..^20]
// let sequenceWithIndex2 = sequnce[^5]

module Seq =
    [<CompiledName ("Range")>]
    let range (range: Range) (source: 'T seq) =
        Enumerable.Take (source, range)

    [<CompiledName ("Index")>]
    let index (index: Index) (source: 'T seq) =
        Enumerable.ElementAt (source, index)

// Current:
sequnce
    |> Seq.map (fun x -> x * 2)
    |> Seq.take 10
    |> Seq.iter Console.WriteLine
sequnce
    |> Seq.map (fun x -> x * 2)
    |> Seq.range (Range(10, Index(20, fromEnd = true)))
    |> Seq.iter Console.WriteLine
// Proposal:
//sequnce
//    |> Seq.map (fun x -> x * 2)
//    |> Seq.range 10..^20
//    |> Seq.iter Console.WriteLine

// Current:
sequnce
    |> Seq.map (fun x -> x * 2)
    |> Seq.item 5
    |> Console.WriteLine
// Proposal:
// sequnce
//    |> Seq.map (fun x -> x * 2)
//    |> Seq.item ^5 // or Seq.index ^5
//    |> Console.WriteLine

// Current:
list
    |> List.map (fun x -> x * 2)
    |> List.take 10
    |> List.iter Console.WriteLine
// Proposal:
// list
//    |> List.map (fun x -> x * 2)
//    |> List.take 10..^20 // or List.range 10..^20
//    |> List.iter Console.WriteLine

// Current:
list
    |> List.map (fun x -> x * 2)
    |> List.item 5
    |> Console.WriteLine
// Proposal:
// list
//    |> List.map (fun x -> x * 2)
//    |> List.item ^5 // or List.index ^5
//    |> Console.WriteLine

// Current:
array
    |> Array.map (fun x -> x * 2)
    |> Array.take 10
    |> Array.iter Console.WriteLine
// Proposal:
//array
//    |> Array.map (fun x -> x * 2)
//    |> Array.take 10..^20 // or Array.range 10..^20
//    |> Array.iter Console.WriteLine

// Current:
array
    |> Array.map (fun x -> x * 2)
    |> Array.item 10
    |> Console.WriteLine
// Proposal:
// array
//    |> Array.map (fun x -> x * 2)
//    |> Array.item ^5 // or Array.index ^5
//    |> Console.WriteLine

// Current:
let number = query {
    for x in sequnce do
    select x
    take 10
    nth 5
}
// Proposal:
// let number = query {
//    for x in sequnce do
//    select x
//    take 10..^20 // or range 10..^20
//    nth ^5 // or index ^5
//}

type DemoQueryBuilder() = 
    /// 'For' and 'Yield' enables the 'for x in xs do ..' syntax
    member _.For (source: QuerySource<'T, 'Q>, body: 'T -> QuerySource<'Result, 'Q2>) : QuerySource<'Result, 'Q> =
        QuerySource (Seq.collect (fun x -> (body x).Source) source.Source)

    member _.Yield value =
        QuerySource (Seq.singleton value)

    /// Instructs the compiler to capture quotation of the query
    member _.Quote  (quotation: Quotations.Expr<'T>) =
        quotation

    member _.Source (source: IEnumerable<'T>) : QuerySource<'T, System.Collections.IEnumerable> =
        QuerySource source

    /// Represents filtering of the source using specified condition
    [<CustomOperation("where",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member _.Where (source: QuerySource<'T, 'Q>, [<ProjectionParameter>] predicate) : QuerySource<'T, 'Q> =
        QuerySource (Enumerable.Where (source.Source, Func<_, _>(predicate)) )

    [<CustomOperation("take",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member _.Take (source: QuerySource<'T, 'Q>, count: int) : QuerySource<'T, 'Q> =
        QuerySource (Enumerable.Take (source.Source, count))

    [<CustomOperation("range",MaintainsVariableSpace=true,AllowIntoPattern=true)>] 
    member _.Range (source: QuerySource<'T, 'Q>, range: Range) : QuerySource<'T, 'Q> =
        QuerySource (Enumerable.Take (source.Source, range))

    [<CustomOperation("nth")>] 
    member _.Nth (source: QuerySource<'T, 'Q>, index: int) =
        Enumerable.ElementAt (source.Source, index)

    [<CustomOperation("index")>] 
    member _.Index (source: QuerySource<'T, 'Q>, index: Index) =
        Enumerable.ElementAt (source.Source, index)

let demoQuery = DemoQueryBuilder()

// Current:
let queryResult1 = demoQuery { 
    for number in sequnce do
    where (number > 10)
    take 10
    nth 5
}
let queryResult2 = demoQuery { 
    for number in sequnce do
    where (number > 10)
    range (Range(10, Index(20, fromEnd = true)))
    index (Index(5, fromEnd = true))
}
// Proposal:
//let queryResult = demoQuery { 
//    for number in sequnce do
//    where (number > 10)
//    range 10..^20
//    index ^5
//}

// https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/slices
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges

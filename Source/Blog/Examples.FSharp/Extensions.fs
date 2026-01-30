module Examples.FSharp.Extensions

open System.Collections.Generic
open System.Runtime.CompilerServices

//type IEnumerable<'T> with
//    /// Repeat each element of the sequence n times
//    member xs.RepeatElements(n: int) =
//        seq {
//            for x in xs do
//                for _ in 1 .. n -> x
//        }

[<Extension>]
type EnumerableExtensions =
    [<Extension>]
    static member RepeatElements(xs: IEnumerable<'T>, n: int) : seq<'T> =
        seq {
            for x in xs do
                for _ in 1 .. n -> x
        }
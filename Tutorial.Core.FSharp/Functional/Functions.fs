namespace Tutorial.Functional

    open System
    open System.Diagnostics
    open System.Linq

    module Functions =
        let CallLambdaExpression : unit -> unit = fun () ->
            (fun value -> value > 0) 1
            |> ignore

        let Curry : unit -> unit = fun () ->
            let curriedAdd2: int -> (int -> int) = fun a -> (fun b -> a + b)
            let add1: int -> int = curriedAdd2 1
            let curriedAdd2Result: int = add1 2
            ()

        let DefaultCurry : unit -> unit = fun () ->
            let add2: int -> int -> int = fun a b -> a + b
            let add2Result: int = add2 1 2
            ()

        let Uncurry : unit -> unit = fun () ->
            let add2Tuple: int * int -> int = fun (a, b) -> a + b
            let add2TupleResult = add2Tuple (1, 2) // add2Tuple(Tuple.Create(1, 2))
            ()

        let CompositionOperator : unit -> unit = fun () ->
            // let (>>) : ('T -> 'TResult1) -> ('TResult1 -> 'TResult2) -> ('T -> 'TResult2) = fun function1 function2 value -> function2 (function1 value)
            let inline (>>) function1 function2 value = function2 (function1 value)

            // let (<<) : ('TResult1 -> 'TResult2) -> ('T -> 'TResult1) -> ('T -> 'TResult2) = fun function2 function1 value -> function2 (function1 value)
            let inline (<<) function2 function1 value = function2 (function1 value)

            let composition1 = Int32.Parse >> Math.Abs >> Convert.ToDouble >> Math.Sqrt

            let composition2 = Math.Sqrt << (Convert.ToDouble: int -> double) << (Math.Abs: int -> int) << Int32.Parse
            ()

        let Composite : unit -> unit = fun () ->
            let filterSortMap : seq<int> -> seq<float> = // IEnumerable<int> -> IEnumerable<float>
                (fun source -> Enumerable.Where (source, ((>) 0))) // (fun int32 -> int32 > 0)
                >> (fun filtered -> Enumerable.OrderBy (filtered, (fun int32 -> int32)))
                >> (fun ordered -> Enumerable.Select (ordered, (fun int32 -> Math.Sqrt (float int32))))
            let query : seq<float> = filterSortMap [| 4; 3; 2; 1; 0; -1 |] // IEnumerable<float>
            for result : float in query do
                Trace.WriteLine result
            ()

        let CompositeAndPartialApply : unit -> unit = fun () ->
            let filterSortMap : seq<int> -> seq<float> = // IEnumerable<int> -> IEnumerable<float>
                Seq.filter ((>) 0) // (fun int32 -> int32 > 0)
                >> Seq.sortBy id // (fun int32 -> int32)
                >> Seq.map (fun int32 -> Math.Sqrt (float int32))
            let query : seq<float> = filterSortMap [| 4; 3; 2; 1; 0; -1 |] // IEnumerable<float>
            for result : float in query do
                Trace.WriteLine result
            ()

        let ForwardOperator : unit -> unit = fun () ->
            // let (>>) : 'T -> ('T -> 'TResult) -> 'TResult = fun value function' -> function' value
            let inline (|>) value function' = function' value
            ()

        let ForwardAndPartialApply : unit -> unit = fun () ->
            let source : int[] = [| 4; 3; 2; 1; 0; -1 |]
            let query : seq<float> = // IEnumerable<float>
                source
                |> Seq.filter ((>) 0) // (fun int32 -> int32 > 0)
                |> Seq.sortBy id // (fun int32 -> int32)
                |> Seq.map (fun int32 -> Math.Sqrt (float int32))
            for result : float in query do
                Trace.WriteLine result
            ()
#if DEMO
        let ValueAndVariable : unit -> unit = fun () ->
            let value = new Uri("https://weblogs.asp.net/dixin") // Immutable value.
            value <- null // Mutation cannot be compiled.

            let mutable variable = new Uri("https://weblogs.asp.net/dixin") // Mutable variable.
            variable <- null // Mutation can be compiled.
            ()
#endif
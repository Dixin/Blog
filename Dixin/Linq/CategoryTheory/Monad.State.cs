namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.FSharp.Core;

    // State: TState -> Tuple<T, TState>
    public delegate Tuple<T, TState> State<TState, T>(TState state);

    public static partial class StateExtensions
    {
        // SelectMany: (State<TState, TSource>, TSource -> State<TState, TSelector>, (TSource, TSelector) -> TResult) -> State<TState, TResult>
        public static State<TState, TResult> SelectMany<TState, TSource, TSelector, TResult>(
            this State<TState, TSource> source,
            Func<TSource, State<TState, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                oldState =>
                {
                    Tuple<TSource, TState> value = source(oldState);
                    Tuple<TSelector, TState> result = selector(value.Item1)(value.Item2);
                    TState newState = result.Item2;
                    return resultSelector(value.Item1, result.Item1).Tuple(newState); // Output new state.
                };

        // Wrap: TSource -> State<TState, TSource>
        public static State<TState, TSource> State<TState, TSource>(this TSource value) =>
            oldState => value.Tuple(oldState); // Output old state.

        // Select: (State<TState, TSource>, TSource -> TResult) -> State<TState, TResult>
        public static State<TState, TResult> Select<TState, TSource, TResult>(
            this State<TState, TSource> source,
            Func<TSource, TResult> selector) =>
                oldState =>
                {
                    Tuple<TSource, TState> value = source(oldState);
                    TState newState = value.Item2;
                    return selector(value.Item1).Tuple(newState); // Output new state.
                };
                // Equivalent to:            
                // source.SelectMany(value => selector(value).State<TState, TResult>(), (value, result) => result);
    }

    public static partial class StateExtensions
    {
        // GetState: () -> State<TState, TState>
        public static State<TState, TState> GetState<TState>() =>
            oldState => oldState.Tuple(oldState); // Output old state.

        // SetState: TState -> State<TState, Unit>
        public static State<TState, Unit> SetState<TState>(TState newState) =>
            oldState => default(Unit).Tuple(newState); // Output new state.
    }

    public static partial class StateExtensions
    {
        internal static void Workflow()
        {
            string initialState = nameof(initialState);
            string newState = nameof(newState);
            string resetState = nameof(resetState);
            State<string, int> source1 = oldState => 1.Tuple(oldState);
            State<string, bool> source2 = oldState => true.Tuple(newState);
            State<string, char> source3 = '@'.State<string, char>(); // oldState => 2.Tuple(oldState).

            State<string, string[]> query =
                from value1 in source1 // source1: State<string, int> = initialState => (1, initialState).
                from state1 in GetState<string>() // GetState<int>(): State<string, string> = initialState => (initialState, initialState).
                from value2 in source2 // source2: State<string, bool>3 = initialState => (true, newState).
                from state2 in GetState<string>() // GetState<int>(): State<string, string> = newState => (newState, newState).
                from unit in SetState(resetState) // SetState(resetState): State<string, Unit> = newState => (default(Unit), resetState).
                from state3 in GetState<string>() // GetState(): State<string, string> = resetState => (resetState, resetState).
                from value3 in source3 // source3: State<string, char> = resetState => (@, resetState).
                select new string[] { state1, state2, state3 }; // Define query.
            Tuple<string[], string> result = query(initialState); // Execute query with initial state.
            result.Item1.WriteLines(); // initialState newState resetState
            result.Item2.WriteLine(); // Final state: resetState
        }
    }

    public static partial class StateExtensions
    {
        // FactorialState: unit -> (unit -> Tuple(unit, unit))
        // FactorialState: unit -> State<unit, unit>
        private static State<uint, uint> FactorialState(uint current) =>
            from state in GetState<uint>() // State<uint, uint>.
            let product = state
            let next = current - 1U
            from result in current > 0U
                ? (from unit in SetState(product * current) // State<unit, Unit>.
                   from value in FactorialState(next) // State<uint, uint>.
                   select next)
                : next.State<uint, uint>() // State<uint, uint>.
            select result;

        public static uint Factorial(uint uInt32)
        {
            State<uint, uint> query = FactorialState(uInt32); // Define query.
            return query(1).Item2; // Execute query, with initial state: 1.
        }

        // AggregateState: (TAccumulate -> TSource -> TAccumulate) -> (Tuple<TAccumulate, IEnumerable<TSource>> -> Tuple(TAccumulate -> TSource -> TAccumulate, Tuple<TAccumulate, IEnumerable<TSource>>))
        // AggregateState: TAccumulate -> TSource -> TAccumulate -> State<Tuple<TAccumulate, IEnumerable<TSource>>, TAccumulate -> TSource -> TAccumulate>
        private static State<Tuple<TAccumulate, IEnumerable<TSource>>, Func<TAccumulate, TSource, TAccumulate>> AggregateState<TSource, TAccumulate>(
            Func<TAccumulate, TSource, TAccumulate> func) =>
                from state in GetState<Tuple<TAccumulate, IEnumerable<TSource>>>() // State<Tuple<TAccumulate, IEnumerable<TSource>>, Tuple<TAccumulate, IEnumerable<TSource>>>.
                let accumulate = state.Item1 // TAccumulate.
                let source = state.Item2.Share() // IBuffer<TSource>.
                let sourceIterator = source.GetEnumerator() // IEnumerator<TSource>.
                from result in sourceIterator.MoveNext()
                    ? (from unit in SetState(func(accumulate, sourceIterator.Current).Tuple(source.AsEnumerable())) // State<Tuple<TAccumulate, IEnumerable<TSource>>, Unit>.
                       from value in AggregateState(func) // State<Tuple<TAccumulate, IEnumerable<TSource>>, Func<TAccumulate, TSource, TAccumulate>>.
                       select func)
                    : func.State<Tuple<TAccumulate, IEnumerable<TSource>>, Func<TAccumulate, TSource, TAccumulate>>() // State<Tuple<TAccumulate, IEnumerable<TSource>>, Func<TAccumulate, TSource, TAccumulate>>.
                select result;

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            State<Tuple<TAccumulate, IEnumerable<TSource>>, Func<TAccumulate, TSource, TAccumulate>> query =
                AggregateState(func); // Define query.
            return query(seed.Tuple(source)).Item2.Item1; // Execute query, with initial state (seed, source).
        }
    }

    public static partial class StateExtensions
    {
        // PopState: Unit -> (IEnumerable<T> -> Tuple<T, IEnumerable<T>>)
        // PopState: Unit -> State<IEnumerable<T>, T>
        internal static State<IEnumerable<T>, T> PopState<T>(Unit unit = null) =>
            oldStack =>
            {
                IEnumerable<T> newStack = oldStack.Share();
                return newStack.First().Tuple(newStack); // Output new state.
            };

        // PushState: T -> (IEnumerable<T> -> Tuple<Unit, IEnumerable<T>>)
        // PushState: T -> State<IEnumerable<T>, Unit>
        internal static State<IEnumerable<T>, Unit> PushState<T>(T value) =>
            oldStack =>
            {
                IEnumerable<T> newStack = oldStack.Concat(value.Enumerable());
                return default(Unit).Tuple(newStack); // Output new state.
            };

        internal static void Stack()
        {
            IEnumerable<int> initialStack = Enumerable.Repeat(0, 5);
            State<IEnumerable<int>, IEnumerable<int>> query =
                from value1 in PopState<int>() // State<IEnumerable<int>, int>.
                from unit1 in PushState(1) // State<IEnumerable<int>, Unit>.
                from unit2 in PushState(2) // State<IEnumerable<int>, Unit>.
                from stack in GetState<IEnumerable<int>>() // State<IEnumerable<int>, IEnumerable<int>>.
                from unit3 in SetState(Enumerable.Range(0, 5)) // State<IEnumerable<int>, Unit>.
                from value2 in PopState<int>() // State<IEnumerable<int>, int>.
                from value3 in PopState<int>() // State<IEnumerable<int>, int>.
                from unit4 in PushState(5) // State<IEnumerable<int>, Unit>.
                select stack; // Define query.
            Tuple<IEnumerable<int>, IEnumerable<int>> result = query(initialStack); // Execute query with initial state.
            result.Item1.WriteLines(); // 0 0 0 0 1 2
            result.Item2.WriteLines(); // 0 1 2 5
        }
    }

    public static partial class StateExtensions
    {
        public static TSource Value<TState, TSource>(this State<TState, TSource> source, TState state) =>
            source(state).Item1;

        public static TState State<TState, T>(this State<TState, T> source, TState state) =>
            source(state).Item2;
    }
}

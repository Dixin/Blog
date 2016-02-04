namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    // State<T, TState> is alias of Func<TState, Lazy<T, TState>>
    public delegate Lazy<T, TState> State<T, TState>(TState state);

    [Pure]
    public static partial class StateExtensions
    {
        // Required by LINQ.
        public static State<TResult, TState> SelectMany<TSource, TState, TSelector, TResult>
            (this State<TSource, TState> source,
             Func<TSource, State<TSelector, TState>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                state => new Lazy<TResult, TState>(() =>
                    {
                        Lazy<TSource, TState> sourceResult = source(state);
                        Lazy<TSelector, TState> selectorResult = selector(sourceResult.Value1)(sourceResult.Value2);
                        return Tuple.Create(
                            resultSelector(sourceResult.Value1, selectorResult.Value1),
                            selectorResult.Value2);
                    });

        // Not required, just for convenience.
        public static State<TResult, TState> SelectMany<TSource, TState, TResult>
            (this State<TSource, TState> source, Func<TSource, State<TResult, TState>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class StateExtensions
    {
        // η: T -> State<T, TState>
        public static State<T, TState> State<T, TState>
            (this T value) => state => new Lazy<T, TState>(value, state);

        // η: T -> State<T, TState>
        public static State<T, TState> State<T, TState>
            (this T value, Func<TState, TState> newState) =>
                oldState => new Lazy<T, TState>(value, newState(oldState));

        // φ: Lazy<State<T1, TState>, State<T2, TState>> => State<Defer<T1, T2>, TState>
        public static State<Lazy<T1, T2>, TState> Binary<T1, T2, TState>
            (this Lazy<State<T1, TState>, State<T2, TState>> binaryFunctor) =>
                binaryFunctor.Value1.SelectMany(
                    value1 => binaryFunctor.Value2,
                    (value1, value2) => new Lazy<T1, T2>(value1, value2));

        // ι: TUnit -> State<TUnit, TState>
        public static State<Unit, TState> Unit<TState>
            (Unit unit) => unit.State<Unit, TState>();

        // Select: (TSource -> TResult) -> (State<TSource, TState> -> State<TResult, TState>)
        public static State<TResult, TState> Select<TSource, TResult, TState>
            (this State<TSource, TState> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).State<TResult, TState>());
    }

    // [Pure]
    public static partial class StateExtensions
    {
        public static TSource Value<TSource, TState>
            (this State<TSource, TState> source, TState state) => source(state).Value1;

        public static TState State<T, TState>
            (this State<T, TState> source, TState state) => source(state).Value2;
    }

    [Pure]
    public static class State
    {
        public static State<TState, TState> Get<TState>
            () => state => new Lazy<TState, TState>(state, state);

        public static State<TState, TState> Set<TState>
            (TState newState) => oldState => new Lazy<TState, TState>(oldState, newState);

        public static State<TState, TState> Set<TState>
            (Func<TState, TState> newState) => oldState => new Lazy<TState, TState>(oldState, newState(oldState));
    }

    public delegate IO<Task<TrafficLightState>> TrafficLightState();

    // Impure.
    internal static partial class StateQuery
    {
        [Pure]
        internal static IO<Task<TrafficLightState>> GreenState
            () =>
                from _ in TraceHelper.Log(nameof(GreenState))
                select (from __ in Task.Delay(TimeSpan.FromSeconds(3))
                        select new TrafficLightState(YellowState));

        [Pure]
        internal static IO<Task<TrafficLightState>> YellowState
            () =>
                from _ in TraceHelper.Log(nameof(YellowState))
                select (from __ in Task.Delay(TimeSpan.FromSeconds(1))
                        select new TrafficLightState(RedState));

        [Pure]
        internal static IO<Task<TrafficLightState>> RedState
            () =>
                from _ in TraceHelper.Log(nameof(RedState))
                select (from __ in Task.Delay(TimeSpan.FromSeconds(2))
                        select default(TrafficLightState));
    }

    // Impure.
    internal static partial class StateQuery
    {
        [Pure]
        internal static State<Unit, IO<Task<TrafficLightState>>> MoveNext
            () =>
                ((Unit)null).State<Unit, IO<Task<TrafficLightState>>>(state => async () =>
                    {
                        TrafficLightState next = await (state ?? GreenState())();
                        return next == null ? null : await next()();
                    });

        [Pure]
        internal static IO<Task<TrafficLightState>> TrafficLight(IO<Task<TrafficLightState>> state = null)
        {
            State<Unit, IO<Task<TrafficLightState>>> query =
                from green in MoveNext()
                from yellow in MoveNext()
                from red in MoveNext()
                select (Unit)null; // Deferred and lazy.
            return query.State(state); // Final state.
        }
    }

    // Impure.
    internal static partial class StateQuery
    {
        internal static async void ExecuteTrafficLight() => await TrafficLight()();
    }

    // [Pure]
    public static partial class EnumerableExtensions
    {
        public static Lazy<T, IEnumerable<T>> Pop<T>
            (this IEnumerable<T> source) =>
                // The execution of First is deferred, so that Pop is still pure.
                new Lazy<T, IEnumerable<T>>(source.First, () => source.Skip(1));

        public static Lazy<T, IEnumerable<T>> Push<T>
            (this IEnumerable<T> source, T value) =>
                new Lazy<T, IEnumerable<T>>(value, source.Concat(value.Enumerable()));
    }

    // Impure.
    internal static partial class StateQuery
    {
        [Pure]
        internal static State<T, IEnumerable<T>> Pop<T>
            () => source => source.Pop();

        [Pure]
        internal static State<T, IEnumerable<T>> Push<T>
            (T value) => source => source.Push(value);

        [Pure]
        internal static IEnumerable<int> Stack(IEnumerable<int> state = null)
        {
            state = state ?? Enumerable.Empty<int>();
            State<IEnumerable<int>, IEnumerable<int>> query =
                from value1 in Push(1)
                from value2 in Push(2)
                from value3 in Pop<int>()
                from stack1 in State.Set(Enumerable.Range(0, 3))
                from value4 in Push(4)
                from value5 in Pop<int>()
                from stack2 in State.Get<IEnumerable<int>>()
                select stack2;
            return query.Value(state);
        }
    }

    // Impure.
    internal class TrafficLightStateMachine
    {
        internal ITrafficLightState State { get; private set; }

        internal async Task MoveNext(ITrafficLightState state = null)
        {
            this.State = state ?? new GreenState();
            await this.State.Handle(this);
        }

        internal static async void Execute() => await new TrafficLightStateMachine().MoveNext();
    }

    internal interface ITrafficLightState // State
    {
        Task Handle(TrafficLightStateMachine light);
    }

    // Impure.
    internal class GreenState : ITrafficLightState // ConcreteStateA
    {
        public async Task Handle(TrafficLightStateMachine light)
        {
            TraceHelper.Log(nameof(GreenState)).Invoke();
            await Task.Delay(3000);
            await light.MoveNext(new YellowState());
        }
    }

    // Impure.
    internal class YellowState : ITrafficLightState // ConcreteStateB
    {
        public async Task Handle(TrafficLightStateMachine light)
        {
            TraceHelper.Log(nameof(YellowState)).Invoke();
            await Task.Delay(1000);
            await light.MoveNext(new RedState());
        }
    }

    // Impure.
    internal class RedState : ITrafficLightState // ConcreteStateC
    {
        public async Task Handle(TrafficLightStateMachine light)
        {
            TraceHelper.Log(nameof(RedState)).Invoke();
            await Task.Delay(2000);
            // await light.MoveNext(new GreenState());
        }
    }
}

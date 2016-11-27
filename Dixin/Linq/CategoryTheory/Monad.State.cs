namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    // State<TState, T> is alias of Func<TState, Lazy<T, TState>>
    public delegate Lazy<T, TState> State<TState, T>(TState state);

    public static partial class StateExtensions
    {
        public static State<TState, TResult> SelectMany<TState, TSource, TSelector, TResult>(
            this State<TState, TSource> source,
            Func<TSource, State<TState, TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                state => new Lazy<TResult, TState>(() =>
                {
                    Lazy<TSource, TState> value = source(state);
                    Lazy<TSelector, TState> result = selector(value.Value1)(value.Value2);
                    return resultSelector(value.Value1, result.Value1).Tuple(result.Value2);
                });

        // Wrap: T -> State<T, TState>
        public static State<TState, T> State<TState, T>(this T value) =>
            state => value.Lazy(state);
    }

    public static partial class StateExtensions
    {
        // η: T -> State<T, TState>
        public static State<TState, T> State<TState, T>(this T value, Func<TState, TState> newState) =>
                oldState => value.Lazy(newState(oldState));
    }

    public static partial class StateExtensions
    {
        public static TSource Value<TState, TSource>(this State<TState, TSource> source, TState state) =>
            source(state).Value1;

        public static TState State<TState, T>(this State<TState, T> source, TState state) =>
            source(state).Value2;
    }

    public static class State
    {
        public static State<TState, TState> Get<TState>() =>
            state => state.Lazy(state);

        public static State<TState, Unit> Set<TState>(TState newState) =>
            oldState => default(Unit).Lazy(newState);
    }

    public delegate IO<Task<TrafficLightState>> TrafficLightState();

    // Impure.
    public static partial class StateExtensions
    {
        internal static IO<Task<TrafficLightState>> GreenState() =>
            from _ in TraceHelper.Log(nameof(GreenState))
            select (from __ in Task.Delay(TimeSpan.FromSeconds(3))
                    select new TrafficLightState(YellowState));

        internal static IO<Task<TrafficLightState>> YellowState() =>
            from _ in TraceHelper.Log(nameof(YellowState))
            select (from __ in Task.Delay(TimeSpan.FromSeconds(1))
                    select new TrafficLightState(RedState));

        internal static IO<Task<TrafficLightState>> RedState() =>
            from _ in TraceHelper.Log(nameof(RedState))
            select (from __ in Task.Delay(TimeSpan.FromSeconds(2))
                    select default(TrafficLightState));
    }

    // Impure.
    public static partial class StateExtensions
    {
        internal static State<IO<Task<TrafficLightState>>, Unit> MoveNext() =>
            ((Unit)null).State<IO<Task<TrafficLightState>>, Unit>(state => async () =>
                {
                    TrafficLightState next = await (state ?? GreenState())();
                    return next == null ? null : await next()();
                });

        internal static IO<Task<TrafficLightState>> TrafficLight(IO<Task<TrafficLightState>> state = null)
        {
            State<IO<Task<TrafficLightState>>, Unit> query =
                from green in MoveNext()
                from yellow in MoveNext()
                from red in MoveNext()
                select (Unit)null; // Deferred and lazy.
            return query.State(state); // Final state.
        }
    }

    // Impure.
    public static partial class StateExtensions
    {
        internal static async void ExecuteTrafficLight() => await TrafficLight()();
    }

    public static partial class EnumerableExtensions
    {
        public static Lazy<T, IEnumerable<T>> Pop<T>(this IEnumerable<T> source) =>
            // The execution of First is deferred, so that Pop is still pure.
            new Lazy<T, IEnumerable<T>>(() => source.First().Tuple(source.Skip(1)));

        public static Lazy<T, IEnumerable<T>> Push<T>(this IEnumerable<T> source, T value) =>
            value.Lazy(source.Concat(value.Enumerable()));
    }

    // Impure.
    internal static partial class StateQuery
    {
        internal static State<IEnumerable<T>, T> Pop<T>() => source => source.Pop();

        internal static State<IEnumerable<T>, T> Push<T>(T value) => source => source.Push(value);

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

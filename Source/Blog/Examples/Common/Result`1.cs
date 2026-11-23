namespace Examples.Common;

using Examples.Threading.Tasks;

[Union]
[AsyncMethodBuilder(typeof(AsyncResultMethodBuilder<>))]
public readonly record struct Result<TResult> : IUnion, IAwaitable<TResult, ResultAwaiter<TResult>>
    where TResult : notnull
{
    private readonly TResult? result;
    private readonly Exception? error;

    public Result(TResult value) => this.result = value;

    public Result(Exception value) => this.error = value;

    public object? Value => this.error is null ? this.result : this.error;

    public bool HasValue => this.error is null;

    public bool TryGetValue([NotNullWhen(true)] out Exception? value)
    {
        value = this.error;
        return this.error is not null;
    }

    public bool TryGetValue([NotNullWhen(true)] out TResult? value)
    {
        value = this.result;
        return this.error is null;
    }

    public ResultAwaiter<TResult> GetAwaiter() => new(this switch
    {
        TResult value => ValueTask.FromResult(value).GetAwaiter(),
        Exception exception => ValueTask.FromException<TResult>(exception).GetAwaiter()
    });
}

public readonly struct ResultAwaiter<TResult>(ValueTaskAwaiter<TResult> valueTaskAwaiter) : IAwaiter<TResult>
where TResult : notnull
{
    public void OnCompleted(Action continuation) => valueTaskAwaiter.OnCompleted(continuation);

    public bool IsCompleted => valueTaskAwaiter.IsCompleted;

    public TResult GetResult() => valueTaskAwaiter.GetResult();
}

public struct AsyncResultMethodBuilder<T> : IAsyncMethodBuilder<T, AsyncResultMethodBuilder<T>, Result<T>>
    where T : notnull
{
    private AsyncValueTaskMethodBuilder<Result<T>> valueTaskBuilder;

    public static AsyncResultMethodBuilder<T> Create() =>
        new() { valueTaskBuilder = AsyncValueTaskMethodBuilder<Result<T>>.Create() };

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        this.valueTaskBuilder.Start(ref stateMachine);

    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        this.valueTaskBuilder.SetStateMachine(stateMachine);

    public void SetResult(T result) => this.valueTaskBuilder.SetResult(result);

    public void SetException(Exception exception) => this.valueTaskBuilder.SetResult(exception);

    public Result<T> Task => this.valueTaskBuilder.Task.AsTask().Exception ?? this.valueTaskBuilder.Task.Result;

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        this.valueTaskBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        this.valueTaskBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
}

public static partial class Result
{
    public static Result<TResult, TError> AsException<TResult, TError>(this Result<TResult> source)
        where TResult : notnull
        where TError : Exception =>
        source switch
        {
            TResult value => value,
            Exception error => (TError)error,
        };

    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector)
        where TSource : notnull
        where TResult : notnull =>
        source switch
        {
            TSource value => selector(value),
            Exception error => error,
        };

    public static Result<TResult> SelectMany<TSource, TMiddle, TResult>(this Result<TSource> source, Func<TSource, Result<TMiddle>> selector, Func<TSource, TMiddle, TResult> resultSelector)
        where TSource : notnull
        where TMiddle : notnull
        where TResult : notnull =>
        source switch
        {
            TSource value => selector(value) switch
            {
                TMiddle middle => resultSelector(value, middle),
                Exception error => error
            },
            Exception error => error
        };

    public static Result<Unit> Ensure(bool condition, Exception error) =>
        condition
            ? Unit()
            : error;

    public static Result<Unit> Ensure(bool condition, Func<Exception> errorFactory) =>
        condition
            ? Unit()
            : errorFactory();

    public static Result<TResult> Ensure<TResult>(this Result<TResult> result, bool condition, Exception error)
        where TResult : notnull =>
        condition
            ? result
            : error;

    public static Result<TResult> Ensure<TResult>(this Result<TResult> result, bool condition, Func<Exception> errorFactory)
        where TResult : notnull =>
        condition
            ? result
            : errorFactory();

    public static Result<TResult> Tap<TResult>(
        this Result<TResult> result,
        Action<TResult> action)
        where TResult : notnull =>
        result switch
        {
            TResult value => new Func<TResult>(() =>
            {
                action(value);
                return value;
            })(),
            Exception error => error
        };

    public static Result<TResult> TapError<TResult>(
        this Result<TResult> result,
        Action<Exception> action)
        where TResult : notnull =>
        result switch
        {
            TResult value => value,
            Exception error => new Func<Exception>(() =>
            {
                action(error);
                return error;
            })()
        };

    public static Result<(TResult1, TResult2)> Zip<TResult1, TResult2>(
        this Result<TResult1> result1,
        Result<TResult2> result2)
        where TResult1 : notnull
        where TResult2 : notnull =>
        from value1 in result1
        from value2 in result2
        select (value1, value2);

    public static Result<(TResult1, TResult2)> Zip<TResult1, TResult2>(
        Func<Result<TResult1>> factory1,
        Func<Result<TResult2>> factory2)
        where TResult1 : notnull
        where TResult2 : notnull =>
        from value1 in factory1()
        from value2 in factory2()
        select (value1, value2);

    public static Result<(TResult1, TResult2)> ZipAll<TResult1, TResult2>(
        this Result<TResult1> result1,
        Result<TResult2> result2)
        where TResult1 : notnull
        where TResult2 : notnull =>
        (result1, result2) switch
        {
            (TResult1 value1, TResult2 value2) => (value1, value2),
            (TResult1 value1, Exception error2) => new AggregateException(innerExceptions: [error2]),
            (Exception error1, TResult2 value2) => new AggregateException(innerExceptions: [error1]),
            (Exception error1, Exception error2) => new AggregateException(innerExceptions: [error1, error2])
        };

    public static Result<(TResult1, TResult2)> ZipAll<TResult1, TResult2>(
        Func<Result<TResult1>> factory1,
        Func<Result<TResult2>> factory2)
        where TResult1 : notnull
        where TResult2 : notnull =>
        (factory1(), factory2()) switch
        {
            (TResult1 value1, TResult2 value2) => (value1, value2),
            (TResult1 value1, Exception error2) => new AggregateException(innerExceptions: [error2]),
            (Exception error1, TResult2 value2) => new AggregateException(innerExceptions: [error1]),
            (Exception error1, Exception error2) => new AggregateException(innerExceptions: [error1, error2])
        };

    public static TResult ValueOrDefault<TResult>(this Result<TResult> result, TResult defaultValue)
        where TResult : notnull =>
        result switch
        {
            TResult value => value,
            Exception _ => defaultValue
        };

    public static TResult ValueOrDefault<TResult>(this Result<TResult> result, Func<Exception, TResult> defaultFactory)
        where TResult : notnull =>
        result switch
        {
            TResult value => value,
            Exception error => defaultFactory(error)
        };

    public static bool IsOk<TResult>(this Result<TResult> result)
        where TResult : notnull =>
        result is TResult;

    public static bool IsError<TResult>(this Result<TResult> result)
        where TResult : notnull =>
        result is Exception;
}

public static partial class Result
{
    public static Result<TResult> OkOr<TResult>(this Option<TResult> option, Exception error)
        where TResult : notnull =>
        option switch
        {
            TResult value => value,
            None _ => error
        };

    public static Result<TResult> OkOr<TResult>(this Option<TResult> option, Func<Exception> errorFactory)
        where TResult : notnull =>
        option switch
        {
            TResult value => value,
            None _ => errorFactory()
        };

    public static Option<TResult> Ok<TResult>(this Result<TResult> result)
        where TResult : notnull =>
        result switch
        {
            TResult value => value,
            Exception _ => Option.None
        };

    public static Option<Exception> Error<TResult>(this Result<TResult> result)
        where TResult : notnull =>
        result switch
        {
            Exception error => error,
            TResult _ => Option.None
        };
}

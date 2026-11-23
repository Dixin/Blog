using Examples.Threading.Tasks;

namespace Examples.Common;

[Union]
public readonly record struct Result<TResult, TError> : IUnion, IAwaitable<TResult, Result<TResult, TError>>, ICriticalAwaiter<TResult>
    where TResult : notnull
    where TError : notnull
{
    private readonly TResult? result;
    private readonly TError? error;

    public Result(TResult value) => this.result = value;

    public Result(TError value) => this.error = value;

    public object? Value => this.result is not null ? this.result : this.error;

    public bool HasValue => this.result is not null;

    public bool TryGetValue([NotNullWhen(true)] out TError? value)
    {
        value = this.error;
        return this.error is not null;
    }

    public bool TryGetValue([NotNullWhen(true)] out TResult? value)
    {
        value = this.result;
        return this.result is not null;
    }

    public Result<TResult, TError> GetAwaiter() => this;

    public void OnCompleted(Action continuation) => continuation();

    public void UnsafeOnCompleted(Action continuation) => continuation();

    public bool IsCompleted => true;

    public TResult GetResult() => this.result is not null ? this.result : throw new ResultException<TError>(this.error!);
}

public static partial class Result
{
    public static Result<TResult> AsException<TResult, TError>(this Result<TResult, TError> source)
        where TResult : notnull
        where TError : Exception =>
        source switch
        {
            TResult value => value,
            TError error => error,
        };

    extension<TResult, TError>(Result<TResult, TError>)
        where TResult : notnull
        where TError : Exception
    {
        public static Result<TResult> operator ~(Result<TResult, TError> source) => source.AsException();
    }


    public static Result<TResult, TError> Select<TSource, TResult, TError>(this Result<TSource, TError> source, Func<TSource, TResult> selector)
        where TSource : notnull
        where TError : notnull
        where TResult : notnull =>
        source switch
        {
            TSource value => selector(value),
            TError error => error,
        };

    public static Result<TResult, TError> SelectMany<TSource, TError, TMiddle, TResult>(this Result<TSource, TError> source, Func<TSource, Result<TMiddle, TError>> selector, Func<TSource, TMiddle, TResult> resultSelector)
        where TSource : notnull
        where TError : notnull
        where TMiddle : notnull
        where TResult : notnull =>
        source switch
        {
            TSource value => selector(value) switch
            {
                TMiddle middle => resultSelector(value, middle),
                TError error => error
            },
            TError error => error
        };

    public static Result<Unit, TError> Ensure<TError>(bool condition, TError error)
        where TError : notnull =>
        condition
            ? Unit()
            : error;

    public static Result<Unit, TError> Ensure<TError>(bool condition, Func<TError> errorFactory)
        where TError : notnull =>
        condition
            ? Unit()
            : errorFactory();

    public static Result<TResult, TError> Ensure<TResult, TError>(this Result<TResult, TError> result, bool condition, TError error)
        where TError : notnull
        where TResult : notnull =>
        condition
            ? result
            : error;

    public static Result<TResult, TError> Ensure<TResult, TError>(this Result<TResult, TError> result, bool condition, Func<TError> errorFactory)
        where TError : notnull
        where TResult : notnull =>
        condition
            ? result
            : errorFactory();

    public static Result<TResult, TError> Tap<TResult, TError>(
        this Result<TResult, TError> result,
        Action<TResult> action)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TResult value => new Func<TResult>(() =>
            {
                action(value);
                return value;
            })(),
            TError error => error
        };

    public static Result<TResult, TError> TapError<TResult, TError>(
        this Result<TResult, TError> result,
        Action<TError> action)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TResult value => value,
            TError error => new Func<TError>(() =>
            {
                action(error);
                return error;
            })()
        };

    public static Result<(TResult1, TResult2), TError> Zip<TResult1, TResult2, TError>(
        this Result<TResult1, TError> result1,
        Result<TResult2, TError> result2)
        where TError : notnull
        where TResult1 : notnull
        where TResult2 : notnull =>
        from value1 in result1
        from value2 in result2
        select (value1, value2);

    public static Result<(TResult1, TResult2), TError> Zip<TResult1, TResult2, TError>(
        Func<Result<TResult1, TError>> factory1,
        Func<Result<TResult2, TError>> factory2)
        where TError : notnull
        where TResult1 : notnull
        where TResult2 : notnull =>
        from value1 in factory1()
        from value2 in factory2()
        select (value1, value2);

    public static Result<(TResult1, TResult2), TError[]> ZipAll<TResult1, TResult2, TError>(
        this Result<TResult1, TError> result1,
        Result<TResult2, TError> result2)
        where TError : notnull
        where TResult1 : notnull
        where TResult2 : notnull =>
        (result1, result2) switch
        {
            (TResult1 value1, TResult2 value2) => (value1, value2),
            (TResult1 value1, TError error2) => new[] { error2 },
            (TError error1, TResult2 value2) => new[] { error1 },
            (TError error1, TError error2) => new[] { error1, error2 }
        };

    public static Result<(TResult1, TResult2), TError[]> ZipAll<TResult1, TResult2, TError>(
        Func<Result<TResult1, TError>> factory1,
        Func<Result<TResult2, TError>> factory2)
        where TError : notnull
        where TResult1 : notnull
        where TResult2 : notnull =>
        (factory1(), factory2()) switch
        {
            (TResult1 value1, TResult2 value2) => (value1, value2),
            (TResult1 value1, TError error2) => new[] { error2 },
            (TError error1, TResult2 value2) => new[] { error1 },
            (TError error1, TError error2) => new[] { error1, error2 }
        };

    public static TResult ValueOrDefault<TResult, TError>(this Result<TResult, TError> result, TResult defaultValue)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TResult value => value,
            _ => defaultValue
        };

    public static TResult ValueOrDefault<TResult, TError>(this Result<TResult, TError> result, Func<TError, TResult> defaultFactory)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TResult value => value,
            TError error => defaultFactory(error)
        };

    public static bool IsOk<TResult, TError>(this Result<TResult, TError> result)
        where TResult : notnull
        where TError : notnull =>
        result is TResult;

    public static bool IsError<TResult, TError>(this Result<TResult, TError> result)
        where TResult : notnull
        where TError : notnull =>
        result is TError;
}

public static partial class Result
{
    public static Result<TResult, TError> OkOr<TResult, TError>(this Option<TResult> option, TError error)
        where TResult : notnull
        where TError : notnull =>
        option switch
        {
            TResult value => value,
            None _ => error
        };

    public static Result<TResult, TError> OkOr<TResult, TError>(this Option<TResult> option, Func<TError> errorFactory)
        where TResult : notnull
        where TError : notnull =>
        option switch
        {
            TResult value => value,
            None _ => errorFactory()
        };

    public static Option<TResult> Ok<TResult, TError>(this Result<TResult, TError> result)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TResult value => value,
            TError _ => Option.None
        };

    public static Option<TError> Error<TResult, TError>(this Result<TResult, TError> result)
        where TResult : notnull
        where TError : notnull =>
        result switch
        {
            TError error => error,
            TResult _ => Option.None
        };
}
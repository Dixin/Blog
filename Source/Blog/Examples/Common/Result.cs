namespace Examples.Common;

using System.Collections;

public interface IResult<TResult> where TResult : IResult<TResult>
{
    Exception? Exception { get; }

    bool IsOk { get; }

    ValueTaskAwaiter GetAwaiter();

    static abstract explicit operator ValueTask(in TResult result);

    static abstract TResult operator |(in TResult x, in TResult y);

    static abstract bool operator &(in TResult left, in TResult right);

    static abstract bool operator !(in TResult result);

    static abstract bool operator true(in TResult result);

    static abstract bool operator false(in TResult result);
}

public interface IResult<T, TResult> : IResult<TResult>, IEnumerable<T> where TResult : IResult<T, TResult>
{
    T Value { get; }

    T? ValueOrDefault { get; }

    bool HasValue { get; }

    bool TryGet(out T? value);

    T? Or(T? value);

    new ValueTaskAwaiter<T> GetAwaiter();

    static abstract explicit operator ValueTask<T>(in TResult result);

    static abstract T? operator |(in TResult result, T? value);

    static abstract implicit operator T(TResult result);

    static abstract implicit operator TResult(T value);
}

public readonly struct Result<T> : IResult<T, Result<T>>
{
    private readonly T? value;

    public Result(T value) => this.value = value;

    public Result(Exception exception) => this.Exception = exception ?? throw new ArgumentNullException(nameof(exception));

    public Exception? Exception { get; }

    public bool IsOk => this.Exception is null;

    public bool HasValue => this.IsOk;

    //[field: AllowNull][field: MaybeNull]
    public T Value => this.IsOk ? this.value! : throw new InvalidOperationException("The result does not have value.", this.Exception);

    public T? ValueOrDefault => this.IsOk ? this.value : default;

    public bool TryGet(out T? value)
    {
        if (this.IsOk)
        {
            value = this.value;
            return true;
        }

        value = default;
        return false;
    }

    public T? Or(T? value) => this.IsOk ? this.value : value;

    public ValueTaskAwaiter<T> GetAwaiter() => ((ValueTask<T>)this).GetAwaiter();

    ValueTaskAwaiter IResult<Result<T>>.GetAwaiter() => ((ValueTask)this).GetAwaiter();

    public IEnumerator<T> GetEnumerator()
    {
        if (this.IsOk)
        {
            yield return this.value!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public override string ToString() => this.IsOk ? this.Exception!.ToString() : this.value?.ToString() ?? "<null>";

    public static explicit operator ValueTask(in Result<T> result) => new(((ValueTask<T>)result).AsTask());

    public static explicit operator ValueTask<T>(in Result<T> result) =>
        result.Exception switch
        {
            null => ValueTask.FromResult(result.value!),
            OperationCanceledException operationCanceledException => ValueTask.FromCanceled<T>(operationCanceledException.CancellationToken),
            { } exception => ValueTask.FromException<T>(exception)
        };

    public static T? operator |(in Result<T> result, T? value) => result.Or(value);

    public static Result<T> operator |(in Result<T> x, in Result<T> y) => x.IsOk ? x : y;

    public static bool operator &(in Result<T> left, in Result<T> right) => left.IsOk && right.IsOk;

    public static bool operator !(in Result<T> result) => !result.IsOk;

    public static bool operator true(in Result<T> result) => result.IsOk;

    public static bool operator false(in Result<T> result) => !result.IsOk;

    public static implicit operator T(Result<T> result) => result.Value;

    public static implicit operator Result<T>(T value) => new(value);
}

public readonly partial struct Result(Exception exception) : IResult<Result>
{
    public Exception? Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));

    public bool IsOk => this.Exception is null;

    public ValueTaskAwaiter GetAwaiter() => ((ValueTask)this).GetAwaiter();

    public override string ToString() => this.Exception?.ToString() ?? "<void>";

    public static explicit operator ValueTask(in Result result) =>
        result.Exception switch
        {
            null => ValueTask.CompletedTask,
            OperationCanceledException operationCanceledException => ValueTask.FromCanceled(operationCanceledException.CancellationToken),
            { } exception => ValueTask.FromException(exception)
        };

    public static Result operator |(in Result x, in Result y) => x.IsOk ? x : y;

    public static bool operator &(in Result left, in Result right) => left.IsOk && right.IsOk;

    public static bool operator !(in Result result) => !result.IsOk;

    public static bool operator true(in Result result) => result.IsOk;

    public static bool operator false(in Result result) => !result.IsOk;
}

public readonly partial struct Result
{
    private static readonly Result ok = new();

    public static Result<T> Ok<T>(T value) => new(value);

    public static Result<T> Error<T>(Exception exception) => new(exception);

    public static Result Ok() => ok;

    public static Result Error(Exception exception) => new(exception);
}

// https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-resultmodule.html
public static class ResultExtensions
{
    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector) =>
        source.IsOk ? Result.Ok(selector(source.Value)) : Result.Error<TResult>(source.Exception!);

    public static Result<TResult> SelectMany<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> selector) =>
        source.IsOk ? selector(source.Value) : Result.Error<TResult>(source.Exception!);

    public static bool Contains<TSource>(this Result<TSource> source, TSource value, IEqualityComparer<TSource>? comparer = null) =>
        source.IsOk && (comparer ?? EqualityComparer<TSource>.Default).Equals(source.Value, value);

    public static int Count<TSource>(this Result<TSource> source) => source.IsOk ? 1 : 0;

    public static bool Any<TSource>(this Result<TSource> source, Func<TSource, bool> predicate) =>
        source.IsOk && predicate(source.Value);

    public static void ForEach<TSource>(this Result<TSource> source, Action<TSource> action)
    {
        if (source.IsOk)
        {
            action(source.Value);
        }
    }
}

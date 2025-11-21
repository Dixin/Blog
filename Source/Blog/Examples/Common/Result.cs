namespace Examples.Common;

public readonly struct Result<T>
{
    public Result(T value) => this.Value = value;

    public Result(Exception exception) => this.Exception = exception;

    public Exception? Exception { get; }

    public bool IsOk => this.Exception is null;

    public bool HasValue => this.IsOk;

    //[field: AllowNull][field: MaybeNull]
    public T Value => this.IsOk ? field! : throw new InvalidOperationException("", this.Exception);

    public T? ValueOrDefault => this.IsOk ? this.Value : default;

    public bool TryGet([MaybeNullWhen(false)] out T? value)
    {
        value = this.ValueOrDefault;
        return this.IsOk;
    }

    public T? Or(T? value) => this.IsOk ? this.Value : value;

    public ValueTask<T> AsTask() =>
        this.Exception switch
        {
            null => ValueTask.FromResult(this.Value),
            OperationCanceledException operationCanceledException => ValueTask.FromCanceled<T>(operationCanceledException.CancellationToken),
            { } exception => ValueTask.FromException<T>(exception)
        };

    public override string ToString() => this.Exception?.ToString() ?? this.Value?.ToString() ?? "<null>";

    public static explicit operator ValueTask<T>(in Result<T> result) => result.AsTask();

    public static T? operator |(in Result<T> result, T? value) => result.Or(value);

    public static Result<T> operator |(in Result<T> x, in Result<T> y) => x.IsOk ? x : y;

    public static bool operator &(in Result<T> left, in Result<T> right) => left.IsOk && right.IsOk;

    public static bool operator !(in Result<T> result) => !result.IsOk;

    public static bool operator true(in Result<T> result) => result.IsOk;

    public static bool operator false(in Result<T> result) => !result.IsOk;

    public static implicit operator T(Result<T> result) => result.Value;

    public static implicit operator Result<T>(T value) => new(value);
}

public readonly partial struct Result
{
    public Result() { }

    public Result(Exception exception) => this.Exception = exception;

    public Exception? Exception { get; }

    public bool IsOk => this.Exception is null;

    public ValueTask AsTask() =>
        this.Exception switch
        {
            null => new(),
            OperationCanceledException operationCanceledException => ValueTask.FromCanceled(operationCanceledException.CancellationToken),
            { } exception => ValueTask.FromException(exception)
        };

    public override string ToString() => this.Exception?.ToString() ?? "<void>";

    public static explicit operator ValueTask(in Result result) => result.AsTask();

    public static Result operator |(in Result x, in Result y) => x.IsOk ? x : y;

    public static bool operator &(in Result left, in Result right) => left.IsOk && right.IsOk;

    public static bool operator !(in Result result) => !result.IsOk;

    public static bool operator true(in Result result) => result.IsOk;

    public static bool operator false(in Result result) => !result.IsOk;
}

public readonly partial struct Result
{
    public static Result<T> Ok<T>(T value) => new(value);

    public static Result<T> Error<T>(Exception value) => new(value);

    public static Result Ok() => new();

    public static Result Error(Exception value) => new(value);
}

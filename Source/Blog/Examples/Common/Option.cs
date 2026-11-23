namespace Examples.Common;

public readonly record struct None
{
    public override string ToString() => "None";
}

public union Option<T>(None, T) where T : notnull;

public static class Option
{
    public static Option<T> Some<T>(T value) where T : notnull => value;
    public static None None => default;

    public static Option<T> FromNullable<T>(T? value) where T : class =>
        value is not null ? value : None;

    public static Option<T> FromNullable<T>(T? value) where T : struct =>
        value.HasValue ? value.Value : None;
}

public static class OptionExtensions
{
    public static bool IsSome<T>(this Option<T> option) where T : notnull =>
        option is T;

    public static bool IsNone<T>(this Option<T> option) where T : notnull =>
        option is None;

    public static T ValueOrDefault<T>(this Option<T> option, T fallback) where T : notnull =>
        option switch
        {
            T value => value,
            _ => fallback
        };

    public static T ValueOrDefault<T>(this Option<T> option, Func<T> fallbackFactory) where T : notnull =>
        option switch
        {
            T value => value,
            _ => fallbackFactory()
        };

    public static Option<TOut> Select<T, TOut>(this Option<T> option, Func<T, TOut> mapper)
        where T : notnull
        where TOut : notnull =>
        option switch
        {
            T value => mapper(value),
            _ => Option.None
        };

    public static Option<TOut> SelectMany<T, TOut>(this Option<T> option, Func<T, Option<TOut>> binder)
        where T : notnull
        where TOut : notnull =>
        option switch
        {
            T value => binder(value),
            _ => Option.None
        };
}
namespace Examples.Common;

public static class Argument
{
    [return: NotNull]
    public static T ThrowIfNull<T>(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this T value, [CallerArgumentExpression(nameof(value))] string name = "") =>
        value is null
            ? throw new ArgumentNullException(name)
            : value;

    [return: NotNull]
    public static string ThrowIfNullOrWhiteSpace(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this string value, [CallerArgumentExpression(nameof(value))] string name = "") =>
        string.IsNullOrWhiteSpace(value.ThrowIfNull())
            ? throw new ArgumentException(default, name)
            : value;

    [return: NotNull]
    public static string ThrowIfNullOrEmpty(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this string value, [CallerArgumentExpression(nameof(value))] string name = "") =>
        string.IsNullOrEmpty(value.ThrowIfNull())
            ? throw new ArgumentException(default, name)
            : value;

    [return: NotNull]
    public static IEnumerable<T> ThrowIfNullOrEmpty<T>(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string name = "") =>
        value.ThrowIfNull().IsEmpty() ? throw new ArgumentException(default, name) : value;

    public static void NotNull<T>(Func<T> value)
    {
        if (value() is null)
        {
            throw new ArgumentNullException(GetName(value));
        }
    }

    private static string GetName<T>(Func<T> func)
    {
        // func: () => arg is compiled to DisplayClass with a field and a method. That method is func.
        object displayClassInstance = func.Target!;
        FieldInfo closure = displayClassInstance.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Single();
        return closure.Name;
    }

    public static void NotNull<T>(Expression<Func<T>> value)
    {
        if (value.Compile().Invoke() is null)
        {
            throw new ArgumentNullException(GetName(value));
        }
    }

    private static string GetName<T>(Expression<Func<T>> expression)
    {
        // expression: () => arg is compiled to DisplayClass with a field. Here expression body is to access DisplayClass instance's field.
        MemberExpression displayClassInstance = (MemberExpression)expression.Body;
        MemberInfo closure = displayClassInstance.Member;
        return closure.Name;
    }
}
namespace Examples.Common;

public static class Argument
{
    [return: NotNull]
    public static T ThrowIfNull<T>(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this T argument, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        argument is null
            ? throw new ArgumentNullException(paramName)
            : argument;

    [return: NotNull]
    public static string ThrowIfNullOrWhiteSpace(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this string argument, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        string.IsNullOrWhiteSpace(argument.ThrowIfNull())
            ? throw new ArgumentException(default, paramName)
            : argument;

    [return: NotNull]
    public static string ThrowIfNullOrEmpty(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this string argument, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        string.IsNullOrEmpty(argument.ThrowIfNull())
            ? throw new ArgumentException(default, paramName)
            : argument;

    [return: NotNull]
    public static IEnumerable<T> ThrowIfNullOrEmpty<T>(
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [NotNull]
#endif
        [ValidatedNotNull]this IEnumerable<T>? argument, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        argument.ThrowIfNull().IsEmpty() ? throw new ArgumentException(default, paramName) : argument;

    public static void ThrowIfNull<T>(Func<T> getArgument)
    {
        if (getArgument() is null)
        {
            throw new ArgumentNullException(GetName(getArgument));
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

    public static void ThrowIfNull<T>(Expression<Func<T>> getArgument)
    {
        if (getArgument.Compile().Invoke() is null)
        {
            throw new ArgumentNullException(GetName(getArgument));
        }
    }

    private static string GetName<T>(Expression<Func<T>> expression)
    {
        // expression: () => arg is compiled to DisplayClass with a field. Here expression body is to access DisplayClass instance's field.
        MemberExpression displayClassInstance = (MemberExpression)expression.Body;
        MemberInfo closure = displayClassInstance.Member;
        return closure.Name;
    }

    public static T ThrowIfEqual<T>(this T argument, T value, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        EqualityComparer<T>.Default.Equals(argument, value)
            ? throw new ArgumentOutOfRangeException(paramName, argument, $"{paramName} ('{argument}') must be equal to '{value}'.")
            : argument;

    public static T ThrowIfNotEqual<T>(this T argument, T value, [CallerArgumentExpression(nameof(argument))] string paramName = "") =>
        EqualityComparer<T>.Default.Equals(argument, value)
            ? argument
            : throw new ArgumentOutOfRangeException(paramName, argument, $"{paramName} ('{argument}') must not be equal to '{value}'.");

    public static T ThrowIfLessThan<T>(this T argument, T value, [CallerArgumentExpression(nameof(argument))] string paramName = "") where T : IComparable<T> =>
        argument.CompareTo(value) < 0
            ? throw new ArgumentOutOfRangeException(paramName, argument, $"{paramName} ('{argument}') must be greater than or equal to '{value}'.")
            : argument;
}
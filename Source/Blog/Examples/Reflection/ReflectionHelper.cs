namespace Examples.Reflection;

public static class ReflectionHelper
{
    public static MethodInfo MethodOf<T>
        (Expression<Func<T>> methodCall) => ((MethodCallExpression)methodCall.Body).Method;

    public static MethodInfo MethodOf
        (Expression<Action> methodCall) => ((MethodCallExpression)methodCall.Body).Method;

    public static ConstructorInfo ConstructorOf<T>
        (Expression<Func<T>> constructorCall) => ((NewExpression)constructorCall.Body).Constructor ?? throw new ArgumentOutOfRangeException(nameof(constructorCall));

    public static PropertyInfo PropertyOf<T>
        (Expression<Func<T>> getterCall) => (PropertyInfo)((MemberExpression)getterCall.Body).Member;

    public static PropertyInfo PropertyOf<T>
        (Expression<Action<T>> setterCall) => (PropertyInfo)((MemberExpression)setterCall.Body).Member;

    public static FieldInfo FieldOf<T>
        (Expression<Func<T>> fieldCall) => (FieldInfo)((MemberExpression)fieldCall.Body).Member;

    public static MemberInfo MemberOf<T>
        (Expression<Func<T>> memberCall) => ((MemberExpression)memberCall.Body).Member;

    public static MemberInfo MemberOf
        (Expression<Action> memberCall) => ((MemberExpression)memberCall.Body).Member;

    public static MemberInfo MemberOf<T>
        (Expression<Action<T>> memberCall) => ((MemberExpression)memberCall.Body).Member;
}
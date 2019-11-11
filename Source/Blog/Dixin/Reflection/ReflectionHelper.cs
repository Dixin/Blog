namespace Dixin.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ReflectionHelper
    {
        public static MethodInfo MethodOf<T>
            (Expression<Func<T>> methodCall) => (methodCall.Body as MethodCallExpression)?.Method;

        public static MethodInfo MethodOf
            (Expression<Action> methodCall) => (methodCall.Body as MethodCallExpression)?.Method;

        public static ConstructorInfo ConstructorOf<T>
            (Expression<Func<T>> constructorCall) => (constructorCall.Body as NewExpression)?.Constructor;

        public static PropertyInfo PropertyOf<T>
            (Expression<Func<T>> getterCall) => (getterCall.Body as MemberExpression)?.Member as PropertyInfo;

        public static PropertyInfo PropertyOf<T>
            (Expression<Action<T>> setterCall) => (setterCall.Body as MemberExpression)?.Member as PropertyInfo;

        public static FieldInfo FieldOf<T>
            (Expression<Func<T>> fieldCall) => (fieldCall.Body as MemberExpression)?.Member as FieldInfo;

        public static MemberInfo MemberOf<T>
            (Expression<Func<T>> memberCall) => (memberCall.Body as MemberExpression)?.Member;

        public static MemberInfo MemberOf
            (Expression<Action> memberCall) => (memberCall.Body as MemberExpression)?.Member;

        public static MemberInfo MemberOf<T>
            (Expression<Action<T>> memberCall) => (memberCall.Body as MemberExpression)?.Member;
    }
}
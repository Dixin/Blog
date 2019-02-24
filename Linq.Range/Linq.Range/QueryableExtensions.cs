namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class QueryableExtensions
    {
        public static TSource ElementAt<TSource>(this IQueryable<TSource> source, Index index)
        {
            if (source == null)
                // throw Error.ArgumentNull(nameof(source));
                throw new ArgumentNullException(nameof(source));
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    CachedReflectionInfo.ElementAt_TSource_2(typeof(TSource)),
                    source.Expression, Expression.Constant(index)
                    ));
        }

        public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, Index index)
        {
            if (source == null)
                // throw Error.ArgumentNull(nameof(source));
                throw new ArgumentNullException(nameof(source));
            return source.Provider.Execute<TSource>(
                Expression.Call(
                    null,
                    CachedReflectionInfo.ElementAtOrDefault_TSource_2(typeof(TSource)),
                    source.Expression, Expression.Constant(index)
                    ));
        }

        public static IQueryable<TSource> ElementsIn<TSource>(this IQueryable<TSource> source, Range range)
        {
            if (source == null)
                // throw Error.ArgumentNull(nameof(source));
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    CachedReflectionInfo.ElementsIn_TSource_2(typeof(TSource)),
                    source.Expression, Expression.Constant(range)));
        }
    }

    internal static class CachedReflectionInfo
    {
        private static MethodInfo s_ElementAt_TSource_2;

        public static MethodInfo ElementAt_TSource_2(Type TSource) =>
             (s_ElementAt_TSource_2 ??
             (s_ElementAt_TSource_2 = new Func<IQueryable<object>, Index, object>(QueryableExtensions.ElementAt).GetMethodInfo().GetGenericMethodDefinition()))
              .MakeGenericMethod(TSource);

        private static MethodInfo s_ElementAtOrDefault_TSource_2;

        public static MethodInfo ElementAtOrDefault_TSource_2(Type TSource) =>
             (s_ElementAtOrDefault_TSource_2 ??
             (s_ElementAtOrDefault_TSource_2 = new Func<IQueryable<object>, Index, object>(QueryableExtensions.ElementAtOrDefault).GetMethodInfo().GetGenericMethodDefinition()))
              .MakeGenericMethod(TSource);

        private static MethodInfo s_ElementsIn_TSource_2;

        public static MethodInfo ElementsIn_TSource_2(Type TSource) =>
             (s_ElementsIn_TSource_2 ??
             (s_ElementsIn_TSource_2 = new Func<IQueryable<object>, Range, IQueryable<object>>(QueryableExtensions.ElementsIn).GetMethodInfo().GetGenericMethodDefinition()))
              .MakeGenericMethod(TSource);
    }
}

namespace Examples.Common;

public static class FuncExtensions
{
    extension<T, TResult1, TResult2>(Func<T, TResult1>)
    {
        public static Func<T, TResult2> operator +(Func<T, TResult1> first, Func<TResult1, TResult2> second) => value => second.ThrowIfNull()(first.ThrowIfNull()(value));
    }

    extension<T, TResult>(Func<T, TResult>)
    {
        public static TResult operator >(T arg, Func<T, TResult> func) => func.ThrowIfNull()(arg);

        public static TResult operator <(T arg, Func<T, TResult> func) => throw new NotSupportedException();

        public static TResult operator >(Func<T, TResult> func, T arg) => throw new NotSupportedException();

        public static TResult operator <(Func<T, TResult> func, T arg) => func.ThrowIfNull()(arg);
    }

    extension<T1, T2, TResult>(Func<T1, T2, TResult>)
    {
        public static Func<T2, TResult> operator >(T1 arg1, Func<T1, T2, TResult> func) => arg2 => func.ThrowIfNull()(arg1, arg2);

        public static Func<T2, TResult> operator <(T1 arg1, Func<T1, T2, TResult> func) => throw new NotSupportedException();

        public static Func<T2, TResult> operator >(Func<T1, T2, TResult> func, T1 arg1) => throw new NotSupportedException();

        public static Func<T2, TResult> operator <(Func<T1, T2, TResult> func, T1 arg1) => arg2 => func.ThrowIfNull()(arg1, arg2);

        public static TResult operator >((T1 Arg1, T2 Arg2) args, Func<T1, T2, TResult> func) => func.ThrowIfNull()(args.Arg1, args.Arg2);

        public static TResult operator <((T1 Arg1, T2 Arg2) args, Func<T1, T2, TResult> func) => throw new NotSupportedException();

        public static TResult operator >(Func<T1, T2, TResult> func, (T1 Arg1, T2 Arg2) args) => throw new NotSupportedException();

        public static TResult operator <(Func<T1, T2, TResult> func, (T1 Arg1, T2 Arg2) args) => func.ThrowIfNull()(args.Arg1, args.Arg2);
    }

    extension<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult>)
    {
        public static Func<T2, T3, TResult> operator >(T1 arg1, Func<T1, T2, T3, TResult> func) => (arg2, arg3) => func.ThrowIfNull()(arg1, arg2, arg3);

        public static Func<T2, T3, TResult> operator <(T1 arg1, Func<T1, T2, T3, TResult> func) => throw new NotSupportedException();

        public static Func<T2, T3, TResult> operator >(Func<T1, T2, T3, TResult> func, T1 arg1) => throw new NotSupportedException();

        public static Func<T2, T3, TResult> operator <(Func<T1, T2, T3, TResult> func, T1 arg1) => (arg2, arg3) => func.ThrowIfNull()(arg1, arg2, arg3);

        public static TResult operator >((T1 Arg1, T2 Arg2, T3 Arg3) args, Func<T1, T2, T3, TResult> func) => func.ThrowIfNull()(args.Arg1, args.Arg2, args.Arg3);

        public static TResult operator <((T1 Arg1, T2 Arg2, T3 Arg3) args, Func<T1, T2, T3, TResult> func) => throw new NotSupportedException();

        public static TResult operator >(Func<T1, T2, T3, TResult> func, (T1 Arg1, T2 Arg2, T3 Arg3) args) => throw new NotSupportedException();

        public static TResult operator <(Func<T1, T2, T3, TResult> func, (T1 Arg1, T2 Arg2, T3 Arg3) args) => func.ThrowIfNull()(args.Arg1, args.Arg2, args.Arg3);
    }

    extension<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult>)
    {
        public static Func<T2, T3, T4, TResult> operator >(T1 arg1, Func<T1, T2, T3, T4, TResult> func) => (arg2, arg3, arg4) => func.ThrowIfNull()(arg1, arg2, arg3, arg4);

        public static Func<T2, T3, T4, TResult> operator <(T1 arg1, Func<T1, T2, T3, T4, TResult> func) => throw new NotSupportedException();

        public static Func<T2, T3, T4, TResult> operator >(Func<T1, T2, T3, T4, TResult> func, T1 arg1) => throw new NotSupportedException();

        public static Func<T2, T3, T4, TResult> operator <(Func<T1, T2, T3, T4, TResult> func, T1 arg1) => (arg2, arg3, arg4) => func.ThrowIfNull()(arg1, arg2, arg3, arg4);

        public static TResult operator >((T1 Arg1, T2 Arg2, T3 Arg3, T4 Arg4) args, Func<T1, T2, T3, T4, TResult> func) => func.ThrowIfNull()(args.Arg1, args.Arg2, args.Arg3, args.Arg4);

        public static TResult operator <((T1 Arg1, T2 Arg2, T3 Arg3, T4 Arg4) args, Func<T1, T2, T3, T4, TResult> func) => throw new NotSupportedException();

        public static TResult operator >(Func<T1, T2, T3, T4, TResult> func, (T1 Arg1, T2 Arg2, T3 Arg3, T4 Arg4) args) => throw new NotSupportedException();

        public static TResult operator <(Func<T1, T2, T3, T4, TResult> func, (T1 Arg1, T2 Arg2, T3 Arg3, T4 Arg4) args) => func.ThrowIfNull()(args.Arg1, args.Arg2, args.Arg3, args.Arg4);
    }
}
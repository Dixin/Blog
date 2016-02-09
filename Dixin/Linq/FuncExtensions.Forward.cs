namespace Dixin.Linq
{
    using System;

    using Dixin.Common;

    public static partial class FuncExtensions
    {
        public static TResult Forward<T, TResult>(this T arg, Func<T, TResult> func)
        {
            func.NotNull(nameof(func));

            return func(arg);
        }
    }
}
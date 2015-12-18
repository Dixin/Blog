namespace Dixin.Linq
{
    using System;
    using System.Diagnostics.Contracts;

    public static partial class FuncExtensions
    {
        public static TResult Forward<T, TResult>(this T arg, Func<T, TResult> func)
        {
            Contract.Requires<ArgumentException>(func != null);

            return func(arg);
        }
    }
}
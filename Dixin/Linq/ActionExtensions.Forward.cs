namespace Dixin.Linq
{
    using System;
    using System.Diagnostics.Contracts;

    public static partial class ActionExtensions
    {
        public static void Forward<T>(this T arg, Action<T> action)
        {
            Contract.Requires<ArgumentException>(action != null);

            action(arg);
        }
    }
}
namespace Dixin.Linq
{
    using System;

    using Dixin.Common;

    public static partial class ActionExtensions
    {
        public static void Forward<T>(this T arg, Action<T> action)
        {
            action.NotNull(nameof(action));

            action(arg);
        }
    }
}
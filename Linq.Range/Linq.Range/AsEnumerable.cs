namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<int> AsEnumerable(this Range range)
        {
            return Range(range);
        }
    }
}
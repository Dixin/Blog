namespace Tutorial.Introduction
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    internal class ParallelLinq
    {
        internal static void QueryExpression()
        {
            int[] values = { 4, 3, 2, 1, 0, -1 };
            ParallelQuery<int> source = values.AsParallel(); // Get source.
            ParallelQuery<double> query = from int32 in source
                                          where int32 > 0
                                          orderby int32
                                          select Math.Sqrt(int32); // Define query.
            query.ForAll(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void QueryMethods()
        {
            int[] values = { 4, 3, 2, 1, 0, -1 };
            ParallelQuery<int> source = values.AsParallel(); // Get source.
            ParallelQuery<double> query = source
                .Where(int32 => int32 > 0)
                .OrderBy(int32 => int32)
                .Select(int32 => Math.Sqrt(int32)); // Define query.
            query.ForAll(result => Trace.WriteLine(result)); // Execute query.
        }
    }
}

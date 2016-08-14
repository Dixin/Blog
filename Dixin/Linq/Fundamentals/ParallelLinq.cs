namespace Dixin.Linq.Fundamentals
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
            ParallelQuery<double> query = from value in source
                where value > 0
                orderby value
                select Math.Sqrt(value); // Create query.
            query.ForAll(result => Trace.WriteLine(result)); // Execute query.
        }

        internal static void QueryMethods()
        {
            int[] values = { 4, 3, 2, 1, 0, -1 };
            ParallelQuery<int> source = values.AsParallel(); // Get source.
            ParallelQuery<double> query = source
                .Where(value => value > 0)
                .OrderBy(value => value)
                .Select(value => Math.Sqrt(value)); // Create query.
            query.ForAll(result => Trace.WriteLine(result)); // Execute query.
        }
    }
}

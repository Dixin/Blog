namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> Select1<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Trace.WriteLine("Select query starts.");
            foreach (TSource value in source)
            {
                Trace.WriteLine($"Select is calling selector with {value}.");
                yield return selector(value);
            }

            Trace.WriteLine("Select query ends.");
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> Select2<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                new Sequence<TResult, IEnumerator<TSource>>(null, sourceIterator => new Iterator<TResult>(
                    start: () =>
                        {
                            Trace.WriteLine("Select query starts.");
                            sourceIterator = source.GetEnumerator();
                        },
                    hasNext: () => sourceIterator.MoveNext(),
                    next: () =>
                        {
                            Trace.WriteLine($"Select is calling selector with {sourceIterator.Current}.");
                            return selector(sourceIterator.Current);
                        },
                    dispose: () => sourceIterator?.Dispose(),
                    end: () =>
                        {
                            sourceIterator = null;
                            Trace.WriteLine("Select query ends.");
                        }));

        internal static IEnumerable<TResult> Select3<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Trace.WriteLine("Select query starts.");
            List<TResult> resultSequence = new FluentList<TResult>();
            foreach (TSource value in source)
            {
                Trace.WriteLine($"Select is calling selector with {value}.");
                TResult result = selector(value);
                resultSequence.Add(result);
            }

            Trace.WriteLine("Select query ends.");
            return resultSequence;
        }

        internal static void ForEachSelect()
        {
            IEnumerable<int> squares1 = Enumerable.Range(0, 5).Select1(@int => @int * @int); // Create sequence.
            Trace.WriteLine(nameof(squares1));
            foreach (int _ in squares1) // Iterate sequence.
            {
                // Select query starts.
                // Select is calling selector with 0.
                // Select is calling selector with 1.
                // Select is calling selector with 2.
                // Select is calling selector with 3.
                // Select is calling selector with 4.
                // Select query ends.
            }

            IEnumerable<int> squares2 = Enumerable.Range(0, 5).Select2(@int => @int * @int); // Create sequence.
            Trace.WriteLine(nameof(squares2));
            foreach (int _ in squares2) // Iterate sequence.
            {
                // Select query starts.
                // Select is calling selector with 0.
                // Select is calling selector with 1.
                // Select is calling selector with 2.
                // Select is calling selector with 3.
                // Select is calling selector with 4.
                // Select query ends.
            }

            IEnumerable<int> squares3 = Enumerable.Range(0, 5).Select3(@int => @int * @int); // Create sequence.
            // Select query starts.
            // Select is calling selector with 0.
            // Select is calling selector with 1.
            // Select is calling selector with 2.
            // Select is calling selector with 3.
            // Select is calling selector with 4.
            // Select query ends.
            Trace.WriteLine(nameof(squares3));
            foreach (int _ in squares3) // Iterate sequence.
            {
            }
        }

        internal static IEnumerable<TResult> Select<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        internal static void Tasks()
        {
            Task coldTask = new Task(() => Trace.WriteLine("Task is running.")); // Deferred execution. Created task is not started.
            Task hotTask = Task.Run(() => Trace.WriteLine("Task is running.")); // Immediate execution. Created task is started.

            coldTask.Start();
        }

        internal static IEnumerable<TSource> Reverse1<TSource>(this IEnumerable<TSource> source)
        {
            Trace.WriteLine("Reverse query starts.");
            TSource[] array = source.ToArray();
            Trace.WriteLine($"Reverse evaluated {array.Length} value(s) in source sequence.");
            for (int index = array.Length - 1; index >= 0; index--)
            {
                Trace.WriteLine($"Reverse is yielding position {index} of source sequence.");
                yield return array[index];
            }

            Trace.WriteLine("Reverse query ends.");
        }

        internal static IEnumerable<TSource> Reverse2<TSource>
            (this IEnumerable<TSource> source) => new Sequence<TSource, Tuple<TSource[], int>>(
                Tuple.Create(default(TSource[]), 0),
                data => new Iterator<TSource>(
                    start: () =>
                        {
                            Trace.WriteLine("Reverse query starts.");
                            TSource[] array = source.ToArray();
                            Trace.WriteLine($"Reverse evaluated {array.Length} value(s) in source sequence.");
                            data = Tuple.Create(array, array.Length - 1);
                        },
                    hasNext: () => data.Item2 >= 0,
                    next: () =>
                        {
                            int index = data.Item2;
                            data = Tuple.Create(data.Item1, index - 1);
                            Trace.WriteLine($"Reverse is yielding position {index} of source sequence.");
                            return data.Item1[index];
                        },
                    end: () =>
                        {
                            data = null;
                            Trace.WriteLine("Reverse query ends.");
                        }));

        internal static void ForEachReverse()
        {
            IEnumerable<int> squares = Enumerable.Range(0, 5).Select1(@int => @int * @int); // Deferred execution.
            IEnumerable<int> reverse = squares.Reverse1(); // Deferred execution.
            using (IEnumerator<int> reverseIterator = reverse.GetEnumerator())
            {
                if (reverseIterator.MoveNext()) // Eager evaluation.
                {
                    // Reverse query starts.
                    // Select query starts.
                    // Select is calling selector with 0.
                    // Select is calling selector with 1.
                    // Select is calling selector with 2.
                    // Select is calling selector with 3.
                    // Select is calling selector with 4.
                    // Select query ends.
                    // Reverse evaluated 5 value(s) in source sequence.
                    // Reverse is yielding position 4 of source sequence.
                    Trace.WriteLine(reverseIterator.Current); // 16.
                    if (reverseIterator.MoveNext())
                    {
                        // Reverse is yielding position 3 of source sequence.
                        Trace.WriteLine(reverseIterator.Current); // 9.
                    }
                }
            }
        }
    }
}

namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Trace.WriteLine("Select query starts.");
            foreach (TSource value in source)
            {
                Trace.WriteLine($"Select query is calling selector with {value}.");
                TResult result = selector(value);
                Trace.WriteLine($"Select query evaluated and is yielding {result}.");
                yield return result;
            }
            Trace.WriteLine("Select query ends.");
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> DesugaredSelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Trace.WriteLine("Select query starts.");
            IEnumerator<TSource> sourceIterator = null; // start.
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    Trace.WriteLine($"Select query is calling selector with {sourceIterator.Current}."); // getCurrent.
                    TResult result = selector(sourceIterator.Current); // getCurrent.
                    Trace.WriteLine($"Select query evaluated and is yielding {result}."); // getCurrent.
                    yield return result; // getCurrent.
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }
            Trace.WriteLine("Select query ends."); // end.
        }

        internal static IEnumerable<TResult> CompiledSelectGenerator<TSource, TResult>(
                this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            new Generator<TResult, IEnumerator<TSource>>(
                data: null, // IEnumerator<TSource> sourceIterator = null;
                iteratorFactory: sourceIterator => new Iterator<TResult>(
                    start: () =>
                        {
                            Trace.WriteLine("Select query starts.");
                            sourceIterator = source.GetEnumerator();
                        },
                    moveNext: () => sourceIterator.MoveNext(),
                    getCurrent: () =>
                        {
                            Trace.WriteLine($"Select query is calling selector with {sourceIterator.Current}.");
                            TResult result = selector(sourceIterator.Current);
                            Trace.WriteLine($"Select query evaluated and is yielding {result}.");
                            return result;
                        },
                    dispose: () => sourceIterator?.Dispose(),
                    end: () => Trace.WriteLine("Select query ends.")));

        internal static IEnumerable<TResult> SelectList<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Trace.WriteLine("Select query starts.");
            List<TResult> resultSequence = new List<TResult>();
            foreach (TSource value in source)
            {
                Trace.WriteLine($"Select query is calling selector with {value}.");
                TResult result = selector(value);
                Trace.WriteLine($"Select query evaluated and is adding {result}.");
                resultSequence.Add(result);
            }

            Trace.WriteLine("Select query ends.");
            return resultSequence;
        }

        internal static void ForEachSelect()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5).SelectGenerator(int32 => new string('*', int32));
            foreach (string result in deferredQuery) // Execute query.
            {
                // Select query starts.
                // Select query is calling selector with 1.
                // Select query evaluated and is yielding *.
                // Select query is calling selector with 2.
                // Select query evaluated and is yielding **.
                // Select query is calling selector with 3.
                // Select query evaluated and is yielding ***.
                // Select query is calling selector with 4.
                // Select query evaluated and is yielding ****.
                // Select query is calling selector with 5.
                // Select query evaluated and is yielding *****.
                // Select query ends.
            }

            IEnumerable<string> immediateQuery = Enumerable.Range(1, 5).SelectList(int32 => new string('*', int32));
                // Execute query.
            // Select query starts.
            // Select query is calling selector with 1.
            // Select query evaluated and is yielding *.
            // Select query is calling selector with 2.
            // Select query evaluated and is yielding **.
            // Select query is calling selector with 3.
            // Select query evaluated and is yielding ***.
            // Select query is calling selector with 4.
            // Select query evaluated and is yielding ****.
            // Select query is calling selector with 5.
            // Select query evaluated and is yielding *****.
            // Select query ends.
            foreach (string result in immediateQuery)
            {
            }
        }

        internal static IEnumerable<double> AbsAndSqrtGenerator(double @double)
        {
            yield return Math.Abs(@double);
            yield return Math.Sqrt(@double);
        }

        internal static IEnumerable<double> AbsAndSqrtArray(double @double) =>
            new double[] {Math.Abs(@double), Math.Sqrt(@double)};

        internal static void Sequences(double @double)
        {
            IEnumerable<double> cold = AbsAndSqrtGenerator(@double); // Deferred execution.
            // Math.Abs and Math.Sqrt are not executed.
            foreach (double result in cold)
            {
            } // Math.Abs and Math.Sqrt are executed.

            IEnumerable<double> hot = AbsAndSqrtArray(@double); // Immediate execution.
            // Math.Abs and Math.Sqrt are executed.
        }

        internal static void Tasks()
        {
            Task cold = new Task(() => Trace.WriteLine(nameof(cold))); // Deferred execution.
            // Created task is not started.
            cold.Start(); // Created task is started.

            Task hot = Task.Run(() => Trace.WriteLine(nameof(hot))); // Immediate execution.
            // Created task is started.
        }

        internal static IEnumerable<TSource> WhereGenerator<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Trace.WriteLine("Where query starts.");
            foreach (TSource value in source)
            {
                Trace.WriteLine($"Where query is calling predicate with {value}.");
                if (predicate(value))
                {
                    Trace.WriteLine($"Where query is yielding {value}.");
                    yield return value;
                }
            }
            Trace.WriteLine("Where query ends.");
        }

        internal static IEnumerable<TSource> CompiledWhereGenerator<TSource>(
                this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            new Generator<TSource, IEnumerator<TSource>>(
                data: null, // IEnumerator<TSource> sourceIterator = null;
                iteratorFactory: sourceIterator => new Iterator<TSource>(
                    start: () =>
                        {
                            Trace.WriteLine("Where query starts.");
                            sourceIterator = source.GetEnumerator();
                        },
                    moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                Trace.WriteLine($"Where query is calling predicate with {sourceIterator.Current}.");
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                    getCurrent: () =>
                        {
                            Trace.WriteLine($"Where query is yielding {sourceIterator.Current}.");
                            return sourceIterator.Current;
                        },
                    dispose: () => sourceIterator?.Dispose(),
                    end: () => Trace.WriteLine("Where query ends.")));

        internal static void ForEachWhereAndSelect()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5)
                .WhereGenerator(int32 => int32 > 3) // Deferred execution.
                .SelectGenerator(int32 => new string('*', int32)); // Deferred execution.
            foreach (string result in deferredQuery)
            {
                // Select query starts.
                // Where query starts.
                // Where query is calling predicate with 1.
                // Where query is calling predicate with 2.
                // Where query is calling predicate with 3.
                // Select query is calling selector with 3.
                // Where query is calling predicate with 4.
                // Where query is yielding 4.
                // Select query is calling selector with 4.
                // Select query evaluated and is yielding ****.
                // Where query is calling predicate with 5.
                // Where query is yielding 5.
                // Select query is calling selector with 5.
                // Select query evaluated and is yielding *****.
                // Where query ends.
                // Select query ends.
            }
        }

        internal static IEnumerable<TSource> ReverseGenerator<TSource>(this IEnumerable<TSource> source)
        {
            Trace.WriteLine("Reverse query starts.");
            TSource[] values = source.ToArray();
            Trace.WriteLine($"Reverse query evaluated all {values.Length} value(s) in input sequence.");
            for (int lastIndex = values.Length - 1; lastIndex >= 0; lastIndex--)
            {
                Trace.WriteLine($"Reverse query is yielding index {lastIndex} of input sequence.");
                yield return values[lastIndex];
            }
            Trace.WriteLine("Reverse query ends.");
        }

        internal static IEnumerable<TSource> CompiledReverseGenerator<TSource>(this IEnumerable<TSource> source) =>
            new Generator<TSource, Tuple<TSource[], int>>(
                data: null,
                iteratorFactory: data => new Iterator<TSource>(
                    start: () =>
                        {
                            Trace.WriteLine("Reverse query starts.");
                            TSource[] values = source.ToArray();
                            Trace.WriteLine($"Reverse query evaluated all {values.Length} value(s) in input sequence.");

                            data = Tuple.Create(values, values.Length - 1);
                        },
                    moveNext: () => data.Item2 >= 0,
                    getCurrent: () =>
                        {
                            TSource[] values = data.Item1;
                            int lastIndex = data.Item2;
                            data = Tuple.Create(values, lastIndex - 1);

                            Trace.WriteLine($"Reverse query is yielding index {lastIndex} of input sequence.");
                            return values[lastIndex];
                        },
                    end: () => Trace.WriteLine("Reverse query ends.")));

        internal static void ForEachSelectAndReverse()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5)
                .SelectGenerator(int32 => new string('*', int32)) // Deferred execution.
                .ReverseGenerator(); // Deferred execution.
            using (IEnumerator<string> reverseIterator = deferredQuery.GetEnumerator())
            {
                if (reverseIterator.MoveNext()) // Eager evaluation.
                {
                    // Reverse query starts.
                    // Select query starts.
                    // Select query is calling selector with 1.
                    // Select query evaluated and is yielding *.
                    // Select query is calling selector with 2.
                    // Select query evaluated and is yielding **.
                    // Select query is calling selector with 3.
                    // Select query evaluated and is yielding ***.
                    // Select query is calling selector with 4.
                    // Select query evaluated and is yielding ****.
                    // Select query is calling selector with 5.
                    // Select query evaluated and is yielding *****.
                    // Select query ends.
                    // Reverse query evaluated all 5 value(s) in input sequence.
                    // Reverse query is yielding index 4 of input sequence.
                    Trace.WriteLine(reverseIterator.Current); // *****
                    while (reverseIterator.MoveNext())
                    {
                        // Reverse query is yielding index 3 of input sequence.
                        // Reverse query is yielding index 2 of input sequence.
                        // Reverse query is yielding index 1 of input sequence.
                        // Reverse query is yielding index 0 of input sequence.
                        Trace.WriteLine(reverseIterator.Current); // 6 4 2 0
                    }
                    // Reverse query ends.
                }
            }
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> DeferredSelect<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> CompiledDeferredSelect<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                new Generator<TResult, IEnumerator<TSource>>(
                    data: null, // IEnumerator<TSource> sourceIterator = null;
                    iteratorFactory: sourceIterator => new Iterator<TResult>(
                        start: () =>
                        {
                            if (source == null)
                            {
                                throw new ArgumentNullException(nameof(source));
                            }
                            if (selector == null)
                            {
                                throw new ArgumentNullException(nameof(selector));
                            }
                            sourceIterator = source.GetEnumerator();
                        },
                        moveNext: () => sourceIterator.MoveNext(),
                        getCurrent: () => selector(sourceIterator.Current),
                        dispose: () => sourceIterator?.Dispose()));

#if DEMO
        internal static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return SelectGenerator(source, selector);
        }

        private static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }
#endif
    }
}

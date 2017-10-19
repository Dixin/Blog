namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            "Select query starts.".WriteLine();
            foreach (TSource value in source)
            {
                $"Select query is calling selector with {value}.".WriteLine();
                TResult result = selector(value);
                $"Select query is yielding {result}.".WriteLine();
                yield return result;
            }
            "Select query ends.".WriteLine();
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> DesugaredSelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            "Select query starts.".WriteLine();
            IEnumerator<TSource> sourceIterator = null; // start.
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    $"Select query is calling selector with {sourceIterator.Current}.".WriteLine(); // getCurrent.
                    TResult result = selector(sourceIterator.Current); // getCurrent.
                    $"Select query is yielding {result}.".WriteLine(); // getCurrent.
                    yield return result; // getCurrent.
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }
            "Select query ends.".WriteLine(); // end.
        }

        internal static IEnumerable<TResult> CompiledSelectGenerator<TSource, TResult>(
                this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            new Generator<TResult, IEnumerator<TSource>>(
                data: null, // IEnumerator<TSource> sourceIterator = null;
                iteratorFactory: sourceIterator => new Iterator<TResult>(
                    start: () =>
                    {
                        "Select query starts.".WriteLine();
                        sourceIterator = source.GetEnumerator();
                    },
                    moveNext: () => sourceIterator.MoveNext(),
                    getCurrent: () =>
                    {
                        $"Select query is calling selector with {sourceIterator.Current}.".WriteLine();
                        TResult result = selector(sourceIterator.Current);
                        $"Select query is yielding {result}.".WriteLine();
                        return result;
                    },
                    dispose: () => sourceIterator?.Dispose(),
                    end: () => "Select query ends.".WriteLine()));

        internal static IEnumerable<TResult> SelectList<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            "Select query starts.".WriteLine();
            List<TResult> resultSequence = new List<TResult>();
            foreach (TSource value in source)
            {
                $"Select query is calling selector with {value}.".WriteLine();
                TResult result = selector(value);
                $"Select query is storing {result}.".WriteLine();
                resultSequence.Add(result);
            }

            "Select query ends.".WriteLine();
            return resultSequence;
        }

        internal static void ForEachSelect()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5)
                .SelectGenerator(int32 => new string('*', int32));
            foreach (string result in deferredQuery) // Execute query.
            {
                // Select query starts.
                // Select query is calling selector with 1.
                // Select query is yielding *.
                // Select query is calling selector with 2.
                // Select query is yielding **.
                // Select query is calling selector with 3.
                // Select query is yielding ***.
                // Select query is calling selector with 4.
                // Select query is yielding ****.
                // Select query is calling selector with 5.
                // Select query is yielding *****.
                // Select query ends.
            }

            IEnumerable<string> immediateQuery = Enumerable.Range(1, 5)
                .SelectList(int32 => new string('*', int32)); // Execute query.
            // Select query starts.
            // Select query is calling selector with 1.
            // Select query is storing *.
            // Select query is calling selector with 2.
            // Select query is storing **.
            // Select query is calling selector with 3.
            // Select query is storing ***.
            // Select query is calling selector with 4.
            // Select query is storing ****.
            // Select query is calling selector with 5.
            // Select query is storing *****.
            // Select query ends.
            foreach (string result in immediateQuery) { }
        }

        internal static IEnumerable<double> AbsAndSqrtGenerator(double @double)
        {
            yield return Math.Abs(@double);
            yield return Math.Sqrt(@double);
        }

        internal static IEnumerable<double> AbsAndSqrtArray(double @double) => new double[]
        {
            Math.Abs(@double),
            Math.Sqrt(@double)
        };

        internal static void Sequences(double @double)
        {
            IEnumerable<double> cold = AbsAndSqrtGenerator(@double); // Deferred execution.
            // Math.Abs and Math.Sqrt are not executed.
            foreach (double result in cold) { }
            // Math.Abs and Math.Sqrt are executed.

            IEnumerable<double> hot = AbsAndSqrtArray(@double); // Immediate execution.
            // Math.Abs and Math.Sqrt are executed.
        }

        internal static void Tasks()
        {
            Task cold = new Task(() => nameof(cold).WriteLine()); // Deferred execution.
            // Created task is not started.
            cold.Start(); // Created task is started.

            Task hot = Task.Run(() => nameof(hot).WriteLine()); // Immediate execution.
            // Created task is started.
        }

        internal static IEnumerable<TSource> WhereGenerator<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            "Where query starts.".WriteLine();
            foreach (TSource value in source)
            {
                $"Where query is calling predicate with {value}.".WriteLine();
                if (predicate(value))
                {
                    $"Where query is yielding {value}.".WriteLine();
                    yield return value;
                }
            }
            "Where query ends.".WriteLine();
        }

        internal static IEnumerable<TSource> CompiledWhereGenerator<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                new Generator<TSource, IEnumerator<TSource>>(
                    data: null, // IEnumerator<TSource> sourceIterator = null;
                    iteratorFactory: sourceIterator => new Iterator<TSource>(
                        start: () =>
                        {
                            "Where query starts.".WriteLine();
                            sourceIterator = source.GetEnumerator();
                        },
                        moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                $"Where query is calling predicate with {sourceIterator.Current}.".WriteLine();
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }
                            return false;
                        },
                        getCurrent: () =>
                        {
                            $"Where query is yielding {sourceIterator.Current}.".WriteLine();
                            return sourceIterator.Current;
                        },
                        dispose: () => sourceIterator?.Dispose(),
                        end: () => "Where query ends.".WriteLine()));

        internal static void ForEachWhereAndSelect()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5)
                .WhereGenerator(int32 => int32 > 2) // Deferred execution.
                .SelectGenerator(int32 => new string('*', int32)); // Deferred execution.
            foreach (string result in deferredQuery)
            {
                // Select query starts.
                // Where query starts.
                // Where query is calling predicate with 1.
                // Where query is calling predicate with 2.
                // Where query is calling predicate with 3.
                // Where query is yielding 3.
                // Select query is calling selector with 3.
                // Select query is yielding ***.
                // Where query is calling predicate with 4.
                // Where query is yielding 4.
                // Select query is calling selector with 4.
                // Select query is yielding ****.
                // Where query is calling predicate with 5.
                // Where query is yielding 5.
                // Select query is calling selector with 5.
                // Select query is yielding *****.
                // Where query ends.
                // Select query ends.
            }
        }

        internal static IEnumerable<TSource> ReverseGenerator<TSource>(this IEnumerable<TSource> source)
        {
            "Reverse query starts.".WriteLine();
            TSource[] values = source.ToArray();
            $"Reverse query evaluated all {values.Length} value(s) in input sequence.".WriteLine();
            for (int index = values.Length - 1; index >= 0; index--)
            {
                $"Reverse query is yielding index {index} of input sequence.".WriteLine();
                yield return values[index];
            }
            "Reverse query ends.".WriteLine();
        }

        internal static IEnumerable<TSource> CompiledReverseGenerator<TSource>(this IEnumerable<TSource> source) =>
            new Generator<TSource, (TSource[] Values, int Index)>(
                data: default, // (TSource[] Values, int Index) data = default;
                iteratorFactory: data => new Iterator<TSource>(
                    start: () =>
                    {
                        "Reverse query starts.".WriteLine();
                        TSource[] values = source.ToArray();
                        $"Reverse query evaluated all {values.Length} value(s) in input sequence.".WriteLine();
                        data = (values, values.Length);
                    },
                    moveNext: () =>
                    {
                        data = (data.Values, data.Index - 1);
                        return data.Index >= 0;
                    },
                    getCurrent: () =>
                    {
                        $"Reverse query is yielding index {data.Index} of input sequence.".WriteLine();
                        return data.Values[data.Index];
                    },
                    end: () => "Reverse query ends.".WriteLine()));

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
                    // Select query is yielding *.
                    // Select query is calling selector with 2.
                    // Select query is yielding **.
                    // Select query is calling selector with 3.
                    // Select query is yielding ***.
                    // Select query is calling selector with 4.
                    // Select query is yielding ****.
                    // Select query is calling selector with 5.
                    // Select query is yielding *****.
                    // Select query ends.
                    // Reverse query evaluated all 5 value(s) in input sequence.
                    // Reverse query is yielding index 4 of source sequence.
                    reverseIterator.Current.WriteLine();
                    while (reverseIterator.MoveNext())
                    {
                        // Reverse query is yielding index 3 of source sequence.
                        // Reverse query is yielding index 2 of source sequence.
                        // Reverse query is yielding index 1 of source sequence.
                        // Reverse query is yielding index 0 of source sequence.
                        reverseIterator.Current.WriteLine();
                    } // Reverse query ends.
                }
            }
        }
    }

    internal static partial class DeferredExecution
    {
        internal static IEnumerable<TResult> DeferredSelect<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null) // Deferred execution.
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null) // Deferred execution.
            {
                throw new ArgumentNullException(nameof(selector));
            }

            foreach (TSource value in source)
            {
                yield return selector(value); // Deferred execution.
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
            if (source == null) // Immediate execution.
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null) // Immediate execution.
            {
                throw new ArgumentNullException(nameof(selector));
            }

            IEnumerable<TResult> SelectGenerator()
            {
                foreach (TSource value in source)
                {
                    yield return selector(value); // Deferred execution.
                }
            }
            return SelectGenerator();
        }
#endif
    }
}

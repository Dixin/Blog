namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal static partial class QueryMethods
    {
        #region Generation

        internal static void Defer()
        {
            Func<IEnumerable<int>> sequenceFactory = () =>
            {
                "Executing factory.".WriteLine();
                return Enumerable.Empty<int>();
            };
            IEnumerable<int> sequence1 = sequenceFactory() // Executing factory.
                .Where(int32 => int32 > 0);
            IEnumerable<int> sequence2 = EnumerableEx.Defer(sequenceFactory)
                .Where(int32 => int32 > 0);
        }

#if DEMO
        // Cannot be compiled.
        internal static void Create()
        {
            Func<IEnumerable<int>> sequenceFactory = () =>
            {
                yield return 0;
                yield return 1;
                yield break;
                yield return 2;
            };
            IEnumerable<int> sequence = sequenceFactory();
            sequence.WriteLine(); // 0 1
        }
#endif

        internal static void Create()
        {
            Action<IYielder<int>> sequenceFactory = async yield =>
            {
                await yield.Return(0); // yield return 0;
                await yield.Return(1); // yield return 1;
                await yield.Break(); // yield break;
                await yield.Return(2); // yield return 2;
            };
            IEnumerable<int> sequence = EnumerableEx.Create(sequenceFactory);
            sequence.WriteLine(); // 0 1
        }

#if DEMO
        internal static void Create()
        {
            IEnumerable<int> SequenceFactory()
            {
                yield return 0;
                yield return 1;
                yield break;
                yield return 2;
            }
            IEnumerable<int> sequence = SequenceFactory();
            sequence.WriteLine(); // 0 1
        }

        internal static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            IEnumerable<TResult> CastGenerator()
            {
                foreach (object value in source)
                {
                    yield return (TResult)value; // Deferred execution.
                }
            }
            return source is IEnumerable<TResult> genericSource
                ? genericSource
                : CastGenerator(); // Deferred execution.
        }
#endif

        internal static IEnumerable<TResult> CastWithCreate<TResult>(this IEnumerable source) =>
            source is IEnumerable<TResult> genericSource
                ? genericSource
                : EnumerableEx.Create<TResult>(async yield =>
                    {
                        foreach (object value in source)
                        {
                            await yield.Return((TResult)value); // yield return (TResult)value;
                        }
                    });

        internal static IEnumerable<TResult> CreateWithCreate<TResult>(Func<IEnumerator<TResult>> getEnumerator) =>
            EnumerableEx.Create<TResult>(async yield =>
            {
                using (IEnumerator<TResult> iterator = getEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        await yield.Return(iterator.Current); // yield return iterator.Current;
                    }
                }
            });

        #endregion

        #region Filtering

        internal static void DistinctUntilChanged()
        {
            IEnumerable<int> source = new int[]
            {
                0, 0, 0, /* Changed. */ 1, 1, /* Changed. */ 0, 0, /* Changed. */ 2, /* Changed. */ 1, 1
            };
            source.DistinctUntilChanged().WriteLines(); // 0 1 0 2 1.
        }

        #endregion

        #region Mapping

        internal static void Scan()
        {
            int finalProduct = Int32Source().Aggregate((product, int32) => product * int32).WriteLine();
            // ((((-1 * 1) * 2) * 3) * -4) => 24.

            IEnumerable<int> allProducts = Int32Source().Scan((product, int32) => product * int32).WriteLines();
            // ((((-1 * 1) * 2) * 3) * -4) => { -1, -2, -6, 24 }.
        }

        internal static void ExpandSingle()
        {
            Enumerable
                .Range(0, 5)
                .Expand(int32 => EnumerableEx.Return(int32 * int32))
                .Take(25)
                .WriteLines();
            // 0 1 2 3 4, map each int32 to { int32 * int32 } =>
            // 0 1 4 9 16, map each int32 to { int32 * int32 }: =>
            // 0 1 16 81 256, map each int32 to { int32 * int32 } =>
            // 0 1 256 6561 65536, map each int32 to { int32 * int32 } =>
            // 0 1 65536 43046721 4294967296, ...
        }

        internal static void ExpandMuliple()
        {
            Enumerable
                .Range(0, 5)
                .Expand(int32 => Enumerable.Repeat(int32, 2))
                .Take(75)
                .WriteLines();
            // 0 1 2 3 4 => map each int32 to { int32, int32 }:
            // 0 0 1 1 2 2 3 3 4 4 => map each int32 to { int32, int32 }:
            // 0 0 0 0 1 1 1 1 2 2 2 2 3 3 3 3 4 4 4 4 => map each int32 to { int32, int32 }:
            // 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 3 3 3 3 3 3 3 3 4 4 4 4 4 4 4 4 => ...
        }

        internal static void ExpandNone()
        {
            Enumerable
                .Range(0, 5)
                .Expand(int32 => Enumerable.Empty<int>())
                .Take(100)
                .WriteLines();
            // 0 1 2 3 4 => map each int32 to { }.
        }

        #endregion

        #region Partioning

        internal static void SkipLastTakeLast()
        {
            int[] skipFirst2 = Enumerable.Range(0, 5).Skip(2).ToArray(); // 2 3 4.
            int[] skipLast2 = Enumerable.Range(0, 5).SkipLast(2).ToArray(); // 0 1 2.
            int[] takeFirst2 = Enumerable.Range(0, 5).Take(2).ToArray(); // 0 1.
            int[] takeLast2 = Enumerable.Range(0, 5).TakeLast(2).ToArray(); // 3 4.
        }

        #endregion

        #region Conversion

        internal static void Hide()
        {
            List<int> source = new List<int>() { 1, 2 };
            IEnumerable<int> readWrite = source.AsEnumerable();
            object.ReferenceEquals(source, readWrite).WriteLine(); // True
            ((List<int>)readWrite).Reverse(); // List<T>.Reverse.
            ((List<int>)readWrite).Add(3); // List<T>.Add.

            IEnumerable<int> readOnly = source.Hide();
            object.ReferenceEquals(source, readOnly).WriteLine(); // False
        }

        #endregion

        #region Buffering

        internal static void Buffer()
        {
            IEnumerable<IList<int>> buffers1 = Enumerable.Range(0, 5).Buffer(2, 1);
            // {
            //    { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4 }   
            // }

            IEnumerable<IList<int>> buffers2 = Enumerable.Range(0, 5).Buffer(2, 2); // Equivalent to Buffer(2).
            // {
            //    { 0, 1 }, { 2, 3 }, { 4 }   
            // }

            IEnumerable<IList<int>> buffers3 = Enumerable.Range(0, 5).Buffer(2, 3);
            // {
            //    { 0, 1 }, { 3, 4 }
            // }
        }

        internal static void Share()
        {
            IEnumerable<int> sequence = Enumerable.Range(0, 5);
            IEnumerator<int> independentIteratorA = sequence.GetEnumerator();
            IEnumerator<int> independentIteratorB = sequence.GetEnumerator();          // A|B|C
            independentIteratorA.MoveNext(); independentIteratorA.Current.WriteLine(); // 0| |
            independentIteratorB.MoveNext(); independentIteratorB.Current.WriteLine(); //  |0|
            independentIteratorA.MoveNext(); independentIteratorA.Current.WriteLine(); // 1| |
            IEnumerator<int> independentIteratorC = sequence.GetEnumerator();          //  | |
            independentIteratorC.MoveNext(); independentIteratorC.Current.WriteLine(); //  | |0
            independentIteratorA.MoveNext(); independentIteratorA.Current.WriteLine(); // 2| |
            independentIteratorB.MoveNext(); independentIteratorB.Current.WriteLine(); //  |1|
            independentIteratorA.MoveNext(); independentIteratorA.Current.WriteLine(); // 3| |
            // ...

            IBuffer<int> share = Enumerable.Range(0, 5).Share();
            IEnumerator<int> sharedIterator1 = share.GetEnumerator();
            IEnumerator<int> sharedIterator2 = share.GetEnumerator();        // A|B|C
            sharedIterator1.MoveNext(); sharedIterator1.Current.WriteLine(); // 0| |
            sharedIterator2.MoveNext(); sharedIterator2.Current.WriteLine(); //  |1|
            sharedIterator1.MoveNext(); sharedIterator1.Current.WriteLine(); // 2| |
            IEnumerator<int> sharedIterator3 = share.GetEnumerator();        //  | |
            sharedIterator3.MoveNext(); sharedIterator3.Current.WriteLine(); //  | |3

            share.Dispose();
            sharedIterator1.MoveNext(); // ObjectDisposedException.
            sharedIterator2.MoveNext(); // ObjectDisposedException.
            sharedIterator3.MoveNext(); // ObjectDisposedException.
        }

        internal static void ConcatShared()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Concat(source1).WriteLines(); // 0 1 2 3 4 0 1 2 3 4

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Share())
            {
                source2.Concat(source2).WriteLines(); // 0 1 2 3 4
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Share(source => source.Concat(source)).WriteLines(); // 0 1 2 3 4
        }

        internal static IEnumerable<TSource> Concat<TSource>(
            IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            using (IEnumerator<TSource> iterator1 = first.GetEnumerator())
            {
                while (iterator1.MoveNext())
                {
                    yield return iterator1.Current;
                }
            }
            using (IEnumerator<TSource> iterator2 = second.GetEnumerator())
            {
                while (iterator2.MoveNext())
                {
                    yield return iterator2.Current;
                }
            }
        }

        internal static void DesugaredConcatShared()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            IEnumerable<int> Concat1() // source1.Concat(source1)
            {
                using (IEnumerator<int> independentIterator1 = source1.GetEnumerator())
                {
                    while (independentIterator1.MoveNext())
                    {
                        yield return independentIterator1.Current; // yield return 0 1 2 3 4.
                    }
                }
                using (IEnumerator<int> independentIterator2 = source1.GetEnumerator())
                {
                    while (independentIterator2.MoveNext())
                    {
                        yield return independentIterator2.Current; // yield return 0 1 2 3 4.
                    }
                }
            }
            Concat1().WriteLines();

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Share())
            {
                IEnumerable<int> Concat2() // source2.Concat(source2)
                {
                    using (IEnumerator<int> sharedIterator1 = source2.GetEnumerator())
                    {
                        while (sharedIterator1.MoveNext())
                        {
                            yield return sharedIterator1.Current; // yield return 0 1 2 3 4.
                        }
                    }
                    using (IEnumerator<int> sharedIterator2 = source2.GetEnumerator())
                    {
                        while (sharedIterator2.MoveNext())
                        {
                            yield return sharedIterator2.Current; // yield return nothing.
                        }
                    }
                }
                Concat2().WriteLines();
            }

            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            IEnumerable<int> Concat3() // source3.Share(source => source.Concat(source))
            {
                using (IBuffer<int> source = source3.Share())
                {
                    using (IEnumerator<int> sharedIterator1 = source.GetEnumerator())
                    {
                        while (sharedIterator1.MoveNext())
                        {
                            yield return sharedIterator1.Current; // yield return 0 1 2 3 4.
                        }
                    }
                    using (IEnumerator<int> sharedIterator2 = source.GetEnumerator())
                    {
                        while (sharedIterator2.MoveNext())
                        {
                            yield return sharedIterator2.Current; // yield return nothing.
                        }
                    }
                }
            }
            Concat3().WriteLines();
        }

        internal static void ZipShared()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Zip(source1, ValueTuple.Create).WriteLines(); // (0, 0) (1, 1) (2, 2) (3, 3) (4, 4)

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Share())
            {
                source2.Zip(source2, ValueTuple.Create).WriteLines(); // (0, 1) (2, 3)
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Share(source => source.Zip(source, ValueTuple.Create)).WriteLines(); // (0, 1) (2, 3).
        }

        internal static void DesugaredZipShared()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            IEnumerable<(int, int)> Zip1()
            {
                using (IEnumerator<int> independentIterator1 = source1.GetEnumerator())
                using (IEnumerator<int> independentIterator2 = source1.GetEnumerator())
                {
                    while (independentIterator1.MoveNext() && independentIterator2.MoveNext())
                    {
                        yield return (independentIterator1.Current, independentIterator2.Current);
                        // yield return (0, 0) (1, 1) (2, 2) (3, 3) (4, 4).
                    }
                }
            }
            Zip1().WriteLines();

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Share())
            {
                IEnumerable<(int, int)> Zip2()
                {
                    using (IEnumerator<int> sharedIterator1 = source2.GetEnumerator())
                    using (IEnumerator<int> sharedIterator2 = source2.GetEnumerator())
                    {
                        while (sharedIterator1.MoveNext() && sharedIterator2.MoveNext())
                        {
                            yield return (sharedIterator1.Current, sharedIterator2.Current);
                            // yield return (0, 1) (2, 3).
                        }
                    }
                }
                Zip2().WriteLines();
            }

            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            IEnumerable<(int, int)> Zip3()
            {
                using (IBuffer<int> source = source3.Share())
                using (IEnumerator<int> sharedIterator1 = source.GetEnumerator())
                using (IEnumerator<int> sharedIterator2 = source.GetEnumerator())
                {
                    while (sharedIterator1.MoveNext() && sharedIterator2.MoveNext())
                    {
                        yield return (sharedIterator1.Current, sharedIterator2.Current);
                        // yield return (0, 1) (2, 3).
                    }
                }
            }
            Zip3().WriteLines();
        }

        internal static void Publish()
        {
            using (IBuffer<int> publish = Enumerable.Range(0, 5).Publish())
            {
                IEnumerator<int> remainderIteratorA = publish.GetEnumerator();
                // remainderIteratorA: 0 1 2 3 4.                                         A|B|C
                remainderIteratorA.MoveNext(); remainderIteratorA.Current.WriteLine(); // 0| |
                remainderIteratorA.MoveNext(); remainderIteratorA.Current.WriteLine(); // 1| |
                remainderIteratorA.MoveNext(); remainderIteratorA.Current.WriteLine(); // 2| |
                IEnumerator<int> remainderIteratorB = publish.GetEnumerator();         //  | |
                // remainderIteratorB: 3 4.                                                | |
                remainderIteratorB.MoveNext(); remainderIteratorB.Current.WriteLine(); //  |3|
                remainderIteratorA.MoveNext(); remainderIteratorA.Current.WriteLine(); // 3| |
                IEnumerator<int> remainderIteratorC = publish.GetEnumerator();         //  | |
                // remainderIteratorC: 4.                                                  | |
                remainderIteratorB.MoveNext(); remainderIteratorB.Current.WriteLine(); //  |4|
                remainderIteratorA.MoveNext(); remainderIteratorA.Current.WriteLine(); // 4| |
                remainderIteratorC.MoveNext(); remainderIteratorC.Current.WriteLine(); //  | |4
            }
        }

        internal static void Publish2()
        {
            IBuffer<int> publish = Enumerable.Range(0, 5).Publish();
            IEnumerator<int> iterator1 = publish.GetEnumerator();
            // iterator1: 0 1 2 3 4
            IEnumerator<int> iterator2 = publish.GetEnumerator();
            // iterator1: 0 1 2 3 4
            iterator1.MoveNext(); iterator1.Current.WriteLine(); // 0| |Trace: 0
            iterator1.MoveNext(); iterator1.Current.WriteLine(); // 1| |Trace: 1
            iterator1.MoveNext(); iterator1.Current.WriteLine(); // 2| |Trace: 2
            iterator2.MoveNext(); iterator2.Current.WriteLine(); //  |0|
            iterator1.MoveNext(); iterator1.Current.WriteLine(); // 3| |Trace: 3
            iterator2.MoveNext(); iterator2.Current.WriteLine(); //  |1|
            iterator1.MoveNext(); iterator1.Current.WriteLine(); // 4| |Trace: 4

            publish.Dispose();
            iterator1.MoveNext(); // ObjectDisposedException.
            iterator2.MoveNext(); // ObjectDisposedException.
        }

        internal static void ConcatPublished()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Concat(source1).WriteLines(); // 0 1 2 3 4 0 1 2 3 4

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Publish())
            {
                source2.Concat(source2).WriteLines(); // 0 1 2 3 4
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Publish(source => source.Concat(source)).WriteLines(); // 0 1 2 3 4
        }

        internal static void ZipPublished()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Zip(source1, ValueTuple.Create).WriteLines();
            // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Publish())
            {
                source2.Zip(source2, ValueTuple.Create).WriteLines();
                // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Publish(source => source.Zip(source, ValueTuple.Create)).WriteLines();
            // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)
        }

        internal static void Memoize()
        {
            using (IBuffer<int> memoize = Enumerable.Range(0, 5).Memoize())
            {
                IEnumerator<int> bufferIteratorA = memoize.GetEnumerator();
                // bufferIteratorA: 0 1 2 3 4.                                      A|B|C
                bufferIteratorA.MoveNext(); bufferIteratorA.Current.WriteLine(); // 0| |
                bufferIteratorA.MoveNext(); bufferIteratorA.Current.WriteLine(); // 1| |
                bufferIteratorA.MoveNext(); bufferIteratorA.Current.WriteLine(); // 2| |
                IEnumerator<int> bufferIteratorB = memoize.GetEnumerator();      //  | |
                // bufferIteratorB: 0 1 2 3 4.                                       | |
                bufferIteratorB.MoveNext(); bufferIteratorB.Current.WriteLine(); //  |0|
                bufferIteratorA.MoveNext(); bufferIteratorA.Current.WriteLine(); // 3| |
                IEnumerator<int> bufferIteratorC = memoize.GetEnumerator();      //  | |
                // bufferIteratorC: 0 1 2 3 4.                                       | |
                bufferIteratorB.MoveNext(); bufferIteratorB.Current.WriteLine(); //  |1|
                bufferIteratorA.MoveNext(); bufferIteratorA.Current.WriteLine(); // 4| |
                bufferIteratorC.MoveNext(); bufferIteratorC.Current.WriteLine(); //  | |0
                bufferIteratorC.MoveNext(); bufferIteratorC.Current.WriteLine(); //  | |1
                bufferIteratorB.MoveNext(); bufferIteratorB.Current.WriteLine(); //  |2|
                // ...
            }
        }

        internal static void ConcatMemoized()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Concat(source1).WriteLines(); // 0 1 2 3 4 0 1 2 3 4

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Memoize())
            {
                source2.Concat(source2).WriteLines(); // 0 1 2 3 4 0 1 2 3 4
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Memoize(source => source.Concat(source)).WriteLines(); // 0 1 2 3 4 0 1 2 3 4
        }

        internal static void ZipMemoized()
        {
            IEnumerable<int> source1 = Enumerable.Range(0, 5);
            source1.Zip(source1, ValueTuple.Create).WriteLines();
            // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)

            using (IBuffer<int> source2 = Enumerable.Range(0, 5).Memoize())
            {
                source2.Zip(source2, ValueTuple.Create).WriteLines();
                // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)
            }
            // Equivalent to:
            IEnumerable<int> source3 = Enumerable.Range(0, 5);
            source3.Memoize(source => source.Zip(source, ValueTuple.Create)).WriteLines();
            // (0, 0), (1, 1), (2, 2), (3, 3), (4, 4)
        }

        internal static void MemoizeWithReaderCount()
        {
            using (IBuffer<int> source1 = Enumerable.Range(0, 5).Memoize(2))
            {
                int[] reader1 = source1.ToArray(); // First full iteration.
                int[] reader2 = source1.ToArray(); // Second full iteration.
                int[] reader3 = source1.ToArray(); // Third full iteration: InvalidOperationException.
            }

            IEnumerable<int> source2 = Enumerable.Range(0, 5);
            source2
                .Memoize(
                    readerCount: 2,
                    selector: source => source // First full iteration.
                        .Concat(source) // Second full iteration.
                        .Concat(source)) // Third full iteration: InvalidOperationException.
                .WriteLines();
        }

        #endregion

        #region Exception

        internal static void Throw()
        {
            IEnumerable<int> @throw = EnumerableEx.Throw<int>(new OperationCanceledException());
            IEnumerable<int> query = Enumerable.Range(0, 5).Concat(@throw); // Define query.
            try
            {
                foreach (int value in query) // Execute query.
                {
                    value.WriteLine();
                }
            }
            catch (OperationCanceledException exception)
            {
                exception.WriteLine();
            }
            // 0 1 2 3 4 System.OperationCanceledException: The operation was canceled.
        }

        internal static void CatchWithHandler()
        {
            IEnumerable<string> @throw = EnumerableEx.Throw<string>(new OperationCanceledException());
            IEnumerable<string> @catch = @throw.Catch<string, OperationCanceledException>(
                exception => EnumerableEx.Return($"Handled {exception.GetType().Name}: {exception.Message}"));
            @catch.WriteLines(); // Handled OperationCanceledException: The operation was canceled.
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void Catch()
        {
            IEnumerable<int> scanWithException = Enumerable.Repeat(0, 5).Scan((a, b) => a / b); // Divide by 0.
            IEnumerable<int> range = Enumerable.Range(0, 5);
            IEnumerable<int> castWithException = new object[] { 5, "a" }.Cast<int>();

            IEnumerable<IEnumerable<int>> source1 = new IEnumerable<int>[]
            {
                scanWithException, // Executed, with DivideByZeroException.
                range, // Executed, without exception.
                castWithException // Not executed.
            };
            source1.Catch().WriteLines(); // 0 1 2 3 4

            IEnumerable<IEnumerable<int>> source2 = new IEnumerable<int>[]
            {
                scanWithException,  // Executed, with DivideByZeroException.
                castWithException // Executed, with InvalidCastException.
            };
            try
            {
                source2.Catch().WriteLines(); // 5 
            }
            catch (InvalidCastException exception)
            {
                exception.WriteLine(); // System.InvalidCastException: Specified cast is not valid.
            }
        }

        #endregion

        #region Iteration

        internal static void Do()
        {
            Enumerable
                .Range(-5, 10).Do(
                    onNext: value => $"{nameof(Enumerable.Range)} yields {value}.".WriteLine(),
                    onCompleted: () => $"{nameof(Enumerable.Range)} query completes.".WriteLine())
                .Where(value => value > 0).Do(
                    onNext: value => $"{nameof(Enumerable.Where)} yields {value}.".WriteLine(),
                    onCompleted: () => $"{nameof(Enumerable.Where)} query completes.".WriteLine())
                .TakeLast(2).Do(
                    onNext: value => $"{nameof(EnumerableEx.TakeLast)} yields {value}.".WriteLine(),
                    onCompleted: () => $"{nameof(EnumerableEx.TakeLast)} query completes.".WriteLine())
                .WriteLines(value => $"Query yields result {value}.");
            // Range yields -5.
            // Range yields -4.
            // Range yields -3.
            // Range yields -2.
            // Range yields -1.
            // Range yields 0.
            // Range yields 1.
            // Where yields 1.
            // Range yields 2.
            // Where yields 2.
            // Range yields 3.
            // Where yields 3.
            // Range yields 4.
            // Where yields 4.
            // Range query completes.
            // Where query completes.
            // TakeLast yields 3.
            // Query yields result 3.
            // TakeLast yields 4.
            // Query yields result 4.
            // TakeLast query completes.
        }

        #endregion

        #region Aggregation

        internal static void MaxMin()
        {
            Character maxCharacter = Characters()
                .Max(Comparer<Character>.Create((character1, character2) => 
                    string.Compare(character1.Name, character2.Name, StringComparison.OrdinalIgnoreCase)));
            Character minCharacter = Characters()
                .Max(Comparer<Character>.Create((character1, character2) =>
                    string.Compare(character1.Name, character2.Name, StringComparison.OrdinalIgnoreCase)));
        }

        internal static void MaxByMinBy()
        {
            IList<Character> maxCharacters = Characters()
                .MinBy(character => character.Name, StringComparer.OrdinalIgnoreCase);
            IList<Character> minCharacters = Characters()
                .MaxBy(character => character.Name, StringComparer.OrdinalIgnoreCase);
        }

        internal static void MaxBy()
        {
            CoreLibrary.GetExportedTypes()
                .Select(type => (Type: type, MemberCount: type.GetDeclaredMembers().Length))
                .MaxBy(typeAndMemberCount => typeAndMemberCount.MemberCount)
                .WriteLines(max => $"{max.Type.FullName}:{max.MemberCount}"); // System.Convert:311
        }

        #endregion
    }
}

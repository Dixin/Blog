namespace Dixin.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class Sequence<T, TData> : IEnumerable<T>
    {
        private readonly int initialThreadId;

        private readonly TData data;

        private readonly Func<TData, Iterator<T>> createEnumerator;

        private Iterator<T> initialThreadIterator;

        public Sequence(TData data, Func<TData, Iterator<T>> createEnumerator)
        {
            this.data = data;
            this.createEnumerator = createEnumerator;
            this.initialThreadId = Environment.CurrentManagedThreadId;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                if (this.initialThreadIterator == null)
                {
                    this.initialThreadIterator = this.createEnumerator(this.data);
                }

                if (this.initialThreadIterator.State == IteratorState.Create)
                {
                    return this.initialThreadIterator.StartState();
                }
            }

            return this.createEnumerator(this.data).StartState();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public enum IteratorState
    {
        Create = -2,

        Start = 0,

        Next = 1,

        End = -1,

        Error = -3
    }

    public sealed class Iterator<T> : IEnumerator<T>
    {
        private readonly Action start;

        private readonly Func<bool> hasNext;

        private readonly Func<T> next;

        private readonly Action dispose;

        private readonly Action end;

        public Iterator(
            Action start = null, Func<bool> hasNext = null, Func<T> next = null, Action dispose = null, Action end = null)
        {
            this.start = start ?? (() => { });
            this.hasNext = hasNext ?? (() => false);
            this.next = next ?? (() => default(T));
            this.dispose = dispose ?? (() => { });
            this.end = end ?? (() => { });
            this.State = IteratorState.Create;
        }

        public IteratorState State { get; private set; }

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public Iterator<T> StartState()
        {
            this.State = IteratorState.Start;
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                switch (this.State)
                {
                    case IteratorState.Start:
                        this.start();
                        this.State = IteratorState.Next;
                        goto case IteratorState.Next;
                    case IteratorState.Next:
                        if (this.hasNext())
                        {
                            this.Current = this.next();
                            return true;
                        }

                        this.State = IteratorState.End;
                        this.dispose();
                        this.end();
                        return false;
                    default:
                        return false;
                }
            }
            catch
            {
                this.State = IteratorState.Error;
                this.Dispose();
                throw;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (this.State == IteratorState.Error || this.State == IteratorState.Next)
            {
                try
                {
                }
                finally
                {
                    // https://msdn.microsoft.com/en-us/library/ty8d3wta.aspx
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.State = IteratorState.End;
                    this.dispose();
                }
            }
        }
    }
}

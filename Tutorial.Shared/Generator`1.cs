namespace Tutorial
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public enum IteratorState
    {
        Create = -2,
        Start = 0,
        MoveNext = 1,
        End = -1,
        Error = -3
    }

#if DEMO
    public class Iterator<T> : IEnumerator<T>
    {
        private readonly Action start;

        private readonly Func<bool> moveNext;

        private readonly Func<T> getCurrent;

        private readonly Action dispose;

        private readonly Action end;

        public Iterator(
            Action start = null,
            Func<bool> moveNext = null,
            Func<T> getCurrent = null,
            Action dispose = null,
            Action end = null)
        {
            this.start = start;
            this.moveNext = moveNext;
            this.getCurrent = getCurrent;
            this.dispose = dispose;
            this.end = end;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        internal IteratorState State { get; private set; } = IteratorState.Create; // IteratorState: Create.

        internal Iterator<T> Start()
        {
            this.State = IteratorState.Start;  // IteratorState: Create => Start.
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                switch (this.State)
                {
                    case IteratorState.Start:
                        this.start?.Invoke();
                        this.State = IteratorState.MoveNext; // IteratorState: Start => MoveNext.
                        goto case IteratorState.MoveNext;
                    case IteratorState.MoveNext:
                        if (this.moveNext?.Invoke() ?? false)
                        {
                            this.Current = this.getCurrent != null ? this.getCurrent() : default(T);
                            return true; // IteratorState: MoveNext => MoveNext.
                        }
                        this.State = IteratorState.End; // IteratorState: MoveNext => End.
                        this.dispose?.Invoke();
                        this.end?.Invoke();
                        break;
                }
                return false;
            }
            catch
            {
                this.State = IteratorState.Error; // IteratorState: Start, MoveNext, End => Error.
                this.Dispose();
                throw;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (this.State == IteratorState.Error || this.State == IteratorState.MoveNext)
            {
                try
                {
                }
                finally
                {
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.State = IteratorState.End; // IteratorState: Error => End.
                    this.dispose?.Invoke();
                }
            }
        }

        public void Reset() => throw new NotSupportedException();
    }
#endif

    public class Iterator<T> : IEnumerator<T>
    {
        private readonly Action start;

        private readonly Func<bool> moveNext;

        private readonly Func<T> getCurrent;

        private readonly Action dispose;

        private readonly Action end;

        private readonly bool ignoreException;

        private readonly Action resetCurrent;

        public Iterator(
            Action start = null,
            Func<bool> moveNext = null,
            Func<T> getCurrent = null,
            Action dispose = null,
            Action end = null,
            bool ignoreException = false,
            bool resetCurrent = false)
        {
            this.start = start;
            this.moveNext = moveNext;
            this.getCurrent = getCurrent;
            this.dispose = () =>
            {
                dispose?.Invoke();
                if (resetCurrent)
                {
                    this.Current = default(T);
                }
            };
            this.end = end;
            this.ignoreException = ignoreException;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        internal IteratorState State { get; private set; } = IteratorState.Create; // IteratorState: Create.

        internal Iterator<T> Start()
        {
            this.State = IteratorState.Start;  // IteratorState: Create => Start.
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                switch (this.State)
                {
                    case IteratorState.Start:
                        this.start?.Invoke();
                        this.State = IteratorState.MoveNext; // IteratorState: Start => MoveNext.
                        goto case IteratorState.MoveNext;
                    case IteratorState.MoveNext:
                        if (this.moveNext?.Invoke() ?? false)
                        {
                            this.Current = this.getCurrent != null ? this.getCurrent() : default(T);
                            return true; // IteratorState: MoveNext => MoveNext.
                        }
                        this.State = IteratorState.End; // IteratorState: MoveNext => End.
                        this.dispose?.Invoke();
                        this.end?.Invoke();
                        break;
                }
                return false;
            }
            catch when (!this.ignoreException)
            {
                this.State = IteratorState.Error; // IteratorState: Start, MoveNext, End => Error.
                this.Dispose();
                throw;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (this.State == IteratorState.Error || this.State == IteratorState.MoveNext)
            {
                try
                {
                }
                finally
                {
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.State = IteratorState.End; // IteratorState: Error => End.
                    this.dispose?.Invoke();
                }
            }
        }

        public void Reset() => throw new NotSupportedException();
    }

    public class Sequence<T, TData> : IEnumerable<T>
    {
        private readonly TData data;

        private readonly Func<TData, Iterator<T>> iteratorFactory;

        public Sequence(Func<TData, Iterator<T>> iteratorFactory, TData data = default(TData))
        {
            this.data = data;
            this.iteratorFactory = iteratorFactory;
        }

        public IEnumerator<T> GetEnumerator() =>
            this.iteratorFactory(this.data).Start(); // IteratorState: Create => Start.

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public interface IGenerator<out T> : IEnumerable<T>, IEnumerator<T>
    {
    }

    public class Generator<T, TData> : IGenerator<T>
    {
        private readonly int initialThreadId = Environment.CurrentManagedThreadId;

        private readonly TData data;

        private readonly Func<TData, Iterator<T>> iteratorFactory;

        private Iterator<T> iterator;

        public Generator(Func<TData, Iterator<T>> iteratorFactory, TData data = default(TData))
        {
            this.iteratorFactory = iteratorFactory;
            this.data = data;
            this.iterator = iteratorFactory(data);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.iterator.State == IteratorState.Create
                && this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                // Where called by the same thread and iteration is not started, reuse the same iterator.
                this.iterator.Start();
                return this;
            }
            // If the iteration is already started, or the iteration is requested from a different thread, return a new iterator.
            Generator<T, TData> generator = new Generator<T, TData>(this.iteratorFactory, this.data);
            generator.iterator.Start();
            return generator;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Dispose() => this.iterator.Dispose();

        public bool MoveNext() => this.iterator.MoveNext();

        public void Reset() => this.iterator.Reset();

        public T Current => this.iterator.Current;

        object IEnumerator.Current => this.Current;
    }

#if DEMO
    public partial class Generator<T, TData> : IGenerator<T>
    {
        private readonly Func<TData, TData> start;

        private readonly Func<TData, (bool, TData)> moveNext;

        private readonly Func<TData, T> getCurrent;

        private readonly Action<TData> dispose;

        private readonly Action<TData> end;

        private TData data;

        public Generator(
            TData data = default(TData),
            Func<TData, TData> start = null,
            Func<TData, (bool, TData)> moveNext = null,
            Func<TData, T> getCurrent = null,
            Action<TData> dispose = null,
            Action<TData> end = null)
        {
            this.data = data;
            this.start = start ?? (currentData => currentData);
            this.moveNext = moveNext ?? (currentData => (false, currentData));
            this.getCurrent = getCurrent ?? (currentData => default(T));
            this.dispose = dispose ?? (currentData => { });
            this.end = end ?? (currentData => { });
        }
    }

    public partial class Generator<T, TData> // : IEnumerator<T>
    {
        private IteratorState state = IteratorState.Create;

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public bool MoveNext()
        {
            try
            {
                switch (this.state)
                {
                    case IteratorState.Start:
                        this.data = this.start(this.data);
                        this.state = IteratorState.MoveNext; // IteratorState: Start => MoveNext.
                        goto case IteratorState.MoveNext;
                    case IteratorState.MoveNext:
                        (bool, TData) result = this.moveNext(this.data);
                        this.data = result.Item2;
                        if (result.Item1)
                        {
                            this.Current = this.getCurrent(this.data);
                            return true; // IteratorState: MoveNext => MoveNext.
                        }
                        this.state = IteratorState.End; // IteratorState: MoveNext => End.
                        this.dispose(this.data);
                        this.end(this.data);
                        return false;
                    default:
                        return false;
                }
            }
            catch // IteratorState: Start, MoveNext, End => End.
            {
                this.state = IteratorState.Error;
                this.Dispose();
                throw;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (this.state == IteratorState.Error || this.state == IteratorState.MoveNext)
            {
                try
                {
                }
                finally
                {
                    // https://msdn.microsoft.com/en-us/library/ty8d3wta.aspx
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.state = IteratorState.End; // IteratorState: Error => End.
                    this.dispose(this.data);
                }
            }
        }

        public void Reset() => throw new NotSupportedException();
    }

    public partial class Generator<T, TData> // : IEnumerable<T>
    {
        private readonly int initialThreadId = Environment.CurrentManagedThreadId;

        public IEnumerator<T> GetEnumerator()
        {
            Generator<T, TData> iterator;
            if (this.state == IteratorState.Create && this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                // Where called by the same thread and iteration is not started, reuse the same iterator.
                iterator = this;
            }
            else
            {
                // If the iteration is already started, or the iteration is requested from a different thread, return a new iterator.
                iterator = new Generator<T, TData>(
                    this.data, this.start, this.moveNext, this.getCurrent, this.dispose, this.end);
            }
            iterator.state = IteratorState.Start; // IteratorState: Create => Start.
            return iterator;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public static class Generator
    {
        public static Generator<T, TData> Create<T, TData>(
            TData data = default(TData),
            Func<TData, TData> start = null,
            Func<TData, (bool, TData)> moveNext = null,
            Func<TData, T> getCurrent = null,
            Action<TData> dispose = null,
            Action<TData> end = null) =>
                new Generator<T, TData>(data, start, moveNext, getCurrent, dispose, end);
    }
#endif
}

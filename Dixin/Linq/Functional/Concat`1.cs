namespace Dixin.Linq.Functional
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public sealed partial class Concat<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> first;

        private readonly IEnumerable<T> second;

        private readonly int initialThreadId;

        public Concat(IEnumerable<T> first, IEnumerable<T> second)
        {
            this.first = first;
            this.second = second;
            this.initialThreadId = Environment.CurrentManagedThreadId;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.firstIterator == null && this.secondIterator == null && this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                this.firstIterator = this.first.GetEnumerator();
                this.secondIterator = this.second.GetEnumerator();
                return this;
            }

            return new Concat<T>(this.first, this.second);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public partial class Concat<T> : IEnumerator<T>
    {
        private IEnumerator<T> firstIterator;

        private IEnumerator<T> secondIterator;

        private bool moveSecond;

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            this.firstIterator?.Dispose();
            this.secondIterator?.Dispose();
        }

        public bool MoveNext()
        {
            if (this.moveSecond)
            {
                return this.secondIterator.MoveNext();
            }

            bool moveFirst = this.firstIterator.MoveNext();
            if (moveFirst)
            {
                return true;
            }

            this.moveSecond = true;
            return this.secondIterator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public T Current => this.moveSecond ? this.secondIterator.Current : this.firstIterator.Current;

        object IEnumerator.Current => this.Current;
    }
}

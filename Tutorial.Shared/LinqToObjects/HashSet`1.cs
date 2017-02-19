namespace Tutorial.LinqToObjects
{
    using System.Collections;
    using System.Collections.Generic;

    internal partial class HashSet<T> : IEnumerable<T>
    {
        private readonly IEqualityComparer<T> equalityComparer;

        private readonly Dictionary<int, T> dictionary;

        internal HashSet(IEqualityComparer<T> equalityComparer = null)
        {
            this.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            this.dictionary = new Dictionary<int, T>();
        }

        public IEnumerator<T> GetEnumerator() => this.dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal partial class HashSet<T>
    {
        internal bool Add(T value)
        {
            int hasCode = this.GetHashCode(value);
            if (this.dictionary.ContainsKey(hasCode))
            {
                return false;
            }

            this.dictionary.Add(hasCode, value);
            return true;
        }

        private int GetHashCode(T value) => value == null
            ? -1
            : this.equalityComparer.GetHashCode(value) & int.MaxValue;
            // int.MaxValue is ‭01111111111111111111111111111111‬ in binary, so the result of & is always >= 0.
    }

    internal partial class HashSet<T>
    {
        internal bool Remove(T value)
        {
            int hashCode = this.GetHashCode(value);
            if (this.dictionary.ContainsKey(hashCode))
            {
                this.dictionary.Remove(hashCode);
                return true;
            }

            return false;
        }
    }

    internal partial class HashSet<T>
    {
        internal bool Contains(T value) => this.dictionary.ContainsKey(this.GetHashCode(value));
    }
}

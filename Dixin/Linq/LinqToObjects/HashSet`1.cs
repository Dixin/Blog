namespace Dixin.Linq.LinqToObjects
{
    using System.Collections;
    using System.Collections.Generic;

    public partial class HashSet<T> : IEnumerable<T>
    {
        private readonly IEqualityComparer<T> equalityComparer;

        private readonly Dictionary<int, T> dictionary;

        public HashSet(IEqualityComparer<T> equalityComparer = null)
        {
            this.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            this.dictionary = new Dictionary<int, T>();
        }

        public IEnumerator<T> GetEnumerator() => this.dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public partial class HashSet<T>
    {
        public bool Add(T value)
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
            // int.MaxValue is ‭01111111111111111111111111111111‬ in binary, so the the result of & is always >= 0.
    }

    public partial class HashSet<T>
    {
        public bool Remove(T value)
        {
            int hasCode = this.GetHashCode(value);
            if (this.dictionary.ContainsKey(hasCode))
            {
                this.dictionary.Remove(hasCode);
                return true;
            }

            return false;
        }
    }

    public partial class HashSet<T>
    {
        public bool Contains(T value) => this.dictionary.ContainsKey(this.GetHashCode(value));
    }
}

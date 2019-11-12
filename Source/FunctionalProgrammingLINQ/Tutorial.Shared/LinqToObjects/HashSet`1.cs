namespace Tutorial.LinqToObjects
{
    using System.Collections;
    using System.Collections.Generic;

    public partial class HashSet<T> : IEnumerable<T>
    {
        private readonly IEqualityComparer<T> equalityComparer;

        private readonly Dictionary<int, T> dictionary = new Dictionary<int, T>();

        public HashSet(IEqualityComparer<T> equalityComparer = null) =>
            this.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

        public IEnumerator<T> GetEnumerator() => this.dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public partial class HashSet<T>
    {
        private int GetHashCode(T value) => value == null
            ? -1
            : this.equalityComparer.GetHashCode(value) & int.MaxValue;
        // int.MaxValue is ‭0b01111111_11111111_11111111_11111111‬, so the result of & is always > -1.

        public bool Add(T value)
        {
            int hashCode = this.GetHashCode(value);
            if (this.dictionary.ContainsKey(hashCode))
            {
                return false;
            }
            this.dictionary.Add(hashCode, value);
            return true;
        }

        public HashSet<T> AddRange(IEnumerable<T> values)
        {
            foreach(T value in values)
            {
                this.Add(value);
            }
            return this;
        }
    }

    public partial class HashSet<T>
    {
        public bool Remove(T value)
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

    public partial class HashSet<T>
    {
        public bool Contains(T value) => this.dictionary.ContainsKey(this.GetHashCode(value));
    }
}

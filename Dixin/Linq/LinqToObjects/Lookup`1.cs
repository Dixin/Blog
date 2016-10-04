namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly IDictionary<TKey, IGrouping<TKey, TElement>> groupsWithNonNullKey;

        private readonly IGrouping<TKey, TElement> groupWithNullKey;

        private readonly bool hasGroupWithNullKey;

        internal Lookup(
            IDictionary<TKey, IGrouping<TKey, TElement>> groupsWithNonNullKey, 
            IGrouping<TKey, TElement> groupWithNullKey, 
            bool hasElementWithNullKey)
        {
            this.groupsWithNonNullKey = groupsWithNonNullKey;
            this.groupWithNullKey = groupWithNullKey; // Dictionary<TKey, TElement> does not support null key.
            this.hasGroupWithNullKey = hasElementWithNullKey;
            this.Count = this.groupsWithNonNullKey.Count + (this.hasGroupWithNullKey ? 1 : 0);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (IGrouping<TKey, TElement> group in this.groupsWithNonNullKey.Values)
            {
                yield return group; // Generator implements both IEnumerable<T> and IEnumerator<T>.
            }

            if (this.hasGroupWithNullKey)
            {
                yield return this.groupWithNullKey; // Generator implements both IEnumerable<T> and IEnumerator<T>.
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Contains
            (TKey key) => key == null ? this.hasGroupWithNullKey : this.groupsWithNonNullKey.ContainsKey(key);

        public int Count { get; }

        public IEnumerable<TElement> this[TKey key] => this.Contains(key)
            ? (key == null ? this.groupWithNullKey : this.groupsWithNonNullKey[key])
            // When key does not exist in lookup, return an empty sequence.
            : (IEnumerable<TElement>)Array.Empty<TElement>();
    }
}
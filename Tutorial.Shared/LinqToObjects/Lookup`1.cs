namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<int, Grouping<TKey, TElement>> groups =
            new Dictionary<int, Grouping<TKey, TElement>>();

        private readonly IEqualityComparer<TKey> equalityComparer;

        public Lookup(IEqualityComparer<TKey> eqqualityComparer = null) =>
            this.equalityComparer = eqqualityComparer ?? EqualityComparer<TKey>.Default;

        private int GetHashCode(TKey key) => key == null
            ? -1
            : this.equalityComparer.GetHashCode(key) & int.MaxValue;
        // int.MaxValue is 0b01111111_11111111_11111111_11111111. So the hash code of non-null key is always > -1.

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => this.groups.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Contains(TKey key) => this.groups.ContainsKey(this.GetHashCode(key));

        public int Count => this.groups.Count;

        public IEnumerable<TElement> this[TKey key] =>
            this.groups.TryGetValue(this.GetHashCode(key), out Grouping<TKey, TElement> group)
                ? (IEnumerable<TElement>)group
                : Array.Empty<TElement>();
    }

    public partial class Lookup<TKey, TElement>
    {
        public Lookup<TKey, TElement> AddRange<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            bool skipNullKey = false)
        {
            foreach (TSource value in source)
            {
                TKey key = keySelector(value);
                if (key == null && skipNullKey)
                {
                    continue;
                }
                int hashCOde = this.GetHashCode(key);
                if (this.groups.TryGetValue(hashCOde, out Grouping<TKey, TElement> group))
                {
                    group.Add(elementSelector(value));
                }
                else
                {
                    this.groups.Add(hashCOde, new Grouping<TKey, TElement>(key) { elementSelector(value) });
                }
            }
            return this;
        }
    }
}
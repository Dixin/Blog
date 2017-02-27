namespace Tutorial.LinqToObjects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly List<TElement> values = new List<TElement>();

        public Grouping(TKey key) => this.Key = key;

        public TKey Key { get; }

        public IEnumerator<TElement> GetEnumerator() => this.values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        internal void Add(TElement value) => this.values.Add(value);
    }
}

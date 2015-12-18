namespace Dixin.Linq.Functional
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Sequence<T> : IEnumerable<T>
    {
        private readonly Func<IEnumerator<T>> factory;

        public Sequence(Func<IEnumerable<T>> factory)
        {
            Lazy<IEnumerable<T>> lazy = new Lazy<IEnumerable<T>>(factory);
            this.factory = () => lazy.Value.GetEnumerator();
        }

        public Sequence(IEnumerator<T> iterator)
        {
            this.factory = () => iterator;
        }

        public IEnumerator<T> GetEnumerator() => this.factory();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
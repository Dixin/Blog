namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    public class Nullable<T>
    {
        private readonly Lazy<Tuple<bool, T>> factory;

        public Nullable(Func<Tuple<bool, T>> factory = null)
        {
            this.factory = factory == null ? null : new Lazy<Tuple<bool, T>>(factory);
        }

        public bool HasValue
        {
            [Pure]
            get
            {
                return this.factory?.Value != null && this.factory.Value.Item1 && this.factory.Value.Item2 != null;
            }
        }

        public T Value
        {
            [Pure]
            get
            {
                // Message is copied from mscorlib.dll string table, where key is InvalidOperation_NoValue.
                Contract.Requires<InvalidOperationException>(this.HasValue, "Nullable object must have a value.");

                return this.factory.Value.Item2;
            }
        }
    }
}

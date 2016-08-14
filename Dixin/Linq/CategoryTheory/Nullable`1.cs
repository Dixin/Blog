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
                Tuple<bool, T> result = this.factory?.Value;
                return result != null && result.Item1 && result.Item2 != null;
            }
        }

        public T Value
        {
            [Pure]
            get
            {
                // Message is copied from mscorlib.dll string table, where key is InvalidOperation_NoValue.
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Nullable object must have a value.");
                }

                return this.factory.Value.Item2;
            }
        }
    }
}

namespace Dixin.Linq.CategoryTheory
{
    using System;

    public struct Optional<T>
    {
        private readonly Lazy<Tuple<bool, T>> factory;

        public Optional(Func<Tuple<bool, T>> factory = null)
        {
            this.factory = factory == null ? null : new Lazy<Tuple<bool, T>>(factory);
        }

        public bool HasValue => this.factory?.Value.Item1 ?? false;

        public T Value
        {
            get
            {
                // Message is copied from mscorlib.dll string table, where key is InvalidOperation_NoValue.
                if (!this.HasValue)
                {
                    throw new InvalidOperationException($"{nameof(Optional<T>)} object must have a value.");
                }
                return this.factory.Value.Item2;
            }
        }
    }
}

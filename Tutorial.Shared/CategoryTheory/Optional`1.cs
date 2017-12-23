namespace Tutorial.CategoryTheory
{
    using System;

    public readonly struct Optional<T>
    {
        private readonly Lazy<(bool, T)> factory;

        public Optional(Func<(bool, T)> factory = null) =>
            this.factory = factory == null ? null : new Lazy<(bool, T)>(factory);

        public bool HasValue => this.factory?.Value.Item1 ?? false;

        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new InvalidOperationException($"{nameof(Optional<T>)} object must have a value.");
                }
                return this.factory.Value.Item2;
            }
        }
    }
}

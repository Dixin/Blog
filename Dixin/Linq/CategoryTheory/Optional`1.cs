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
                    throw new InvalidOperationException("Optional object must have a value.");
                }
                return this.factory.Value.Item2;
            }
        }
    }

    public class Nullable2<T>
    {
        private readonly Lazy<T> factory;

        internal Nullable2(Func<T> factory = null)
        {
            this.factory = factory == null ? null : new Lazy<T>(factory);
        }

        public bool HasValue => this.factory != null && this.factory.Value != null;

        public T Value
        {
            get
            {
                // Message is copied from mscorlib.dll string table, where key is InvalidOperation_NoValue.
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Nullable object must have a value.");
                }
                return this.factory.Value;
            }
        }
    }

    public class NullableRef<T> where T : class
    {
        private readonly Lazy<T> factory;

        internal NullableRef(Func<T> factory = null)
        {
            this.factory = factory == null ? null : new Lazy<T>(factory);
        }

        public bool HasValue => this.factory?.Value != null;

        public T Value
        {
            get
            {
                // Message is copied from mscorlib.dll string table, where key is InvalidOperation_NoValue.
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Nullable object must have a value.");
                }
                return this.factory.Value;
            }
        }
    }

    public static class Nullable
    {
        public static Nullable2<T> Create<T>(Func<T> factory) where T : class => new Nullable2<T>(factory);

        public static Nullable2<T?> Create<T>(Func<T?> factory) where T : struct => new Nullable2<T?>(factory);

        public static Nullable2<T> Nullable2<T>(this T value) => new Nullable2<T>(() => value);
    }
}

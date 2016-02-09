namespace Dixin.Linq.LinqToObjects
{
    using System.Diagnostics.CodeAnalysis;

    public static class EmptyArray<T>
    {
        private static T[] cache;

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public static T[] Cache => cache ?? (cache = new T[0]);
    }
}
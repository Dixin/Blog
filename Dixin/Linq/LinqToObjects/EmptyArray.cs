namespace Dixin.Linq.LinqToObjects
{
    public static class EmptyArray<T>
    {
        private static T[] cache;

        public static T[] Cache => cache ?? (cache = new T[0]);
    }
}
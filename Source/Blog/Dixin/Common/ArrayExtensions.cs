namespace Examples.Common
{
    using System;

    public static class ArrayExtensions
    {
        public static T[] ConcatArray<T>(this T[] array1, T[] array2)
        {
            if (array1 == null)
            {
                throw new ArgumentNullException(nameof(array1));
            }

            if (array2 == null)
            {
                throw new ArgumentNullException(nameof(array2));
            }

            T[] concat = new T[array1.Length + array2.Length];
            if (typeof(T).IsPrimitive)
            {
                int byteLength1 = Buffer.ByteLength(array1);
                int byteLength2 = Buffer.ByteLength(array2);
                Buffer.BlockCopy(array1, 0, concat, 0, byteLength1);
                Buffer.BlockCopy(array2, 0, concat, byteLength1, byteLength2);
            }
            else
            {
                Array.Copy(array1, concat, array1.Length);
                Array.Copy(array2, 0, concat, array1.Length, array2.Length);
            }
            return concat;
        }
    }
}

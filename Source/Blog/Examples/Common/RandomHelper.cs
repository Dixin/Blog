namespace Examples.Common;

public static class RandomHelper
{
    public static void Shuffle<T>(Span<T> values, Random? random = null)
    {
        random ??= new Random();
        int length = values.Length;

        for (int index = 0; index < length - 1; index++)
        {
            int swapIndex = random.Next(index, length);
            if (swapIndex != index)
            {
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }
    }

    public static void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination, Random? random = null)
    {
        random ??= new Random();
        if (choices.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(choices));
        }

        for (int index = 0; index < destination.Length; index++)
        {
            destination[index] = choices[random.Next(choices.Length)];
        }
    }

    public static T[] GetItems<T>(ReadOnlySpan<T> choices, int length)
    {
        if (choices.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(choices));
        }

        T[] items = new T[length];
        GetItems(choices, items.AsSpan());
        return items;
    }
}
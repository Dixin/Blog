namespace Examples.Common;

public static class MemoryExtensions
{
    public static List<(Range Range, int Offset, int Length)> ToList<T>(this System.MemoryExtensions.SpanSplitEnumerator<T> enumerator, int length)
        where T : IEquatable<T>
    {
        List<(Range Range, int Offset, int Length)> list = [];
        while (enumerator.MoveNext())
        {
            Range range = enumerator.Current;
            (int offset, int rangeLength) = range.GetOffsetAndLength(length);
            list.Add((range, offset, rangeLength));
        }

        return list;
    }

    public static List<Range> ToList<T>(this System.MemoryExtensions.SpanSplitEnumerator<T> enumerator)
        where T : IEquatable<T>
    {
        List<Range> list = [];
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current);
        }

        return list;
    }
}
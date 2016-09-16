namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;

    internal partial class MutableData
    {
        internal int Value { get; set; }
    }

    internal partial class ImmutableData
    {
        internal ImmutableData(int value)
        {
            this.Value = value;
        }

        internal int Value { get; }
    }

    internal static partial class Immutability
    {
        internal static void State()
        {
            MutableData mutableData = new MutableData() { Value = 1 };
            mutableData.Value = 2; // Change state.

            ImmutableData immutableData = new ImmutableData(value: 1);
        }
    }

    internal partial class MutableData
    {
        internal void Increase() => this.Value++;
    }

    internal partial class ImmutableData
    {
        internal ImmutableData Increase() => new ImmutableData(this.Value + 1);
    }

    internal static partial class Immutability
    {
        internal static void Tuple()
        {
            Tuple<bool, int> tuple = new Tuple<bool, int>(true, 1); // Or Tuple.Create(true, 1).
            List<int> list = new List<int>() { 0, 2 };
        }
    }
}

#if DEMO
namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal interface ITuple
    {
        string ToString(StringBuilder sb);

        int GetHashCode(IEqualityComparer comparer);

        int Size { get; }

    }

    [Serializable]
    public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        public Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public T1 Item1 { get; }

        public T2 Item2 { get; }

        public override bool Equals(object obj) => 
            ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }

            Tuple<T1, T2> objTuple = other as Tuple<T1, T2>;

            return objTuple != null && comparer.Equals(this.Item1, objTuple.Item1)
                && comparer.Equals(this.Item2, objTuple.Item2);
        }

        int IComparable.CompareTo(object obj) => 
            ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            Tuple<T1, T2> otherTuple = other as Tuple<T1, T2>;
            int compareResult = comparer.Compare(this.Item1, otherTuple.Item1);
            return compareResult != 0 ? compareResult : comparer.Compare(this.Item2, otherTuple.Item2);
        }

        // Other members.
    }
}
#endif
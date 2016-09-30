namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal static partial class Immutability
    {
        internal static void Let(IEnumerable<int> source)
        {
            IEnumerable<double> query = from int32 in source
                                        where int32 > 0
                                        let immutableValue = Math.Sqrt(int32)
                                        select int32 + immutableValue;
        }
    }

    internal static partial class Immutability
    {
        internal static void CompiledLet(IEnumerable<int> source)
        {
            IEnumerable<double> query = source
                .Where(int32 => int32 > 0) // where.
                .Select(int32 => new { int32, immutableValue = Math.Sqrt(int32) }) // let.
                .Select(anonymous => anonymous.int32 + anonymous.immutableValue); // select.
        }
    }

    internal partial class MutableDevice
    {
        internal string Name { get; set; }

        internal decimal Price { get; set; }
    }

    internal partial class ImmutableDevice
    {
        internal ImmutableDevice(string name, decimal price)
        {
            this.Name = name;
            this.Price = price;
        }

        internal string Name { get; }

        internal decimal Price { get; }
    }

    internal static partial class Immutability
    {
        internal static void State()
        {
            MutableDevice mutableDevice = new MutableDevice() { Name = "Microsoft Band 2", Price = 249.99M };
            mutableDevice.Price = 174.99M; // Change state.

            ImmutableDevice immutableDevice = new ImmutableDevice(name: "Surface Book", price: 1349.00M);
        }
    }

    internal partial class MutableDevice
    {
        internal void Discount() => this.Price = this.Price * 0.9M;
    }

    internal partial class ImmutableDevice
    {
        internal ImmutableDevice Discount() => new ImmutableDevice(name: this.Name, price: this.Price * 0.9M);
    }

    internal static partial class Immutability
    {
        internal static void AnonymousType()
        {
            var immutableDevice = new { Name = "Surface Book", Price = 1349.00M };
        }

        internal static void CompiledAnonymousType()
        {
            AnonymousType0<string, decimal> immutableDevice = new AnonymousType0<string, decimal>(
                name: "Surface Book", price: 1349.00M);
        }

        internal static void ResuseAnonymousType()
        {
            var device1 = new { Name = "Surface Book", Price = 1349.00M };
            var device2 = new { Name = "Surface Pro 4", Price = 899.00M };
            var device3 = new { Name = "Xbox One S", Price = 399.00 }; // Price is of type double.
            var device4 = new { Price = 174.99M, Name = "Microsoft Band 2" };
            Trace.WriteLine(device1.GetType() == device2.GetType()); // True
            Trace.WriteLine(device1.GetType() == device3.GetType()); // False
            Trace.WriteLine(device1.GetType() == device4.GetType()); // False
        }

        internal static void AnonymousTypeParameter()
        {
            var source = new[] // AnonymousType0<string, decimal>[]
                {
                    new { Name = "Surface Book", Price = 1349.00M },
                    new { Name = "Surface Pro 4", Price = 899.00M }
                };
            var query = source.Where(device => device.Price > 0); // IEnumerable<AnonymousType0<string, decimal>>.
        }

        internal static void AnonymousTypeProperty(Uri uri)
        {
            int variable = 1;
            var anonymous1 = new { variable, uri.Host };
            var anonymous2 = new { variable = variable, Host = uri.Host };
        }
    }

    [CompilerGenerated]
    [DebuggerDisplay(@"\{ Name = {Name}, Price = {Price} }", Type = "<Anonymous Type>")]
    internal sealed class AnonymousType0<TName, TPrice>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TName name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TPrice price;

        [DebuggerHidden]
        public AnonymousType0(TName name, TPrice price)
        {
            this.name = name;
            this.price = price;
        }

        public TName Name => this.name;

        public TPrice Price => this.price;

        [DebuggerHidden]
        public override bool Equals(object value)
        {
            AnonymousType0<TName, TPrice> type = value as AnonymousType0<TName, TPrice>;
            return type != null
                && EqualityComparer<TName>.Default.Equals(this.name, type.name)
                && EqualityComparer<TPrice>.Default.Equals(this.price, type.price);
        }

        // Other members.
    }

    internal class Generic<T>
    {
        internal Generic(T value)
        {
        }
    }

    internal class Generic // Not Generic<T>.
    {
        internal static Generic<T> Create<T>(T value) => new Generic<T>(value); // T can be inferred.
    }

    internal static partial class Immutability
    {
#if DEMO
        internal static void GenericWithAnonymousType()
        {
            var generic = new Generic(new { Name = "Surface Book", Price = 1349.00M });
        }
#endif

        internal static void GenericWithAnonymousType()
        {
            var generic = Generic.Create(new { Name = "Surface Book", Price = 1349.00M });
        }

        internal static void LocalVariable(IEnumerable<int> source, string path)
        {
            var a = default(int); // int.
            var b = 1M; // decimal.
            var c = typeof(void); // Type.
            var d = from int32 in source where int32 > 0 select Math.Sqrt(int32); // IEnumerable<double>.
            var e = File.ReadAllLines(path); // string[].
        }

        internal static void LocalVariableWithType(IEnumerable<int> source, string path)
        {
            var f = (Uri)null;
            var g = (Func<int, int>)(int32 => int32 + 1);
            var h = (Expression<Func<int, int>>)(int32 => int32 + 1);
        }

        internal static void TupleAndList()
        {
            Tuple<string, decimal> tuple1 = new Tuple<string, decimal>("Surface Book", 1349.00M);
            Tuple<string, decimal> tuple2 = Tuple.Create("Surface Book", 1349.00M);
            List<string> list = new List<string>() { "Surface Book", "1349.00M" };
        }

#if DEMO
        internal static var Method(var value) // Cannot be compiled.
        {
            var value1; // Cannot be compiled.
            var value2 = null; // Cannot be compiled.
            return value2;
        }
#endif

        internal static Tuple<string, decimal> Method(Tuple<string, decimal> value)
        {
            Tuple<string, decimal> value1;
            Tuple<string, decimal> value2 = null;
            return value2;
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
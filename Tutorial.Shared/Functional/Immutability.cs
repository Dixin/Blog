namespace Tutorial.Functional
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

        internal static void ReuseAnonymousType()
        {
            var device1 = new { Name = "Surface Book", Price = 1349.00M };
            var device2 = new { Name = "Surface Pro 4", Price = 899.00M };
            var device3 = new { Name = "Xbox One S", Price = 399.00 }; // Price is of type double.
            var device4 = new { Price = 174.99M, Name = "Microsoft Band 2" };
            (device1.GetType() == device2.GetType()).WriteLine(); // True
            (device1.GetType() == device3.GetType()).WriteLine(); // False
            (device1.GetType() == device4.GetType()).WriteLine(); // False
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
        public override bool Equals(object value) =>
            value is AnonymousType0<TName, TPrice> type
            && type != null
            && EqualityComparer<TName>.Default.Equals(this.name, type.name)
            && EqualityComparer<TPrice>.Default.Equals(this.price, type.price);

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
            ValueTuple<string, decimal> tuple = new ValueTuple<string, decimal>("Surface Book", 1349M);
            List<string> list = new List<string>() { "Surface Book", "1349.00M" };
        }

#if DEMO
        internal static var Method(var values) // Cannot be compiled.
        {
            return values;
        }
#endif

        internal static ValueTuple<string, decimal> Method(ValueTuple<string, decimal> values)
        {
            return values;
        }

        internal static void TupleTypeLiteral()
        {
            (string, decimal) tuple1 = ("Surface Pro 4", 899M);
            // Compiled to: 
            // ValueTuple<string, decimal> tuple1 = new ValueTuple<string, decimal>("Surface Pro 4", 899M);

            (int, bool, (string, decimal)) tuple2 = (1, true, ("Surface Studio", 2999M));
            // ValueTuple<int, bool, ValueTuple<string, decimal>> tuple2 = 
            //    new ValueTuple<int, bool, ValueTuple<string, decimal>>(1, true, ("Surface Studio", 2999M))
        }

        internal static (string, decimal) MethodReturnMultipleValues()
        // internal static ValueTuple<string, decimal> MethodReturnMultipleValues()
        {
            string returnValue1 = default(string);
            int returnValue2 = default(int);

            (string, decimal) Function() => (returnValue1, returnValue2);
            // ValueTuple<string, decimal> Function() => new ValueTuple<string, decimal>(returnValue1, returnValue2);

            Func<(string, decimal)> function = () => (returnValue1, returnValue2);
            // Func<ValueTuple<string, decimal>> function = () => new ValueTuple<string, decimal>(returnValue1, returnValue2);

            return (returnValue1, returnValue2);
        }

        internal static void ElementName()
        {
            (string Name, decimal Price) tuple1 = ("Surface Pro 4", 899M);
            tuple1.Name.WriteLine();
            tuple1.Price.WriteLine();
            // Compiled to: 
            // ValueTuple<string, decimal> tuple1 = new ValueTuple<string, decimal>("Surface Pro 4", 899M);
            // TraceExtensions.WriteLine(tuple1.Item1);
            // TraceExtensions.WriteLine(tuple1.Item2)

            (string Name, decimal Price) tuple2 = (ProductNanme: "Surface Book", ProductPrice: 1349M);
            tuple2.Name.WriteLine(); // Element names on the right are ignore.

            var tuple3 = (Name: "Surface Studio", Price: 2999M);
            tuple3.Name.WriteLine(); // Element names are available through var.

            ValueTuple<string, decimal> tuple4 = (Name: "Xbox One", Price: 179M);
            tuple4.Item1.WriteLine(); // Element names are not available on ValueTuple<T1, T2>.
            tuple4.Item2.WriteLine();

            (string Name, decimal Price) Function((string Name, decimal Price) tuple)
            {
                tuple.Name.WriteLine(); // Parameter element names are available in function.
                return (tuple.Name, tuple.Price - 10M);
            };
            var tuple5 = Function(("Xbox One S", 299M));
            tuple5.Name.WriteLine(); // Return value element names are available through var.
            tuple5.Price.WriteLine();

            Func<(string Name, decimal Price), (string Name, decimal Price)> function = tuple =>
            {
                tuple.Name.WriteLine(); // Parameter element names are available in function.
                return (tuple.Name, tuple.Price - 100M);
            };
            var tuple6 = function(("HoloLens", 3000M));
            tuple5.Name.WriteLine(); // Return value element names are available through var.
            tuple5.Price.WriteLine();
        }

        internal static void DeconstructTuple()
        {
            (string, decimal) GetProductInfo() => ("HoLoLens", 3000M);
            var (name, price) = GetProductInfo();
            name.WriteLine(); // name is string.
            price.WriteLine(); // price is decimal.
        }
    }

    internal partial class Device
    {
        internal void Deconstruct(out string name, out string description, out decimal price)
        {
            name = this.Name;
            description = this.Description;
            price = this.Price;
        }
    }

    internal static class DeviceExtensions
    {
        internal static void Deconstruct(this Device device, out string name, out string description, out decimal price)
        {
            name = device.Name;
            description = device.Description;
            price = device.Price;
        }
    }

    internal static partial class Immutability
    { 
        internal static void DeconstructDevice()
        {
            Device GetDevice() => new Device() { Name = "Surface studio", Description = "All-in-one PC.", Price = 2999M };
            var (name, description, price) = GetDevice();
            // Compiled to:
            // string name; string description; decimal prive;
            // surfaceStudio.Deconstruct(out name, out description, out price);
            name.WriteLine(); // Surface studio
            description.WriteLine(); // All-in-one PC.
            price.WriteLine(); // 2999
        }

        internal static void Performance()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            (int, bool, string)[] tuples = Enumerable.Repeat(0, 1_000_000).Select(_ => (Int32: 1, Boolean: true, String: nameof(Performance))).ToArray();
            stopwatch.Stop(); // 26
            stopwatch.ElapsedMilliseconds.WriteLine();

            stopwatch = Stopwatch.StartNew();
            var anonymous = Enumerable.Repeat(0, 1_000_000).Select(_ => new { Int32 = 1, Boolean = true, String = nameof(Performance) }).ToArray();
            stopwatch.Stop(); // 114
            stopwatch.ElapsedMilliseconds.WriteLine();
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

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) =>
            other != null && other is Tuple<T1, T2> objTuple && objTuple != null
            && comparer.Equals(this.Item1, objTuple.Item1) && comparer.Equals(this.Item2, objTuple.Item2);

        int IComparable.CompareTo(object obj) =>
            ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if(other is Tuple<T1, T2> otherTuple)
            {
                int compareResult = comparer.Compare(this.Item1, otherTuple.Item1);
                return compareResult != 0 ? compareResult : comparer.Compare(this.Item2, otherTuple.Item2);
            }
            return 1;
        }

        // Other members.
    }
}

namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface ITupleInternal
    {
        int Size
        {
            get;
        }

        int GetHashCode(IEqualityComparer comparer);

        string ToStringEnd();
    }

    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2>>, ITupleInternal
    {
        public T1 Item1;

        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public override bool Equals(object obj) => obj is ValueTuple<T1, T2> tuple && this.Equals(tuple);

        public bool Equals(ValueTuple<T1, T2> other) =>
            EqualityComparer<T1>.Default.Equals(this.Item1, other.Item1)
            && EqualityComparer<T2>.Default.Equals(this.Item2, other.Item2);

        public int CompareTo(ValueTuple<T1, T2> other)
        {
            int compareItem1 = Comparer<T1>.Default.Compare(this.Item1, other.Item1);
            return compareItem1 != 0 ? compareItem1 : Comparer<T2>.Default.Compare(this.Item2, other.Item2);
        }

        public override string ToString() => $"({this.Item1}, {this.Item2})";

        // Other members.
    }
}
#endif

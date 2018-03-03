namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal static partial class Immutability
    {
        internal static void Const()
        {
            const int immutable1 = 1;
            const int immutable2 = immutable1 + 10;
            const string immutable3 = "https://weblogs.asp.net/dixin";
            const object immutale4 = null;
            const Uri immutable5 = null;
#if DEMO
            const Uri immutable6 = new Uri(immutable2); // Cannot be compiled.
#endif

            double variable = Math.Abs(immutable2); // Compiled to Math.Abs(11)
        }
    }

    internal static partial class Immutability
    {
        internal static void ForEach(IEnumerable<int> source)
        {
            foreach (int immutable in source)
            {
                // Cannot reassign to immutable.
            }
        }

        internal static void Using(Func<IDisposable> disposableFactory)
        {
            using (IDisposable immutable = disposableFactory())
            {
                // Cannot reassign to immutable.
            }
        }
    }

    internal partial class Device
    {
        internal void InstanceMethod()
        {
            // Cannot reassign to this.
        }
    }

    internal static partial class Immutability
    {
        internal static void ParameterAndReturn<T>(Span<T> span)
        {
            ref readonly T Last(in Span<T> immutableParameter)
            {
                // Cannot reassign to immutableParameter.
                int length = immutableParameter.Length;
                if (length > 0)
                {
                    return ref immutableParameter[length - 1];
                }
                throw new ArgumentException("Span is empty.", nameof(immutableParameter));
            }

            ref readonly T immutableReturn = ref Last(in span);
            // Cannot reassign to immutableReturn.
        }

        internal static void ReadOnlyReference()
        {
            int value = 1;
            int copyOfValue = value; // Assign by copy.
            copyOfValue = 10; // After the assignment, value does not change.
            ref int mutaleRefOfValue = ref value; // Assign by reference.
            mutaleRefOfValue = 10; // After the reassignment, value changes too.
            ref readonly int immutableRefOfValue = ref value; // Assign by readonly reference.
#if DEMO
            immutableRefOfValue = 0; // Cannot be compiled. Cannot reassign to immutableRefToValue.
#endif

            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            Uri copyOfReference = reference; // Assign by copy.
            copyOfReference = new Uri("https://flickr.com/dixin"); // After the assignment, reference does not change.
            ref Uri mutableRefOfReference = ref reference; // Assign by reference.
            mutableRefOfReference = new Uri("https://flickr.com/dixin"); // After the reassignment, reference changes too.
            ref readonly Uri immutableRefOfReference = ref reference; // Assign by readonly reference.
#if DEMO
            immutableRefOfReference = null; // Cannot be compiled. Cannot reassign to immutableRefToValue.
#endif
        }

        internal static void QueryExpression(IEnumerable<int> source1, IEnumerable<int> source2)
        {
            IEnumerable<IGrouping<int, int>> query =
                from immutable1 in source1
                    // Cannot assign to immutable1.
                join immutable2 in source2 on immutable1 equals immutable2 into immutable3
                // Cannot assign to immutable2, immutable3.
                let immutable4 = immutable1
                // Cannot assign to immutable4.
                group immutable4 by immutable4 into immutable5
                // Cannot assign to immutable5.
                select immutable5 into immutable6
                // Cannot assign to immutable6.
                select immutable6;
        }

        internal static void Let(IEnumerable<int> source)
        {
            IEnumerable<double> query =
                from immutable1 in source
                let immutable2 = Math.Sqrt(immutable1)
                select immutable1 + immutable2;
        }

        internal static void CompiledLet(IEnumerable<int> source)
        {
            IEnumerable<double> query = source // from clause.
                .Select(immutable1 => new { immutable1, immutable2 = Math.Sqrt(immutable1) }) // let clause.
                .Select(anonymous => anonymous.immutable1 + anonymous.immutable2); // select clause.
        }
    }

    internal partial class ImmutableDevice
    {
        private readonly string name;

        private readonly decimal price;
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
            // Price drops.
            mutableDevice.Price -= 50M;

            ImmutableDevice immutableDevice = new ImmutableDevice(name: "Surface Book", price: 1349.00M);
            // Price drops.
            immutableDevice = new ImmutableDevice(name: immutableDevice.Name, price: immutableDevice.Price - 50M);
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

    internal partial struct Complex
    {
        internal Complex(double real, double imaginary)
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }

        internal double Real { get; }

        internal double Imaginary { get; }
    }

    internal partial struct Complex
    {
        internal Complex(Complex value) => this = value; // Can reassign to this.

        internal Complex Value
        {
            get => this;
            set => this = value; // Can reassign to this.
        }

        internal Complex ReplaceBy(Complex value) => this = value; // Can reassign to this.

        internal Complex Mutate(double real, double imaginary) =>
            this = new Complex(real, imaginary); // Can reassign to this.
    }

    internal static partial class Immutability
    {
        internal static void Structure()
        {
            Complex complex1 = new Complex(1, 1);
            Complex complex2 = new Complex(2, 2);
            complex1.Real.WriteLine(); // 1
            complex1.ReplaceBy(complex2);
            complex1.Real.WriteLine(); // 2
        }
    }

    internal readonly partial struct ImmutableComplex
    {
        internal ImmutableComplex(double real, double imaginary)
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }

        internal ImmutableComplex(in ImmutableComplex value) =>
            this = value; // Can reassign to this only in constructor.

        internal double Real { get; }

        internal double Imaginary { get; }

        internal void InstanceMethod(in ImmutableComplex value)
        {
            // Cannot reassign to this.
        }
    }

    internal readonly partial struct ImmutableComplex
    {
        private static readonly ImmutableComplex zero = new ImmutableComplex(0.0, 0.0);

        public static ref readonly ImmutableComplex Zero => ref zero;
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
            var source = new[] // AnonymousType0<string, decimal>[].
            {
                new { Name = "Surface Book", Price = 1349.00M },
                new { Name = "Surface Pro 4", Price = 899.00M }
            };
            var query = // IEnumerable<AnonymousType0<string, decimal>>.
                source.Where(device => device.Price > 0);
        }

        internal static void PropertyInference(Uri uri, int value)
        {
            var anonymous1 = new { value, uri.Host };
            var anonymous2 = new { value = value, Host = uri.Host };
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

    internal static partial class Immutability
    {
        internal static void LocalVariable(IEnumerable<int> source, string path)
        {
            var a = default(int); // int.
            var b = 1M; // decimal.
            var c = typeof(void); // Type.
            var d = from int32 in source where int32 > 0 select Math.Sqrt(int32); // IEnumerable<double>.
            var e = File.ReadAllLines(path); // string[].
        }

        internal static void LocalVariableWithType()
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

        internal static ValueTuple<string, decimal> Method(ValueTuple<string, decimal> values)
        {
            ValueTuple<string, decimal> variable1;
            ValueTuple<string, decimal> variable2 = default;
            IEnumerable<ValueTuple<string, decimal>> variable3;
            return values;
        }

#if DEMO
        internal static var Method(var values) // Cannot be compiled.
        {
            var variable1; // Cannot be compiled.
            var variable2 = default; // Cannot be compiled.
            IEnumerable<var> variable3; // Cannot be compiled.
            return values;
        }
#endif

        internal static void TupleTypeLiteral()
        {
            (string, decimal) tuple1 = ("Surface Pro 4", 899M);
            // Compiled to: 
            // ValueTuple<string, decimal> tuple1 = new ValueTuple<string, decimal>("Surface Pro 4", 899M);

            (int, bool, (string, decimal)) tuple2 = (1, true, ("Surface Studio", 2999M));
            // ValueTuple<int, bool, ValueTuple<string, decimal>> tuple2 = 
            //    new ValueTuple<int, bool, new ValueTuple<string, decimal>>(1, true, ("Surface Studio", 2999M))
        }

        internal static (string, decimal) MethodReturnMultipleValues()
        // internal static ValueTuple<string, decimal> MethodReturnMultipleValues()
        {
            string returnValue1 = default;
            int returnValue2 = default;

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

        internal static void ElementInference(Uri uri, int value)
        {
            var tuple1 = (value, uri.Host);
            var tuple2 = (value: value, Host: uri.Host);
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
            // string name; string description; decimal price;
            // surfaceStudio.Deconstruct(out name, out description, out price);
            name.WriteLine(); // Surface studio
            description.WriteLine(); // All-in-one PC.
            price.WriteLine(); // 2999
        }

        internal static void Discard()
        {
            Device GetDevice() => new Device() { Name = "Surface studio", Description = "All-in-one PC.", Price = 2999M };
            var (_, _, price1) = GetDevice();
            (_, _, decimal price2) = GetDevice();
        }

        internal static void TupleAssignment(int value1, int value2)
        {
            (value1, value2) = (1, 2);
            // Compiled to:
            // value1 = 1; value2 = 2;

            (value1, value2) = (value2, value1);
            // Compiled to:
            // int temp1 = value1; int temp2 = value2;
            // value1 = temp2; value2 = temp1;
        }

        internal static int Fibonacci(int n)
        {
            (int a, int b) = (0, 1);
            for (int i = 0; i < n; i++)
            {
                (a, b) = (b, a + b);
            }
            return a;
        }

        internal class ImmutableDevice
        {
            internal ImmutableDevice(string name, decimal price) =>
                (this.Name, this.Price) = (name, price);

            internal string Name { get; }

            internal decimal Price { get; }
        }

        internal static void ImmutableCollection()
        {
            ImmutableList<int> immutableList1 = ImmutableList.Create(1, 2, 3);
            ImmutableList<int> immutableList2 = immutableList1.Add(4); // Create a new collection.
            object.ReferenceEquals(immutableList1, immutableList2).WriteLine(); // False
        }

        internal static void ReadOnlyCollection()
        {
            List<int> mutableList = new List<int>() { 1, 2, 3 };
            ImmutableList<int> immutableList = ImmutableList.CreateRange(mutableList);
            ReadOnlyCollection<int> readOnlyCollection = new ReadOnlyCollection<int>(mutableList);
            // ReadOnlyCollection<int> wraps a mutable source, just has no methods like Add, Remove, etc.

            mutableList.Add(4);
            immutableList.Count.WriteLine(); // 3
            readOnlyCollection.Count.WriteLine(); // 4
        }

        internal static void Closure()
        {
            int value = 1;
            Action writeValue = () => value.WriteLine();
            writeValue(); // 1
            value = 2;
            writeValue(); // 2
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
    using System.Runtime.Serialization;

    public struct DateTime : IComparable, IComparable<DateTime>, IConvertible, IEquatable<DateTime>, IFormattable, ISerializable
    {
        private const int DaysPerYear = 365;
        // Compiled to:
        // .field private static literal int32 DaysPerYear = 365

        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Compiled to:
        // .field private static literal int32 DaysPer4Years = 1461

        // Other members.
    }
}

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

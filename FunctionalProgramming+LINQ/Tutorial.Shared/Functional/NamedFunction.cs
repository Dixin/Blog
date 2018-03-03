namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal partial class Data
    {
        private readonly int value;

        static Data() // Static constructor.
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // .cctor
        }

        internal Data(int value) // Constructor.
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // .ctor
            this.value = value;
        }

        internal int Value
        {
            get { return this.value; }
        }

        ~Data() // Finalizer.
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // Finalize
        }
        // Compiled to:
        // protected override void Finalize()
        // {
        //    try
        //    {
        //        Trace.WriteLine(MethodBase.GetCurrentMethod().Name);
        //    }
        //    finally
        //    {
        //        base.Finalize();
        //    }
        // }
    }

    internal partial class Data
    {
        internal int InstanceAdd(int value1, int value2)
        {
            return this.value + value1 + value2;
        }

        internal static int StaticAdd(Data @this, int value1, int value2)
        {
            return @this.value + value1 + value2;
        }
    }

    internal partial class Data
    {
        internal int CompiledInstanceAdd(int value1, int value2)
        {
            Data arg0 = this;
            int arg1 = value1;
            int arg2 = value2;
            return arg0.value + arg1 + arg2;
        }

        internal static int CompiledStaticAdd(Data @this, int value1, int value2)
        {
            Data arg0 = @this;
            int arg1 = value1;
            int arg2 = value2;
            return arg0.value + arg1 + arg2;
        }
    }

    internal static partial class DataExtensions
    {
        internal static int ExtensionAdd(this Data @this, int value1, int value2)
        {
            return @this.Value + value1 + value2;
        }
    }

    internal static partial class Functions
    {
        internal static void CallExtensionMethod(Data data)
        {
            int result = data.ExtensionAdd(1, 2);
        }
    }

#if DEMO
    internal static partial class DataExtensions
    {
        [Extension]
        internal static int CompiledExtensionAdd(Data @this, int value1, int value2)
        {
            return @this.value + value1 + value2;
        }
    }
#endif

    internal static partial class Functions
    {
        internal static void CompiledCallExtensionMethod(Data data)
        {
            int result = DataExtensions.ExtensionAdd(data, 1, 2);
        }
    }

    internal partial class Data : IEquatable<Data>
    {
        public override bool Equals(object obj)
        {
            return obj is Data && this.Equals((Data)obj);
        }

        public bool Equals(Data other) // Member of IEquatable<T>.
        {
            return this.value == other.value;
        }
    }

    internal static partial class DataExtensions
    {
        internal static bool Equals(Data @this, Data other)
        {
            return @this.Value == other.Value;
        }
    }

    internal static partial class Functions
    {
        internal static void CallMethods(Data data1, Data data2)
        {
            bool result1 = data1.Equals(string.Empty); // object.Equals.
            bool result2 = data1.Equals(data2); // Data.Equals.
            bool result3 = DataExtensions.Equals(data1, data2); // DataExtensions.Equals.
        }
    }

    internal static class DayOfWeekExtensions
    {
        internal static bool IsWeekend(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday;
        }
    }

    internal static partial class Functions
    {
        internal static void CallEnumerationExtensionMethod(DayOfWeek dayOfWeek)
        {
            bool result = dayOfWeek.IsWeekend();
        }

        internal static void TraceValueAndSequence(Uri value, IEnumerable<Uri> values)
        {
            value.WriteLine();
            // Equivalent to: Trace.WriteLine(value);

            values.WriteLines();
            // Equivalent to: 
            // foreach (Uri value in values)
            // {
            //    Trace.WriteLine(value);
            // }
        }
    }

    internal partial class Data
    {
        public static Data operator +(Data data1, Data data2)
        // Compiled to: public static Data op_Addition(Data data1, Data data2)
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // op_Addition
            return new Data(data1.value + data2.value);
        }

        public static explicit operator int(Data value)
        // Compiled to: public static int op_Explicit(Data data)
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // op_Explicit
            return value.value;
        }

        public static explicit operator string(Data value)
        // Compiled to: public static string op_Explicit(Data data)
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // op_Explicit
            return value.value.ToString();
        }

        public static implicit operator Data(int value)
        // Compiled to: public static Data op_Implicit(int data)
        {
            Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // op_Implicit
            return new Data(value);
        }
    }

    internal static partial class Functions
    {
        internal static void Operators(Data data1, Data data2)
        {
            Data result = data1 + data2; // Compiled to: Data.op_Addition(data1, data2)
            int int32 = (int)data1; // Compiled to: Data.op_Explicit(data1)
            string @string = (string)data1; // Compiled to: Data.op_Explicit(data1)
            Data data = 1; // Compiled to: Data.op_Implicit(1)
        }
    }

    internal partial class Device
    {
        private string description;

        internal string Description
        {
            get // Compiled to: internal string get_Description()
            {
                Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // get_Description
                return this.description;
            }
            set // Compiled to: internal void set_Description(string value)
            {
                Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // set_Description
                this.description = value;
            }
        }
    }

    internal static partial class Functions
    {
        internal static void Property(Device device)
        {
            string description = device.Description; // Compiled to: device.get_Description()
            device.Description = string.Empty; // Compiled to: device.set_Description(string.Empty)
        }

        internal partial class Category
        {
            private readonly Subcategory[] subcategories;

            internal Category(Subcategory[] subcategories)
            {
                this.subcategories = subcategories;
            }

            internal Subcategory this[int index]
            {
                get // Compiled to: internal Uri get_Item(int index)
                {
                    Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // get_Item
                    return this.subcategories[index];
                }
                set // Compiled to: internal Uri set_Item(int index, Subcategory subcategory)
                {
                    Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // set_Item
                    this.subcategories[index] = value;
                }
            }
        }

        internal static void Indexer(Category category)
        {
            Subcategory subcategory = category[0]; // Compiled to: category.get_Item(0)
            category[0] = subcategory; // Compiled to: category.set_Item(0, subcategory)
        }

#if DEMO
        internal class Downloader
        {
            internal event EventHandler<DownloadEventArgs> Completed
            {
                add // Compiled to: internal void add_Completed(EventHandler<DownloadEventArgs> value)
                {
                    Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // add_Completed
                }
                remove // Compiled to: internal void remove_Completed(EventHandler<DownloadEventArgs> value)
                {
                    Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // remove_Completed
                }
            }
        }
#endif

        internal static void EventAccessor(Downloader downloader)
        {
            downloader.Completed += TraceContent; // Compiled to: downloader.add_Completed(TraceContent)
            downloader.Completed -= SaveContent; // Compiled to: downloader.remove_Completed(SaveContent)
        }
    }

    internal partial class Data
    {
        internal event EventHandler Saved
        {
            add // Compiled to: internal void add_Saved(EventHandler value)
            {
                Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // add_Saved
            }
            remove // Compiled to: internal void remove_Saved(EventHandler value)
            {
                Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // remove_Saved
            }
        }
    }

    internal partial class Functions
    {
        internal static void DataSaved(object sender, EventArgs args) { }

        internal static void EventAccessor(Data data)
        {
            data.Saved += DataSaved; // Compiled to: data.add_Saved(DataSaved)
            data.Saved -= DataSaved; // Compiled to: data.remove_Saved(DataSaved)
        }

        internal static void TraceString(Uri uri, FileInfo file, int int32)
        {
            Trace.WriteLine(uri?.ToString());
            Trace.WriteLine(file?.ToString());
            Trace.WriteLine(int32.ToString());
        }
    }

    internal partial class Functions
    {
        internal static void TraceObject(Uri uri, FileInfo file, int int32)
        {
            Trace.WriteLine(uri);
            Trace.WriteLine(file);
            Trace.WriteLine(int32);
        }

#if DEMO
        internal static string FromInt64(long value)
        {
            return value.ToString();
        }

        internal static DateTime FromInt64(long value)
        {
            return new DateTime(value);
        }
#endif

        internal static void SwapInt32(ref int value1, ref int value2)
        {
            (value1, value2) = (value2, value1);
        }

        internal static void Swap<T>(ref T value1, ref T value2)
        {
            (value1, value2) = (value2, value1);
        }

        internal static IStack<T> PushValue<T>(IStack<T> stack) where T : new()
        {
            stack.Push(new T());
            return stack;
        }

        internal static void TypeArgumentInference(string value1, string value2)
        {
            Swap<string>(ref value1, ref value2);
            Swap(ref value1, ref value2);
        }

        internal static T Generic1<T>(T value)
        {
            Trace.WriteLine(value);
            return default;
        }

        internal static TResult Generic2<T, TResult>(T value)
        {
            Trace.WriteLine(value);
            return default;
        }

        internal static void ReturnTypeInference()
        {
            int value2 = Generic1(0);
            string value3 = Generic2<int, string>(0); // Generic2<int>(0) cannot be compiled.
        }

        internal static void NullArgumentType()
        {
            Generic1<FileInfo>(null);
            Generic1((FileInfo)null);
            FileInfo file = null;
            Generic1(file);
        }

        internal class Generic<T>
        {
            internal Generic(T input) { } // T cannot be inferred.
        }

        internal static Generic<IEnumerable<IGrouping<int, string>>> GenericConstructor(
            IEnumerable<IGrouping<int, string>> input)
        {
            return new Generic<IEnumerable<IGrouping<int, string>>>(input);
            // Cannot be compiled:
            // return new Generic(input);
        }

        internal class Generic // Not Generic<T>.
        {
            internal static Generic<T> Create<T>(T input) => new Generic<T>(input); // T can be inferred.
        }

        internal static Generic<IEnumerable<IGrouping<int, string>>> GenericCreate(
            IEnumerable<IGrouping<int, string>> input)
        {
            return Generic.Create(input);
        }
    }
}

namespace Tutorial.Functional
{
    using System.Collections.Generic;

    using static System.DayOfWeek;
    using static System.Math;
    using static System.Diagnostics.Trace;
    using static System.Linq.Enumerable;

    internal static partial class Functions
    {
        internal static void UsingStatic(int value, int[] array)
        {
            int abs = Abs(value); // Compiled to: Math.Abs(value)
            WriteLine(Monday); // Compiled to: Trace.WriteLine(DayOfWeek.Monday)
            List<int> list2 = array.ToList(); // Compiled to: Enumerable.ToList(array)
        }
    }

#if DEMO
    [Table(Name = "Production.Product")]
    public partial class Product : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public Product()
        {
            this.OnCreated(); // Call.
        }

        partial void OnCreated(); // Signature.

        // Other members.
    }

    public partial class Product
    {
        partial void OnCreated() // Optional implementation.
        {
            Trace.WriteLine($"{nameof(Product)} is created.");
        }
    }
#endif
}

#if DEMO
namespace System.Reflection
{
    using System.Runtime.InteropServices;

    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(_MethodBase))]
    [ComVisible(true)]
    public abstract class MethodBase : MemberInfo, _MethodBase
    {
        public bool IsAbstract { get; }

        public bool IsPrivate { get; }

        public bool IsPublic { get; }

        public bool IsStatic { get; }

        public bool IsVirtual { get; }

        // public abstract string Name { get; }

        public static MethodBase GetCurrentMethod();

        public virtual MethodBody GetMethodBody();

        public abstract ParameterInfo[] GetParameters();

        public object Invoke(object obj, object[] parameters);
    }
}

namespace System.Diagnostics
{
    public sealed class Trace
    {
        [Conditional("TRACE")]
        public static void WriteLine(string message);

        [Conditional("TRACE")]
        public static void WriteLine(object value);
    }
}

namespace System
{
    public struct DateTime : IComparable, IComparable<DateTime>, IConvertible, IEquatable<DateTime>, IFormattable
    {
        public DateTime(long ticks);

        public DateTime(int year, int month, int day);

        public DateTime(int year, int month, int day, int hour, int minute, int second);

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond);

        // Other constructor overloads and other members.
    }
}

namespace System.Data
{
    using System.Reflection;

    public class DataRow
    {
        public object this[DataColumn column] { get; set; }

        public object this[string columnName] { get; set; }

        public object this[int columnIndex] { get; set; }

        // Other indexer overloads and other members.
    }
}

namespace System
{
    using System.Collections;

    internal interface ITuple { }

    [Serializable]
    public class ValueTuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4);

        // Other members.
    }
}

namespace System
{
    public static class ValueTuple
    {
        public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);
    }
}

namespace System
{
    public static class Convert
    {
        public static string ToString(bool value);

        public static string ToString(int value);

        public static string ToString(long value);

        public static string ToString(decimal value);

        public static string ToString(DateTime value);

        public static string ToString(object value);

        public static string ToString(int value, IFormatProvider provider);

        public static string ToString(int value, int toBase);

        // More overloads and other members.
    }
}

namespace System.IO
{
    public abstract class Stream : MarshalByRefObject, IDisposable
    {
        public virtual void WriteByte(byte value);

        // Other members.
    }

    public class FileStream : Stream
    {
        public override void WriteByte(byte value);

        // Other members.
    }

    public class MemoryStream : Stream
    {
        public override void WriteByte(byte value);

        // Other members.
    }
}
#endif

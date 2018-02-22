#if DEMO
namespace System
{
    public class Object
    {
        public Object();

        public static bool Equals(Object objA, Object objB);

        public static bool ReferenceEquals(Object objA, Object objB);

        public virtual bool Equals(Object obj);

        public virtual int GetHashCode();

        public Type GetType();

        public virtual string ToString();

        // Other members.
    }
}

namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    internal sealed class SafeSerializationManager
    {
        internal event EventHandler<SafeSerializationEventArgs> SerializeObjectState;
    }

    [Serializable]
    public class Exception : ISerializable, _Exception // , System.Object.
    {
        internal string _message; // Field.

        private Exception _innerException; // Field.

        [OptionalField(VersionAdded = 4)]
        private SafeSerializationManager _safeSerializationManager; // Field.

        public Exception InnerException { get { return this._innerException; } } // Property.

        public Exception(string message, Exception innerException) // Constructor.
        {
            this.Init();
            this._message = message;
            this._innerException = innerException;
        }

        public virtual Exception GetBaseException() // Method.
        {
            Exception innerException = this.InnerException;
            Exception result = this;
            while (innerException != null)
            {
                result = innerException;
                innerException = innerException.InnerException;
            }
            return result;
        }

        private void Init()
        {
            this._message = null;
            this._safeSerializationManager = new SafeSerializationManager();
        }

        protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState // Event.
        {
            add
            {
                this._safeSerializationManager.SerializeObjectState += value;
            }
            remove
            {
                this._safeSerializationManager.SerializeObjectState -= value;
            }
        }

        internal enum ExceptionMessageKind // Nested enumeration type.
        {
            ThreadAbort = 1,
            ThreadInterrupted,
            OutOfMemory
        }

        // Other members.
    }
}

namespace System
{
    public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable<TimeSpan>, IFormattable // , System.ValueType
    {
        public const long TicksPerMillisecond = 10000; // Constant.

        public static readonly TimeSpan Zero = new TimeSpan(0); // Field.

        internal long _ticks; // Field.

        public TimeSpan(long ticks) // COnstructor.
        {
            this._ticks = ticks;
        }

        public long Ticks { get { return _ticks; } } // Property.

        public int Milliseconds // Property.
        {
            get { return (int)((_ticks / TicksPerMillisecond) % 1000); }
        }

        public static bool Equals(TimeSpan t1, TimeSpan t2) // Method.
        {
            return t1._ticks == t2._ticks;
        }

        public static bool operator ==(TimeSpan t1, TimeSpan t2) // Operator.
        {
            return t1._ticks == t2._ticks;
        }

        // Other members.
    }
}

namespace System
{
    [Serializable]
    public enum DayOfWeek // : int
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
    }
}

namespace System
{
    public delegate void Action();
}

namespace System.ComponentModel
{
    using System.Collections;

    public interface INotifyDataErrorInfo
    {
        event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged; // Event.

        bool HasErrors { get; } // Property.

        IEnumerable GetErrors(string propertyName); // Method.
    }
}

namespace System.Data.SqlClient
{
    using System.ComponentModel;
    using System.Data.Common;

    [TypeConverter(typeof(SqlParameterConverter))]
    public sealed class SqlParameter : DbParameter, IDbDataParameter, IDataParameter, ICloneable
    {
        object ICloneable.Clone()
        {
            return new SqlParameter(this);
        }

        internal sealed class SqlParameterConverter : ExpandableObjectConverter { }

        // Other members.
    }
}

namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDisposable
    {
        void Dispose();
    }
}

namespace System
{
    public static class Math
    {
        // Static members only.
    }
}

namespace System
{
    public partial struct Nullable<T> where T : struct
    {
        private bool hasValue;

        internal T value;

        public Nullable(T value)
        {
            this.value = value;
            this.hasValue = true;
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                {
                    throw new InvalidOperationException("Nullable object must have a value.");
                }
                return this.value;
            }
        }

        // Other members.
    }
}
#endif

namespace Tutorial.Functional
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;


    internal static partial class Fundamentals
    {
        internal partial class Point
        {
            private readonly int x;

            private readonly int y;

            internal Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            internal int X { get { return this.x; } }

            internal int Y { get { return this.y; } }
        }

        internal partial struct ValuePoint
        {
            private readonly int x;

            private readonly int y;

            internal ValuePoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            internal int X { get { return this.x; } }

            internal int Y { get { return this.y; } }
        }
    }

    internal static partial class Fundamentals
    {
        internal static void ValueTypeReferenceType()
        {
            Point reference1 = new Point(1, 2);
            Point reference2 = reference1;
            Trace.WriteLine(object.ReferenceEquals(reference1, reference2)); // True

            ValuePoint value1 = new ValuePoint(3, 4);
            ValuePoint value2 = value1;
            Trace.WriteLine(object.ReferenceEquals(value1, value2)); // False

            Point[] referenceArray = new Point[] { new Point(5, 6) };
            ValuePoint[] valueArray = new ValuePoint[] { new ValuePoint(7, 8) };
        }
    }

    internal static partial class Fundamentals
    {
        internal ref struct OnStackOnly { }

#if DEMO
        internal static void Allocation()
        {
            OnStackOnly valueOnStack = new OnStackOnly();
            OnStackOnly[] arrayOnHeap = new OnStackOnly[10]; // Cannot be compiled.
        }

        internal class OnHeapOnly
        {
            private OnStackOnly fieldOnHeap; // Cannot be compiled.
        }

        internal struct OnStackOrHeap
        {
            private OnStackOnly fieldOnStackOrHeap; // Cannot be compiled.
        }
#endif
    }

    internal static partial class Fundamentals
    {
        internal static void Default()
        {
            Point defaultReference = default(Point);
            Trace.WriteLine(defaultReference is null); // True

            ValuePoint defaultValue = default(ValuePoint);
            Trace.WriteLine(defaultValue.X); // 0
            Trace.WriteLine(defaultValue.Y); // 0
        }

        internal static void CompiledDefault()
        {
            Point defaultReference = null;

            ValuePoint defaultValue = new ValuePoint();
        }

        internal static void DefaultLiteralExpression()
        {
            Point defaultReference = default;

            ValuePoint defaultValue = default;
        }
    }

    internal static partial class Fundamentals
    {
        internal static void Dispose(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                Trace.WriteLine(connection.ServerVersion);
                // Work with connection.
            }
            finally
            {
                if ((object)connection != null)
                {
                    ((IDisposable)connection).Dispose();
                }
            }
        }
    }

    internal static partial class Fundamentals
    {
        internal static void Using(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Trace.WriteLine(connection.ServerVersion);
                // Work with connection.
            }
        }
    }

    internal interface IInterface
    {
        void Implicit();

        void Explicit();
    }

    internal class Implementation : IInterface
    {
        public void Implicit() { }

        void IInterface.Explicit() { }
    }

    internal static partial class Fundamentals
    {
        internal static void InterfaceMembers()
        {
            Implementation @object = new Implementation();
            @object.Implicit(); // @object.Explicit(); cannot be compiled.

            IInterface @interface = @object;
            @interface.Implicit();
            @interface.Explicit();
        }
    }

    internal interface IInt32Stack
    {
        void Push(int value);

        int Pop();
    }

    internal class Int32Stack : IInt32Stack
    {
        private int[] values = new int[0];

        public void Push(int value)
        {
            Array.Resize(ref this.values, this.values.Length + 1);
            this.values[this.values.Length - 1] = value;
        }

        public int Pop()
        {
            if (this.values.Length == 0)
            {
                throw new InvalidOperationException("Stack empty.");
            }
            int value = this.values[this.values.Length - 1];
            Array.Resize(ref this.values, this.values.Length - 1);
            return value;
        }
    }

    internal interface IStack<T>
    {
        void Push(T value);

        T Pop();
    }

    internal static partial class Fundamentals
    {
        internal static void Stack()
        {
            Stack<int> stack1 = new Stack<int>();
            stack1.Push(int.MaxValue);
            int value1 = stack1.Pop();

            Stack<string> stack2 = new Stack<string>();
            stack2.Push(Environment.MachineName);
            string value2 = stack2.Pop();

            Stack<Uri> stack3 = new Stack<Uri>();
            stack3.Push(new Uri("https://weblogs.asp.net/dixin"));
            Uri value3 = stack3.Pop();
        }
    }

    internal class Stack<T> : IStack<T>
    {
        private T[] values = new T[0];

        public void Push(T value)
        {
            Array.Resize(ref this.values, this.values.Length + 1);
            this.values[this.values.Length - 1] = value;
        }

        public T Pop()
        {
            if (this.values.Length == 0)
            {
                throw new InvalidOperationException("Stack empty.");
            }
            T value = this.values[this.values.Length - 1];
            Array.Resize(ref this.values, this.values.Length - 1);
            return value;
        }
    }

#if DEMO
    internal class Constraint<T>
    {
        internal void Method()
        {
            T value = null;
        }
    }
#endif

    internal class Constraint<T> where T : class
    {
        internal static void Method()
        {
            T value = null;
        }
    }

    internal partial class Constraints<T1, T2, T3, T4, T5, T6, T7>
        where T1 : struct
        where T2 : class
        where T3 : DbConnection
        where T4 : IDisposable
        where T5 : struct, IComparable, IComparable<T5>
        where T6 : new()
        where T7 : T2, T3, T4, IDisposable, new()
    { }

    internal partial class Constraints<T1, T2, T3, T4, T5, T6, T7>
    {
        internal static void Method(T3 connection) // where T3 : DbConnectiong
        {
            using (connection) // DbConnection implements IDisposable.
            {
                connection.Open(); // DbConnection has Open method.
            }
        }
    }

    internal static partial class Fundamentals
    {
        internal static void CloseType()
        {
            Constraints<bool, object, DbConnection, IDbConnection, int, Exception, SqlConnection> closed = default;
        }

        internal static void Nullable()
        {
            int? nullable = null;
            nullable = 1;
            if (nullable != null)
            {
                int value = (int)nullable;
            }
        }

        internal static void CompiledNullable()
        {
            Nullable<int> nullable1 = new Nullable<int>();
            Nullable<int> nullable2 = new Nullable<int>(int.MaxValue);
            if (nullable2.HasValue)
            {
                int value = nullable2.Value;
            }
        }
    }

    internal partial class Device
    {
        private string name;

        internal string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }

    internal partial class Device
    {
        public string FormattedName
        {
            get { return this.name.ToUpper(); }
        }
    }

    internal partial class Device
    {
        internal decimal Price { get; set; }
    }

    internal partial class CompiledDevice
    {
        [CompilerGenerated]
        private decimal priceBackingField;

        internal decimal Price
        {
            [CompilerGenerated]
            get { return this.priceBackingField; }

            [CompilerGenerated]
            set { this.priceBackingField = value; }
        }

        // Other members.
    }

    internal partial class Category
    {
        internal Category(string name)
        {
            this.Name = name;
        }

        internal string Name { get; /* private set; */ }
    }

    internal partial class CompiledCategory
    {
        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string nameBackingField;

        internal CompiledCategory(string name)
        {
            this.nameBackingField = name;
        }

        internal string Name
        {
            [CompilerGenerated]
            get { return this.nameBackingField; }
        }
    }

    internal partial class Category
    {
        internal Guid Id { get; } = Guid.NewGuid();

        internal string Description { get; set; } = string.Empty;
    }

    internal partial class CompiledCategory
    {
        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Guid idBackingField = Guid.NewGuid();

        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string descriptionBackingField = string.Empty;

        internal Guid Id
        {
            [CompilerGenerated]
            get { return this.idBackingField; }
        }

        internal string Description
        {
            [CompilerGenerated]
            get { return this.descriptionBackingField; }

            [CompilerGenerated]
            set { this.descriptionBackingField = value; }
        }
    }

    internal static partial class Fundamentals
    {
        internal static void SetProperties()
        {
            Device device = new Device();
            device.Name = "Surface Book";
            device.Price = 1349M;
        }

        internal static void ObjectInitializer()
        {
            Device device = new Device() { Name = "Surface Book", Price = 1349M };
        }
    }

    internal class DeviceCollection : IEnumerable
    {
        private Device[] devices = new Device[0];

        internal void Add(Device device)
        {
            Array.Resize(ref this.devices, this.devices.Length + 1);
            this.devices[this.devices.Length - 1] = device;
        }

        public IEnumerator GetEnumerator() // From IEnumerable.
        {
            return this.devices.GetEnumerator();
        }
    }

    internal static partial class Fundamentals
    {
        internal static void CollectionInitializer(Device device1, Device device2)
        {
            DeviceCollection devices = new DeviceCollection { device1, device2 };
        }

        internal static void CompiledCollectionInitializer(Device device1, Device device2)
        {
            DeviceCollection devices = new DeviceCollection();
            devices.Add(device1);
            devices.Add(device2);
        }
    }

    internal class DeviceDictionary
    {
        internal Device this[int id] { set { } }
    }

    internal static partial class Fundamentals
    {
        internal static void IndexInitializer(Device device1, Device device2)
        {
            DeviceDictionary devices = new DeviceDictionary { [10] = device1, [11] = device2 };
        }

        internal static void CompiledIndexInitializer(Device device1, Device device2)
        {
            DeviceDictionary devices = new DeviceDictionary();
            devices[0] = device1;
            devices[1] = device2;
        }
    }

    internal static partial class Fundamentals
    {
        internal partial class Point
        {
            internal static Point Default { get; } = new Point(0, 0);
        }

        internal partial struct ValuePoint
        {
            internal static ValuePoint Default { get; } = new ValuePoint(0, 0);
        }

        internal static void DefaultValueForNull(Point reference, ValuePoint? nullableValue)
        {
            Point point = reference != null ? reference : Point.Default;
            ValuePoint valuePoint = nullableValue != null ? (ValuePoint)nullableValue : ValuePoint.Default;
        }

        internal static void DefaultValueForNullWithNullCoalescing(Point reference, ValuePoint? nullableValue)
        {
            Point point = reference ?? Point.Default;
            ValuePoint valuePoint = nullableValue ?? ValuePoint.Default;
        }

        internal static void NullCheck(Category category, Device[] devices)
        {
            string categoryText = null;
            if (category != null)
            {
                categoryText = category.ToString();
            }
            string firstDeviceName;
            if (devices != null)
            {
                Device firstDevice = devices[0];
                if (firstDevice != null)
                {
                    firstDeviceName = firstDevice.Name;
                }
            }
        }

        internal static void NullCheckWithNullConditional(Category category, Device[] devices)
        {
            string categoryText = category?.ToString();
            string firstDeviceName = devices?[0]?.Name;
        }
    }

    internal partial class Subcategory
    {
        internal Subcategory(string name, Category category)
        {
            this.Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException("name");
            this.Category = category ?? throw new ArgumentNullException("category");
        }

        internal Category Category { get; }

        internal string Name { get; }
    }

    internal static partial class Fundamentals
    {
        internal static void ArgumentCheck(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }

        internal static void NameOf(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        internal static void Log(Device device)
        {
            string message = string.Format("{0}: {1}, {2}", DateTime.Now.ToString("o"), device.Name, device.Price);
            Trace.WriteLine(message);
        }

        internal static void LogWithStringInterpolation(Device device)
        {
            string message = string.Format($"{DateTime.Now.ToString("o")}: {device.Name}, {device.Price}");
            Trace.WriteLine(message);
        }

        internal static void Rethrow(WebClient webClient)
        {
            try
            {
                string html = webClient.DownloadString("http://weblogs.asp.net/dixin");
            }
            catch (WebException exception)
            {
                if ((exception.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Handle exception.
                }
                else
                {
                    throw;
                }
            }
        }

        internal static void ExceptionFilter(WebClient webClient)
        {
            try
            {
                string html = webClient.DownloadString("http://weblogs.asp.net/dixin");
            }
            catch (WebException exception) when ((exception.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.BadRequest)
            {
                // Handle exception.
            }
        }

        internal static void DigitSeparator()
        {
            int value1 = 10_000_000;
            double value2 = 0.123_456_789;

            int value3 = 0b0001_0000; // Binary.
            int value4 = 0b_0000_1000; // Binary.
        }
    }
}

#if DEMO
namespace System
{
    public struct Nullable<T> where T : struct
    {
        private bool hasValue;

        internal T value;

        public Nullable(T value)
        {
            this.value = value;
            this.hasValue = true;
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                {
                    throw new InvalidOperationException("Nullable object    must have a value.");
                }
                return this.value;
            }
        }

        // Other members.
    }
}
#endif
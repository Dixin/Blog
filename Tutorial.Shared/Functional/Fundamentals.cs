#if DEMO
namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    internal sealed class SafeSerializationManager
    {
        internal event EventHandler<SafeSerializationEventArgs> SerializeObjectState;
    }

    [Serializable]
    public class Exception : ISerializable, _Exception // Derived from System.Object.
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

namespace System.Diagnostics.SymbolStore
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public struct SymbolToken
    {
        internal int m_token; // Field.

        public SymbolToken(int val) // Constructor.
        {
            this.m_token = val;
        }

        public bool Equals(SymbolToken obj) // Method.
        {
            return obj.m_token == this.m_token;
        }

        public static bool operator ==(SymbolToken a, SymbolToken b) // Operator.
        {
            return a.Equals(b);
        }

        // Other members.
    }
}

namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    [Serializable]
    public enum DayOfWeek
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

        internal sealed class SqlParameterConverter : ExpandableObjectConverter
        {
        }

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

namespace System
{
    public partial struct Nullable<T> where T : struct
    {
        public T GetValueOrDefault()
        {
            return this.value;
        }

        public T GetValueOrDefault(T defaultValue)
        {
            return this.hasValue ? this.value : defaultValue;
        }

        public override bool Equals(object other)
        {
            if (!this.hasValue)
            {
                return other == null;
            }
            return other != null && this.value.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.hasValue ? this.value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return this.hasValue ? this.value.ToString() : string.Empty;
        }

        public static implicit operator Nullable<T>(T value)
        {
            return new Nullable<T>(value);
        }

        public static explicit operator T(Nullable<T> value)
        {
            return value.Value;
        }
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
    using System.Runtime.CompilerServices;

    internal static partial class Fundamentals
    {
        internal static void Dispose(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                // Use connection object.
                connection.Open();
                Trace.WriteLine(connection.ServerVersion); // 13.00.1708
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
                // Use connection object.
                connection.Open();
                Trace.WriteLine(connection.ServerVersion); // 13.00.1708
            }
        }
    }

    internal interface IInterface
    {
        void Implicit();

        void Explicit();
    }

    internal class Implementations : IInterface
    {
        public void Implicit()
        {
        }

        void IInterface.Explicit()
        {
        }
    }

    internal static partial class Fundamentals
    {
        internal static void InterfaceMembers()
        {
            Implementations @object = new Implementations();
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
        internal void Method()
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
    {
    }

    internal partial class Constraints<T1, T2, T3, T4, T5, T6, T7>
    {
        internal void Method(T3 t3) // where T3 : DbConnectiong
        {
            using (t3) // DbConnection implements IDisposable.
            {
                DbCommand command = t3.CreateCommand(); // DbConnection has CreateCommand method.
            }
        }
    }

    internal static partial class Fundamentals
    {
        internal static void CloseType()
        {
            Constraints<bool, object, DbConnection, IDbConnection, int, Exception, SqlConnection> value = null;
        }

        internal static void Nullable()
        {
            int? nullable1 = null;
            int? nullable2 = int.MaxValue;
            if (nullable2 != null)
            {
                int value = nullable2.Value;
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
        public override string ToString()
        {
            return this.Name;
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

        internal static void AddToCollections(Device device1, Device device2)
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
            DeviceDictionary devices = new DeviceDictionary { [0] = device1, [1] = device2 };
        }

        internal static void SetIndexer(Device device1, Device device2)
        {
            DeviceDictionary devices = new DeviceDictionary();
            devices[0] = device1;
            devices[1] = device2;
        }
    }

    internal static partial class Fundamentals
    {
        internal static void DefaultValueForNull(Uri nullable)
        {
            Uri uri;
            if ((object)nullable != null)
            {
                uri = nullable;
            }
            else
            {
                uri = new Uri("https://weblogs.asp.net/dixin"); // Default value for null.
            }
            // uri is not null.
        }

        internal static void NullCoalescing(Uri nullable)
        {
            Uri uri = nullable ?? new Uri("https://weblogs.asp.net/dixin");
            // uri is not null.
        }

        internal static void MemberAccess(Uri nullableValue)
        {
            string result;
            if ((object)nullableValue != null)
            {
                result = nullableValue.ToString();
            }
            else
            {
                result = null;
            }
        }

        internal static void NullConditional(Uri nullableValue)
        {
            string result = nullableValue?.ToString();
        }

        internal static void ArgumentCheck(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
        }

        internal static void NameOf(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
        }

        internal static int AddWithLog(int value1, int value2)
        {
            int sum = value1 + value2;
            Trace.WriteLine(string.Format("{0}: {1} + {2} => {3}", DateTime.Now.ToString("o"), value1, value2, sum));
            return sum;
        }

        internal static int StringInterpolation(int value1, int value2)
        {
            int sum = value1 + value2;
            Trace.WriteLine($"{DateTime.Now.ToString("o")}: {value1} + {value2} => {sum}");
            return sum;
        }
    }
}

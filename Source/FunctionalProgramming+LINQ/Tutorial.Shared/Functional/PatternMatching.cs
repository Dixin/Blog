namespace Tutorial.Functional
{
    using System;

    internal static partial class PatternMatching
    {
        internal static void IsConstantValue(object @object)
        {
            // Type test:
            bool test1 = @object is string;
            // Constant value test:
            bool test5 = @object is null; // Compiled to: @object == null
            bool test2 = @object is int.MinValue; // Compiled to: object.Equals(int.MinValue, @object)
            bool test3 = @object is DayOfWeek.Monday; // Compiled to: object.Equals(DayOfWeek.Monday, @object)
            bool test4 = @object is "test"; // Compiled to: object.Equals("test", @object)

#if DEMO
            // https://github.com/dotnet/roslyn/issues/25450
            // https://github.com/dotnet/roslyn/issues/23499
            bool test6 = @object is default; // Compiled to: @object == mull
#endif
        }
    }

    internal static partial class PatternMatching
    {
        internal static void CompiledIsConstantValue(object @object)
        {
            if (object.Equals(int.MinValue, @object))
            {
                @object.WriteLine();
            }
            if (object.Equals(null, @object))
            {
                @object.WriteLine();
            }
            if (object.Equals(DayOfWeek.Monday, @object))
            {
                @object.WriteLine();
            }
        }

        internal static void IsReferenceType(object @object)
        {
            if (@object is Uri uri)
            {
                uri.AbsolutePath.WriteLine();
            }
        }

        internal static void CompiledIsReferenceType(object @object)
        {
            Uri uri = @object as Uri;
            if (uri != null)
            {
                uri.AbsolutePath.WriteLine();
            }
        }

        internal static void IsValueType(object @object)
        {
            if (@object is DateTime dateTime)
            {
                dateTime.ToString("o").WriteLine();
            }
        }

        internal static void CompiledIsValueType(object @object)
        {
            DateTime? nullableDateTime = @object as DateTime?;
            DateTime dateTime = nullableDateTime.GetValueOrDefault();
            if (nullableDateTime.HasValue)
            {
                dateTime.ToString("o").WriteLine();
            }
        }

        internal static void IsWithCondition(object @object)
        {
            if (@object is string @string && TimeSpan.TryParse(@string, out TimeSpan timeSpan))
            {
                timeSpan.TotalMilliseconds.WriteLine();
            }
        }

        internal static void OpenType<T1, T2>(object @object, T1 open1)
        {
            if (@object is T1 open) { }
            if (open1 is Uri uri) { }
            if (open1 is T2 open2) { }
        }

        internal static void CompiledIsWithCondition(object @object)
        {
            string @string = @object as string;
            if (@string != null && TimeSpan.TryParse(@string, out TimeSpan timeSpan))
            {
                timeSpan.TotalMilliseconds.WriteLine();
            }
        }
    }

#if DEMO
    internal partial class Data : IEquatable<Data>
    {
        public override bool Equals(object obj) => 
            obj is Data data && this.Equals(data);
    }
#endif

    internal static partial class PatternMatching
    {
        internal static void IsAnyType(object @object)
        {
            if (@object is var match)
            {
                object.ReferenceEquals(@object, match).WriteLine();
            }
        }

        internal static void CompiledIsAnyType(object @object)
        {
            object match = @object;
            if (true)
            {
                object.ReferenceEquals(@object, match).WriteLine();
            }
        }

        internal static DateTime ToDateTime(object @object)
        {
            switch (@object)
            {
                // Match constant @object.
                case null:
                    throw new ArgumentNullException(nameof(@object));
                // Match value type.
                case DateTime dateTIme:
                    return dateTIme;
                // Match value type with condition.
                case long ticks when ticks >= 0:
                    return new DateTime(ticks);
                // Match reference type with condition.
                case string @string when DateTime.TryParse(@string, out DateTime dateTime):
                    return dateTime;
                // Match reference type with condition.
                case int[] date when date.Length == 3 && date[0] > 0 && date[1] > 0 && date[2] > 0:
                    return new DateTime(year: date[0], month: date[1], day: date[2]);
                // Match reference type.
                case IConvertible convertible:
                    return convertible.ToDateTime(provider: null);
                case var _: // default:
                    throw new ArgumentOutOfRangeException(nameof(@object));
            }
        }

        internal static DateTime CompiledToDateTime(object @object)
        {
            // case null:
            if (@object == null)
            {
                throw new ArgumentNullException("@object");
            }

            // case DateTime dateTIme:
            DateTime? nullableDateTime = @object as DateTime?;
            DateTime dateTime = nullableDateTime.GetValueOrDefault();
            if (nullableDateTime.HasValue)
            {
                return dateTime;
            }

            // case long ticks
            long? nullableInt64 = @object as long?;
            long ticks = nullableInt64.GetValueOrDefault();
            // when ticks >= 0:
            if (nullableInt64.HasValue && ticks >= 0L)
            {
                return new DateTime(ticks);
            }

            // case string text 
            string @string = @object as string;
            // when DateTime.TryParse(text, out DateTime dateTime):
            if (@string != null && DateTime.TryParse(@string, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            // case int[] date
            int[] date = @object as int[];
            // when date.Length == 3 && date[0] >= 0 && date[1] >= 0 && date[2] >= 0:
            if (date != null && date.Length == 3 && date[0] >= 0 && date[1] >= 0 && date[2] >= 0)
            {
                return new DateTime(date[0], date[1], date[2]);
            }

            // case IConvertible convertible:
            IConvertible convertible = @object as IConvertible;
            if (convertible != null)
            {
                return convertible.ToDateTime(null);
            }

            // case var _:
            // or
            // default:
            throw new ArgumentOutOfRangeException("@object");
        }
    }
}

#if DEMO
namespace System
{
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Object
    {
        public static bool Equals(object objA, object objB) =>
            objA == objB || (objA != null && objB != null && objA.Equals(objB));

        public virtual bool Equals(object obj) =>
            RuntimeHelpers.Equals(this, obj);

        // Other members.
    }
}
#endif
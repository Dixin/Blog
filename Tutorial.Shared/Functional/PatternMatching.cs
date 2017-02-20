namespace Tutorial.Functional
{
    using System;

    internal static partial class PatternMatching
    {
        internal static void IsConstantValue(object @object)
        {
            if (@object is int.MinValue)
            {
                @object.WriteLine();
            }
            if (@object is null)
            {
                @object.WriteLine();
            }
            if (@object is DayOfWeek.Monday)
            {
                @object.WriteLine();
            }
        }
    }

    internal static partial class PatternMatching
    {
        internal static void CompiledIsConstantValue(object @object)
        {
            if (object.Equals(@object, int.MinValue))
            {
                @object.WriteLine();
            }
            if (object.Equals(@object, null))
            {
                @object.WriteLine();
            }
            if (object.Equals(@object, DayOfWeek.Monday))
            {
                @object.WriteLine();
            }
        }

        internal static void IsReferenceType(object @object)
        {
            if (@object is Uri uri)
            {
                uri.AbsoluteUri.WriteLine();
            }
        }

        internal static void CompiledIsReferenceType(object @object)
        {
            Uri uri = @object as Uri;
            if (uri != null)
            {
                uri.AbsoluteUri.WriteLine();
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
            if (nullableDateTime != null) // if (nullableDateTime.HasValue)
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

        internal static void CompiledIsWithConditio(object @object)
        {
            string @string = @object as string;
            if (@string != null && TimeSpan.TryParse(@string, out TimeSpan timeSpan))
            {
                timeSpan.TotalMilliseconds.WriteLine();
            }
        }

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
                // Match @object type.
                case DateTime dateTIme:
                    return dateTIme;
                // Match @object type with condition.
                case long ticks when ticks >= 0:
                    return new DateTime(ticks);
                // Match reference type with condition.
                case string @string when DateTime.TryParse(@string, out DateTime dateTime):
                    return dateTime;
                // Match reference type with condition.
                case int[] date when date.Length == 3 && date[0] >= 0 && date[1] >= 0 && date[2] >= 0:
                    return new DateTime(year: date[0], month: date[1], day: date[2]);
                // Match reference type.
                case IConvertible convertible:
                    return convertible.ToDateTime(null);
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
            if (nullableDateTime != null)
            {
                return dateTime;
            }

            // case long ticks
            long? nullableInt64 = @object as long?;
            long ticks = nullableInt64.GetValueOrDefault();
            // when ticks >= 0:
            if (nullableInt64 != null && ticks >= 0L)
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

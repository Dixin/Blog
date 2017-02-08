namespace Dixin.Linq.CSharp
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class Point
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

    internal struct ValuePoint
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
        }
    }

    internal static partial class Fundamentals
    {
        internal static void Default()
        {
            Point defaultReference = default(Point);
            (defaultReference is null).WriteLine(); // True.

            ValuePoint defaultValue = default(ValuePoint);
            Trace.WriteLine(defaultValue.X); // 0
            Trace.WriteLine(defaultValue.Y); // 0
        }
    }

    internal static partial class Functions
    {
        internal static void PassByValue(Uri reference, int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallPassByValue()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            PassByValue(reference, value);
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }

        internal static void PassByReference(ref Uri reference, ref int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallPassByReference()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            PassByReference(ref reference, ref value);
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void Output(out Uri reference, out int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallOutput()
        {
            Uri reference;
            int value;
            Output(out reference, out value);
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void OutVariable()
        {
            Output(out Uri reference, out int value);
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static int Sum(params int[] values)
        {
            int sum = 0;
            foreach (int value in values)
            {
                sum += value;
            }
            return sum;
        }

#if DEMO
        internal static int CompiledSum([ParamArray] int[] values)
        {
            int sum = 0;
            foreach (int value in values)
            {
                sum += value;
            }
            return sum;
        }
#endif

        internal static void CallSum(int[] array)
        {
            int sum1 = Sum();
            int sum2 = Sum(1);
            int sum3 = Sum(1, 2, 3, 4, 5);
            int sum4 = Sum(array);
        }

        internal static void CompiledCallSum(int[] array)
        {
            int sum1 = Sum(Array.Empty<int>());
            int sum2 = Sum(new int[] { 1 });
            int sum3 = Sum(new int[] { 1, 2, 3, 4, 5 });
            int sum4 = Sum(array);
        }

        internal static void Named()
        {
            PassByValue(reference: null, value: 1);
            PassByValue(value: 1, reference: null);
            PassByValue(null, value: 1);
        }

        internal static void Optional(
            bool required, int optional1 = 1, string optional2 = "Default value.", Uri optional3 = null)
        {
        }

        internal static void CallOptional()
        {
            Optional(true);
            Optional(true, 10);
            Optional(true, 10, string.Empty);
            Optional(true, optional2: string.Empty);
            Optional(optional3: new Uri("https://weblogs.asp.net/dixin"), required: false, optional1: 10);
        }

        internal static void CompiledCallOptional()
        {
            Optional(true);
            Optional(true, 10);
            Optional(true, 10, string.Empty);
            Optional(true, optional2: string.Empty);
            Optional(optional3: new Uri("https://weblogs.asp.net/dixin"), required: false, optional1: 10);
        }

        internal static void TraceWithCaller(
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Trace.WriteLine($"[{callerMemberName}, {callerFilePath}, line {callerLineNumber}]: {message}");
        }

        internal static void CallTraceWithCaller()
        {
            TraceWithCaller("Message.");
            // [CallTraceWithCaller, D:\Data\GitHub\CodeSnippets\Tutorial.Shared\Linq\CSharp\Parameters.cs, line 189]: Message.
        }

        internal static void CompiledCallTraceWithCaller()
        {
            TraceWithCaller("Message.", "CompiledCallTraceWithCaller", @"D:\Data\GitHub\CodeSnippets\Tutorial.Shared\Linq\CSharp\Parameters.cs", 189);
        }
    }
}

namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

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
            PassByValue(reference, value); // Copied.
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }
    }

    internal static partial class Functions
    {
        internal static void PassByReference(ref Uri reference, ref int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallPassByReference()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            PassByReference(ref reference, ref value); // Not copied.
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void PassByReadOnlyReference(in Uri reference, in int value)
        {
        }

        internal static void CallPassByReadOnlyReference()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            PassByReadOnlyReference(in reference, in value); // Not copied.
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }

        internal static void CallPassByReadOnlyReference2()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            PassByReadOnlyReference(reference, value); // Not copied.
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }

#if DEMO
        internal static void PassByReadOnlyReference(in Uri reference, in int value)
        {
            reference = new Uri("https://flickr.com/dixin"); // Cannot be compiled.
            value = 10; // Cannot be compiled.
        }
#endif

        internal static bool Output(out Uri reference, out int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
            return false;
        }

        internal static void CallOutput()
        {
            Uri reference;
            int value;
            Output(out reference, out value); // Not copied.
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void OutVariable()
        {
            Output(out Uri reference, out int value);
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void Discard()
        {
            bool result = Output(out _, out _);
            _ = Output(out _, out _);
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

        internal static void ParameterArray(bool required1, int required2, params string[] optional) { }

        internal static void PositionalAndNamed()
        {
            PassByValue(null, 0); // Positional arguments.
            PassByValue(reference: null, value: 0); // Named arguments.
            PassByValue(value: 0, reference: null); // Named arguments.
            PassByValue(null, value: 0); // Positional argument followed by named argument.
            PassByValue(reference: null, 0); // Named argument followed by positional argument.
        }

        internal static void CompiledPositionalAndNamed()
        {
            PassByValue(null, 0);
            PassByValue(null, 0);
            PassByValue(null, 0);
            PassByValue(null, 0);
            PassByValue(null, 0);
        }

        internal static void NamedEvaluation()
        {
            PassByValue(reference: GetUri(), value: GetInt32()); // Call GetUri then GetInt32.
            PassByValue(value: GetInt32(), reference: GetUri()); // Call GetInt32 then GetUri.
        }

        internal static Uri GetUri() { return default; }

        internal static int GetInt32() { return default; }

        internal static void CompiledNamedArgument()
        {
            PassByValue(GetUri(), GetInt32()); // Call GetUri then GetInt32.
            int value = GetInt32(); // Call GetInt32 then GetUri.
            PassByValue(GetUri(), value);
        }

        internal static void Named()
        {
            UnicodeEncoding unicodeEncoding1 = new UnicodeEncoding(true, true, true);
            UnicodeEncoding unicodeEncoding2 = new UnicodeEncoding(
                bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true);
        }

        internal static void Optional(
            bool required1, char required2,
            int optional1 = int.MaxValue, string optional2 = "Default value.",
            Uri optional3 = null, Guid optional4 = new Guid(),
            Uri optional5 = default, Guid optional6 = default) { }

        internal static void CallOptional()
        {
            Optional(true, '@');
            Optional(true, '@', 1);
            Optional(true, '@', 1, string.Empty);
            Optional(true, '@', optional2: string.Empty);
            Optional(
                optional6: Guid.NewGuid(), optional3: GetUri(), required1: false, optional1: GetInt32(), 
                required2: Convert.ToChar(64)); // Call Guid.NewGuid, then GetUri, then GetInt32, then Convert.ToChar.
        }

        internal static void CompiledCallOptional()
        {
            Optional(true, '@', 1, "Default value.", null, new Guid(), null, new Guid());
            Optional(true, '@', 1, "Default value.", null, new Guid(), null, new Guid());
            Optional(true, '@', 1, string.Empty, null, new Guid(), null, new Guid());
            Optional(true, '@', 1, string.Empty, null, new Guid(), null, new Guid());
            Guid optional6 = Guid.NewGuid(); // Call Guid.NewGuid, then GetUri, then GetInt32, then Convert.ToChar.
            Uri optional3 = GetUri();
            int optional1 = GetInt32();
            Optional(false, Convert.ToChar(64), optional1, "Default value.", optional3);
        }

        internal static void TraceWithCaller(
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Trace.WriteLine($"[{callerMemberName}, {callerFilePath}, {callerLineNumber}]: {message}");
        }

        internal static void CallTraceWithCaller()
        {
            TraceWithCaller("Message.");
            // [CallTraceWithCaller, D:\Data\GitHub\CodeSnippets\Tutorial.Shared\Linq\CSharp\Parameters.cs, 242]: Message.
        }

        internal static void CompiledCallTraceWithCaller()
        {
            TraceWithCaller("Message.", "CompiledCallTraceWithCaller", @"D:\Data\GitHub\CodeSnippets\Tutorial.Shared\Linq\CSharp\Parameters.cs", 242);
        }

        internal static int LastValue(int[] values)
        {
            int length = values.Length;
            if (length > 0)
            {
                return values[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(values));
        }

        internal static Uri LastReference(Uri[] references)
        {
            int length = references.Length;
            if (length > 0)
            {
                return references[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(references));
        }

        internal static void ReturnByValue()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            int lastValue = LastValue(values); // Copied.
            lastValue = 10;
            Trace.WriteLine(values[values.Length - 1]); // 4

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            Uri lastReference = LastReference(references); // Copied.
            lastReference = new Uri("https://flickr.com/dixin");
            Trace.WriteLine(references[references.Length - 1]); // https://weblogs.asp.net/dixin
        }

        internal static ref int RefLastValue(int[] values)
        {
            int length = values.Length;
            if (length > 0)
            {
                return ref values[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(values));
        }

        internal static ref Uri RefLastReference(Uri[] references)
        {
            int length = references.Length;
            if (length > 0)
            {
                return ref references[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(references));
        }

        internal static void ReturnByReference()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            ref int lastValue = ref RefLastValue(values); // Not copied.
            lastValue = 10;
            Trace.WriteLine(values[values.Length - 1]); // 10

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            ref Uri lastReference = ref RefLastReference(references); // Not copied.
            lastReference = new Uri("https://flickr.com/dixin");
            Trace.WriteLine(references[references.Length - 1]); // https://flickr.com/dixin
        }

        internal static ref readonly int RefReadOnlyLastValue(int[] values)
        {
            int length = values.Length;
            if (length > 0)
            {
                return ref values[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(values));
        }

        internal static ref readonly Uri RefReadOnlyLastReference(Uri[] references)
        {
            int length = references.Length;
            if (length > 0)
            {
                return ref references[length - 1];
            }
            throw new ArgumentException("Array is empty.", nameof(references));
        }

#if DEMO
        internal static void ReturnByRedOnlyReference()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            ref readonly int lastValue = ref RefReadOnlyLastValue(values); // Not copied.
            lastValue = 10; // Cannot be compiled.
            Trace.WriteLine(values[values.Length - 1]); // 10

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            ref readonly Uri lastReference = ref RefReadOnlyLastReference(references); // Not copied.
            lastReference = new Uri("https://flickr.com/dixin"); // Cannot be compiled.
            Trace.WriteLine(references[references.Length - 1]); // https://flickr.com/dixin
        }
#endif
    }
}

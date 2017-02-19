namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    // () -> void
    internal delegate void FuncToVoid();

    // string -> void
    internal delegate void FuncStringToVoid(string @string);

    // () -> int
    internal delegate int FuncToInt32();

    // (string, int) -> int
    internal delegate int FuncStringInt32ToInt32(string @string, int int32);

    // string -> string
    internal delegate string FuncStringToString(string @string);

    // () -> bool
    internal delegate bool FuncToBoolean();

    // () -> string
    internal delegate string FuncToString();

    // () -> object
    internal delegate object FuncToObject();

#if DEMO
    // () -> TResult
    internal delegate TResult Func<TResult>();

    // (T1, T2) -> TResult
    internal delegate TResult Func<T1, T2, TResult>(T1 value1, T2 value2);
#endif

    // (T, T) -> int
    internal delegate int NewComparison<in T>(T x, T y);

    // (string, string) -> TResult
    internal delegate TResult FuncStringString<TResult>(string value1, string value2);

    // (T1, T2) -> int
    internal delegate int FuncToInt32<T1, T2>(T1 value1, T2 value2);

    // (string, string) -> int
    internal delegate int FuncStringStringToInt32(string value1, string value2);

    internal static partial class Functions
    {
        internal static void Constructor()
        {
            Func<int, int, int> func = new Func<int, int, int>(Math.Max);
            int result = func(1, 2);
            Trace.WriteLine(result); // 2
        }
    }

    internal static partial class Functions
    {
        internal static void Instantiate()
        {
            Func<int, int, int> func = Math.Max;
            int result = func(1, 2);
            Trace.WriteLine(result); // 2
        }
    }

#if DEMO
    public sealed class CompiledFunc<in T1, in T2, out TResult> : MulticastDelegate
    {
        public CompiledFunc(object @object, IntPtr method)
        {
        }

        public virtual TResult Invoke(T1 arg1, T2 arg2)
        {
        }

        public virtual IAsyncResult BeginInvoke(T1 arg1, T2 arg2, AsyncCallback callback, object @object)
        {
        }

        public virtual void EndInvoke(IAsyncResult result)
        {
        }
    }

    internal static partial class Functions
    {
        internal static void CompiledInstantiate()
        {
            CompiledFunc<int, int, int> func = new CompiledFunc<int, int, int>(null, Math.Max);
            int result = func.Invoke(1, 2);
            Trace.WriteLine(result); // 2
        }
    }
#endif

    internal static partial class Functions
    {
        internal static void Invoke(Action<int> action)
        {
            action?.Invoke(0); // if (action != null) { action(); }
        }

        internal static void TraceAllTextAsync(string path)
        {
            Func<string, string> func = File.ReadAllText;
            func.BeginInvoke(path, TraceAllTextCallback, func);
        }

        internal static void TraceAllTextCallback(IAsyncResult asyncResult)
        {
            Func<string, string> func = (Func<string, string>)asyncResult.AsyncState;
            string text = func.EndInvoke(asyncResult);
            Trace.WriteLine(text);
        }

        internal static async Task TraceAllTextAsyncTask(string path)
        {
            string text = await Task.Run(() => File.ReadAllText(path));
            Trace.WriteLine(text);
        }

        internal static void Static()
        {
            Func<int, int, int> func1 = Math.Max; // new Func<int, int, int>(Math.Max);
            int result1 = func1(1, 2); // func1.Invoke(1, 2);;
            Trace.WriteLine(func1.Target == null); // True
            MethodInfo method1 = func1.GetMethodInfo();
            Trace.WriteLine($"{method1.DeclaringType}: {method1}"); // System.Math: Int32 Max(Int32, Int32)

            Func<int, int, int> func2 = Math.Max; // new Func<int, int, int>(Math.Max);
            Trace.WriteLine(object.ReferenceEquals(func1, func2)); // False
            Trace.WriteLine(func1 == func2); // True
        }

        internal static void Instance()
        {
            object object1 = new object();
            Func<object, bool> func1 = object1.Equals; // new Func<object, bool>(object1.Equals);
            Trace.WriteLine(ReferenceEquals(func1.Target, object1)); // True
            MethodInfo method2 = func1.GetMethodInfo();
            Trace.WriteLine($"{method2.DeclaringType}: {method2}"); // System.Object: Boolean Equals(System.Object)

            object object2 = new object();
            Func<object, bool> func2 = object2.Equals; // new Func<object, bool>(object2.Equals);
            Trace.WriteLine(ReferenceEquals(func2.Target, object2)); // True
            Trace.WriteLine(object.ReferenceEquals(func1, func2)); // False
            Trace.WriteLine(func1 == func2); // False

            Func<object, bool> func3 = object1.Equals; // new Func<object, bool>(object1.Equals);
            Trace.WriteLine(object.ReferenceEquals(func1, func3)); // False
            Trace.WriteLine(func1 == func3); // True
        }

        internal static int Add(int a, int b)
        {
            Trace.WriteLine(nameof(Add));
            return a + b;
        }

        internal static int Subtract(int a, int b)
        {
            Trace.WriteLine(nameof(Subtract));
            return a - b;
        }

        internal static int Multiply(int a, int b)
        {
            Trace.WriteLine(nameof(Multiply));
            return a * b;
        }

        internal static int Divide(int a, int b)
        {
            Trace.WriteLine(nameof(Divide));
            return a / b;
        }

        internal static void FunctionGroup2()
        {
            Func<int, int, int> addFunction = Add;
            Func<int, int, int> subtractFunction = Subtract;
            Func<int, int, int> functionGroup1 = addFunction + subtractFunction;
            functionGroup1 += Multiply;
            functionGroup1 += Divide;
            int lastResult1 = functionGroup1(6, 2); // Add Subtract Multiply Divide
            Trace.WriteLine(lastResult1); // 3

            Func<int, int, int> functionGroup2 = functionGroup1 - addFunction;
            functionGroup2 -= Divide;
            int lastResult2 = functionGroup2(6, 2); // Subtract Multiply
            Trace.WriteLine(lastResult2); // 12

            Func<int, int, int> functionGroup3 = functionGroup1 - functionGroup2 + addFunction;
            int lastResult3 = functionGroup3(6, 2); // Add Divide Add
            Trace.WriteLine(lastResult3); // 8
        }

        internal static string A()
        {
            Trace.WriteLine(nameof(A));
            return nameof(A);
        }

        internal static string B()
        {
            Trace.WriteLine(nameof(B));
            return nameof(B);
        }

        internal static string C()
        {
            Trace.WriteLine(nameof(C));
            return nameof(C);
        }

        internal static string D()
        {
            Trace.WriteLine(nameof(D));
            return nameof(D);
        }

        internal static void FunctionGroup()
        {
            Func<string> a = A;
            Func<string> b = B;
            Func<string> functionGroup1 = a + b;
            functionGroup1 += C;
            functionGroup1 += D;
            string lastResult1 = functionGroup1(); // A B C D
            Trace.WriteLine(lastResult1); // D

            Func<string> functionGroup2 = functionGroup1 - a;
            functionGroup2 -= D;
            string lastResult2 = functionGroup2(); // B C
            Trace.WriteLine(lastResult2); // C

            Func<string> functionGroup3 = functionGroup1 - functionGroup2 + a;
            string lastResult3 = functionGroup3(); // A D A
            Trace.WriteLine(lastResult3); // 8
        }

        internal static void CompiledFunctionGroup()
        {
            Func<string> a = A;
            Func<string> b = B;
            Func<string> functionGroup1 = (Func<string>)Delegate.Combine(a, b); // = A + B;
            functionGroup1 = (Func<string>)Delegate.Combine(functionGroup1, new Func<string>(C)); // += C;
            functionGroup1 = (Func<string>)Delegate.Combine(functionGroup1, new Func<string>(D)); // += D;
            string lastResult1 = functionGroup1.Invoke(); // A B C D
            Trace.WriteLine(lastResult1); // D

            Func<string> functionGroup2 = (Func<string>)Delegate.Remove(functionGroup1, a); // = functionGroup1 - A;
            functionGroup2 = (Func<string>)Delegate.Remove(functionGroup2, new Func<string>(D)); // -= D;
            string lastResult2 = functionGroup2.Invoke(); // B C
            Trace.WriteLine(lastResult2); // C

            Func<string> functionGroup3 = (Func<string>)Delegate.Combine( // = functionGroup1 - functionGroup2 + A;
                (Func<string>)Delegate.Remove(functionGroup1, functionGroup2), a);
            string lastResult3 = functionGroup3(); // A D A
            Trace.WriteLine(lastResult3); // A
        }
    }
}

#if DEMO
namespace System.Diagnostics
{
    public sealed class Trace
    {
        public static void Close();

        public static void Flush();

        public static void Indent();
    }
}

namespace System.Diagnostics
{
    public sealed class Trace
    {
        public static void TraceInformation(string message);

        public static void Write(string message);

        public static void WriteLine(string message);
    }
}

namespace System.Runtime.InteropServices
{
    public static class Marshal
    {
        public static int GetExceptionCode();

        public static int GetHRForLastWin32Error();

        public static int GetLastWin32Error();
    }
}

namespace System.Globalization
{
    public static class CharUnicodeInfo
    {
        public static int GetDecimalDigitValue(string s, int index);

        public static int GetDigitValue(string s, int index);
    }
}

namespace System
{
    public static class Math
    {
        // (double, double) -> double
        public static double Log(double a, double newBase);

        // (int, int) -> int
        public static int Max(int val1, int val2);

        // (double, int) -> double
        public static double Round(double value, int digits);

        // (decimal, MidpointRounding) -> decimal
        public static decimal Round(decimal d, MidpointRounding mode);
    }
}

namespace System
{
    // (T, T) -> int
    public delegate int Comparison<in T>(T x, T y);
}

namespace System.Threading
{
    using System.Runtime.InteropServices;

    // object -> void
    public delegate void SendOrPostCallback(object state);

    // object -> void
    public delegate void ContextCallback(object state);

    // object -> void
    public delegate void ParameterizedThreadStart(object obj);

    // object -> void
    public delegate void WaitCallback(object state);

    // object -> void
    public delegate void TimerCallback(object state);
}

namespace System
{
    // () -> TResult
    public delegate TResult Func<out TResult>();

    // T -> TResult
    public delegate TResult Func<in T, out TResult>(T arg);

    // (T1, T2) -> TResult
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    // (T1, T2, T3) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);

    // (T1, T2, T3, T4) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // ...

    // (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
}

namespace System
{
    // () -> void
    public delegate void Action();

    // T -> void
    public delegate void Action<in T>(T obj);

    // (T1, T2) -> void
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    // (T1, T2, T3) -> void
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    // (T1, T2, T3, T4) -> void
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // ...

    // (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16) -> void
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
}

namespace System
{
    public abstract class Delegate
    {
        public object Target { get; }

        public static bool operator ==(Delegate d1, Delegate d2);

        public static bool operator !=(Delegate d1, Delegate d2);

        // Other members.
    }
}

namespace System.Reflection
{
    public static class RuntimeReflectionExtensions
    {
        public static MethodInfo GetMethodInfo(this Delegate del);
    }
}

namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    [DefaultMember("Chars")]
    public sealed class String : IEnumerable<char>, IEnumerable, IComparable, IComparable<String>, IConvertible, IEquatable<String>
    {
        public static bool IsNullOrEmpty(String value);

        public static bool IsNullOrWhiteSpace(String value);

        public bool Contains(String value);

        public bool Equals(String value);

        public bool StartsWith(String value);

        public bool EndsWith(String value);
    }
}
#endif

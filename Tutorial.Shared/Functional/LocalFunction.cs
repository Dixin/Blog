namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static partial class Functions
    {
        internal static void MethodWithLocalFunction()
        {
            void LocalFunction() // Define local function.
            {
                Trace.WriteLine(nameof(LocalFunction));
            }
            LocalFunction(); // Call local function.
        }

        internal static int PropertyWithLocalFunction
        {
            get
            {
                LocalFunction(); // Call local function.
                void LocalFunction() // Define local function.
                {
                    Trace.WriteLine(nameof(LocalFunction));
                }
                LocalFunction(); // Call local function.
                return 0;
            }
        }
    }

    internal static partial class Functions
    {
#if DEMO
// Cannot be compiled.
        internal static void LocalFunctionOverload()
        {
            void LocalFunction()
            {
            }
            void LocalFunction(int int32) // Cannot be the same name.
            {
            }
        }
#endif

        internal static int BinarySearch<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            return BinarySearch(source, value, comparer ?? Comparer<T>.Default, 0, source.Count - 1);
        }

        private static int BinarySearch<T>(IList<T> source, T value, IComparer<T> comparer, int startIndex, int endIndex)
        {
            if (startIndex > endIndex)
            {
                return -1;
            }
            int middleIndex = startIndex + (endIndex - startIndex) / 2;
            int compare = comparer.Compare(source[middleIndex], value);
            if (compare == 0)
            {
                return middleIndex;
            }
            return compare > 0
                ? BinarySearch(source, value, comparer, startIndex, middleIndex - 1)
                : BinarySearch(source, value, comparer, middleIndex + 1, endIndex);
        }

        internal static int BinarySearchWithLocalFunction<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            int BinarySearch(
                IList<T> localSource, T localValue, IComparer<T> localComparer, int startIndex, int endIndex)
            {
                if (startIndex > endIndex)
                {
                    return -1;
                }
                int middleIndex = startIndex + (endIndex - startIndex) / 2;
                int compare = localComparer.Compare(localSource[middleIndex], localValue);
                if (compare == 0)
                {
                    return middleIndex;
                }
                return compare > 0
                    ? BinarySearch(localSource, localValue, localComparer, startIndex, middleIndex - 1)
                    : BinarySearch(localSource, localValue, localComparer, middleIndex + 1, endIndex);
            }

            return BinarySearch(source, value, comparer ?? Comparer<T>.Default, 0, source.Count - 1);
        }

        internal static int BinarySearchWithClosure<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            int BinarySearch(int startIndex, int endIndex)
            {
                if (startIndex > endIndex)
                {
                    return -1;
                }
                int middleIndex = startIndex + (endIndex - startIndex) / 2;
                int compare = comparer.Compare(source[middleIndex], value);
                if (compare == 0)
                {
                    return middleIndex;
                }
                return compare > 0
                    ? BinarySearch(startIndex, middleIndex - 1)
                    : BinarySearch(middleIndex + 1, endIndex);
            }

            comparer = comparer ?? Comparer<T>.Default;
            return BinarySearch(0, source.Count - 1);
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        private struct Display1<T>
        {
            public IComparer<T> Comparer;

            public IList<T> Source;

            public T Value;
        }

        [CompilerGenerated]
        private static int CompiledLocalBinarySearch<T>(int startIndex, int endIndex, ref Display1<T> display)
        {
            if (startIndex > endIndex)
            {
                return -1;
            }
            int middleIndex = startIndex + (endIndex - startIndex) / 2;
            int compare = display.Comparer.Compare(display.Source[middleIndex], display.Value);
            if (compare == 0)
            {
                return middleIndex;
            }
            return compare <= 0
                ? CompiledLocalBinarySearch(middleIndex + 1, endIndex, ref display)
                : CompiledLocalBinarySearch(startIndex, middleIndex - 1, ref display);
        }

        internal static int CompiledBinarySearchWithClosure<T>(IList<T> source, T value, IComparer<T> comparer = null)
        {
            Display1<T> display = new Display1<T>()
            {
                Source = source,
                Value = value,
                Comparer = comparer
            };
            return CompiledLocalBinarySearch(0, source.Count - 1, ref display);
        }

        internal static void FunctionMember()
        {
            void LocalFunction()
            {
                void LocalFunctionInLocalFunction()
                {
                }
            }
        }

        internal static Action AnonymousFunctionWithLocalFunction()
        {
            return () =>
            {
                void LocalFunction()
                {
                }
                LocalFunction();
            };
        }

        internal class Display
        {
            int outer = 1; // Otside the scope of method Add.

            internal void Add()
            {
                int local = 2; // Inside the scope of method Add.
                Trace.WriteLine(local + outer); // this.outer field.
            }
        }

        internal static void LocalFunctionClosure2()
        {
            int outer = 1; // Outside the scope of function Add.
            void Add()
            {
                int local = 2; // Inside the scope of function Add.
                Trace.WriteLine(local + outer);
            }
            Add(); // 3
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        private struct Display0
        {
            public int Outer;
        }

        private static void Add(ref Display0 display)
        {
            int local = 2;
            Trace.WriteLine(local + display.Outer);
        }

        internal static void CompiledLocalFunctionClosure()
        {
            int outer = 1; // Outside the scope of function Add.
            Display0 display = new Display0() { Outer = outer };
            Add(ref display); // 3
        }

        internal static void Outer()
        {
            int outer = 1; // Outside the scope of function Add.

            void Add()
            {
                int local = 2; // Inside the scope of function Add.
                Trace.WriteLine(local + outer);
            }

            Add(); // 3
            outer = 3; // Outer variable can change.
            Add(); // 5
        }

        internal static void CompiledOuter()
        {
            int outer = 1;
            Display0 closure = new Display0 { Outer = outer };
            Add(ref closure);
            closure.Outer = outer = 3;
            Add(ref closure);
        }

        internal static void OuterReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int outer = 0; outer < 3; outer++)
            {
                void LocalFunction()
                {
                    Trace.WriteLine(outer); // outer is 0, 1, 2.
                }

                localFunctions.Add(LocalFunction);
            } // outer is 3.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 3 3 3 (instead of 0 1 2)
            }
        }

        internal static void CopyOuterReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int outer = 0; outer < 3; outer++)
            {
                int copyOfOuter = outer; // outer is 0, 1, 2.
                // When outer changes, copyOfOuter does not change.
                void LocalFunction()
                {
                    Trace.WriteLine(copyOfOuter);
                }

                localFunctions.Add(LocalFunction);
            } // copyOfOuter is 0, 1, 2.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 0 1 2
            }
        }

        [CompilerGenerated]
        private sealed class Display2
        {
            public int CopyOfOuter;

            internal void LocalFunction()
            {
                Trace.WriteLine(this.CopyOfOuter);
            }
        }

        internal static void CompiledCopyOuterReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int outer = 0; outer < 3; outer++)
            {
                Display2 display = new Display2() { CopyOfOuter = outer }; // outer is 0, 1, 2.
                // When outer changes, display.CopyOfOuter does not change.
                localFunctions.Add(display.LocalFunction);
            } // display.CcopyOfOuter is 0, 1, 2.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 0 1 2
            }
        }
    }

    internal static partial class Functions
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] private static Action longLife;
        internal static void Reference()
        {
            // https://msdn.microsoft.com/en-us/library/System.Array.aspx
            byte[] shortLife = new byte[0X7FFFFFC7]; // Local variable of large array (Array.MaxByteArrayLength).
            // ...
            void LocalFunction()
            {
                // ...
                byte @byte = shortLife[0]; // Closure.
                // ...
            }
            // ...
            LocalFunction();
            // ...
            longLife = LocalFunction; // Reference from longLife to shortLife.
        }
    }

    internal static partial class Functions
    {
        [CompilerGenerated]
        private sealed class Display3
        {
            public byte[] ShortLife;

            internal void LocalFunction()
            {
                // ...
                byte @byte = this.ShortLife[0];
                // ...
            }
        }

        internal static void CompiledReference()
        {
            byte[] shortLife = new byte[0X7FFFFFC7]; // Local variable of large array (Array.MaxByteArrayLength).
            // ...
            Display3 display = new Display3();
            display.ShortLife = shortLife;
            display.LocalFunction();
            // ...
            longLife = display.LocalFunction;
            // Now longLife.ShortLife holds the reference to the huge large array.
        }
    }
}

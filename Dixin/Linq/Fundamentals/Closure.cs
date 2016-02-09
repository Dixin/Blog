namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    internal class DisplayClass
    {
        int nonLocalVariable = 0; // Outside the scope of method Add.

        internal int Add()
        {
            int localVariable = 1; // Inside the scope of method Add.
            return localVariable + nonLocalVariable; // 1.
        }
    }

    internal static partial class Closure
    {
        internal static void Lambda()
        {
            int nonLocalVariable = 0; // Outside the scope of function add.
            Func<int> add = () =>
                {
                    int localVariable = 1; // Inside the scope of function add.
                    return localVariable + nonLocalVariable;
                };

            int result = add(); // 1.
        }
    }

    internal static class CompiledClosure
    {
        [CompilerGenerated]
        private sealed class DisplayClass0
        {
            public int nonLocalVariable;

            internal int Add()
            {
                int localVariable = 1;
                return localVariable + nonLocalVariable;
            }
        }

        internal static void Outer()
        {
            DisplayClass0 displayClass0 = new DisplayClass0();
            displayClass0.nonLocalVariable = 0;
            Func<int> add = displayClass0.Add;
            int result = add(); // 1.
        }
    }

    internal static partial class Closure
    {
        internal static void ChangedNonLocal()
        {
            int nonLocalVariable = 1; // Outside the scope of function add.
            Func<int> add = () =>
                {
                    int localVariable = 0; // Inside the scope of function add.
                    return localVariable + nonLocalVariable;
                };

            nonLocalVariable = 2; // Non-local variable can change.
            int result = add(); // 2 instead of 1.
        }

        internal static void MultipleReferences()
        {
            List<Func<int>> functions = new List<Func<int>>(3);
            for (int nonLocalVariable = 0; nonLocalVariable < 3; nonLocalVariable++)
            // Outside the scope of function print.
            {
                Func<int> function = () => nonLocalVariable; // nonLocalVariable: 0, 1, 2.
                functions.Add(function);
            }

            // Now nonLocalVariable is 3.
            foreach (Func<int> function in functions)
            {
                int result = function();
                Trace.WriteLine(result); // 3, 3, 3 instead of 0, 1, 2.
            }
        }

        internal static void CopyCurrent()
        {
            List<Func<int>> functions = new List<Func<int>>(3);
            for (int nonLocalVariable = 0; nonLocalVariable < 3; nonLocalVariable++)
            // Outside the scope of function print.
            {
                int copyOfCurrentValue = nonLocalVariable; // nonLocalVariable: 0, 1, 2.
                // When nonLocalVariable changes, copyOfIntermediateState does not change.
                Func<int> function = () => copyOfCurrentValue; // copyOfCurrentValue: 0, 1, 2.
                functions.Add(function);
            }

            // Now nonLocalVariable is 3. Each copyOfCurrentValue does not change.
            foreach (Func<int> function in functions)
            {
                int result = function();
                Trace.WriteLine(result); // 3, 3, 3 instead of 0, 1, 2.
            }
        }
    }

    internal static partial class Closure
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static Func<int> longLifeFunction;

        internal static void Reference()
        {
            // https://msdn.microsoft.com/en-us/library/System.Array.aspx
            byte[] shortLifeVariable = new byte[0X7FFFFFC7];
            // Some code...
            longLifeFunction = () =>
                {
                // Some code...
                byte value = shortLifeVariable[0]; // Reference.
                                                   // More code...
                return 0;
                };
            // More code...
        }
    }
}

namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    internal class DisplayClass
    {
        int outer = 0; // Outside the scope of method Add.

        internal int Add()
        {
            int local = 1; // Inside the scope of method Add.
            return local + this.outer; // 1.
        }
    }

    internal static partial class Functions
    {
        internal static void Closure()
        {
            int outer = 1; // Outside the scope of function add.
            new Action(() =>
            {
                int local = 2; // Inside the scope of function add.
                Trace.WriteLine(local + outer);
            })(); // 3
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private sealed class DisplayClass0
        {
            public int Outer;

            internal void Add()
            {
                int local = 2;
                Trace.WriteLine(local + this.Outer);
            }
        }

        internal static void CompiledClosure()
        {
            int outer = 1;
            DisplayClass0 display = new DisplayClass0() { Outer = outer };
            display.Add(); // 3
        }
    }

#if DEMO
    internal static partial class Functions
    {
        internal static void Outer()
        {
            int outer = 1; // Outside the scope of function add.
            Func<int> add = () =>
            {
                int local = 2; // Inside the scope of function add.
                return local + outer;
            };
            Trace.WriteLine(add()); // 3
            outer = 3; // Outer variable can change.
            Trace.WriteLine(add()); // 5
        }
    }

    internal static partial class Functions
    {
        internal static void OuterReference()
        {
            List<Action> functions = new List<Action>();
            for (int outer = 0; outer < 3; outer++)
            {
                Action function = () => Trace.WriteLine(outer); // outer: 0, 1, 2.
                functions.Add(function);
            } // outer: 3.
            foreach (Action function in functions)
            {
                function(); // 3, 3, 3 instead of 0, 1, 2.
            }
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private sealed class DisplayClass1
        {
            public int outer;

            internal void OuterReference()
            {
                Trace.WriteLine(this.outer);
            }
        }

        internal static void OuterReference()
        {
            List<Action> functions = new List<Action>();
            DisplayClass1 closure = new DisplayClass1();
            for (closure.outer = 0; closure.outer < 3; closure.outer++)
            {
                Action function = closure.OuterReference; // display.outer: 0, 1, 2.
                functions.Add(function);
            } // closure.outer: 3.
            foreach (Action function in functions)
            {
                function(); // 3, 3, 3 instead of 0, 1, 2.
            }
        }
    }

    internal static partial class Functions
    {
        internal static void CopyOuter()
        {
            List<Action> functions = new List<Action>();
            for (int outer = 0; outer < 3; outer++)
            {
                int copyOfOuter = outer; // outer: 0, 1, 2.
                // When outer changes, copyOfOuter does not change.
                Action function = () => Trace.WriteLine(copyOfOuter);
                functions.Add(function);
            } // copyOfOuter: 0, 1, 2.
            foreach (Action function in functions)
            {
                function(); // 0, 1, 2.
            }
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private sealed class DisplayClass2
        {
            public int copyOfOuter;

            internal void CopyOuter()
            {
                Trace.WriteLine(this.copyOfOuter);
            }
        }

        internal static void CopyOuter()
        {
            List<Action> functions = new List<Action>(3);
            for (int outer = 0; outer < 3; outer++)
            {
                DisplayClass2 closure = new DisplayClass2();
                closure.copyOfOuter = outer; // outer: 0, 1, 2.
                // When outer changes, closure.copyOfOuter does not change.
                Action function = closure.CopyOuter;
                functions.Add(function);
            } // closure.copyOfOuter: 0, 1, 2.
            foreach (Action function in functions)
            {
                function(); // 0, 1, 2.
            }
        }
    }

    internal static partial class Functions
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static Action longLife;

        internal static void Reference()
        {
            // https://msdn.microsoft.com/en-us/library/System.Array.aspx
            byte[] shortLife = new byte[0X7FFFFFC7];
            // Code.
            longLife = () =>
            {
                // Code.
                byte @byte = shortLife[0]; // Reference from longLife to shortLife.
                // Code.
            };
            // Code.
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private sealed class DisplayClass3
        {
            public byte[] shortLife;

            internal void Reference()
            {
                // Code.
                byte @byte = this.shortLife[0];
                // Code.
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static Action longLife;

        internal static void Reference()
        {
            byte[] shortLife = new byte[0X7FFFFFC7];
            // Code.
            DisplayClass3 closure = new DisplayClass3();
            closure.shortLife = shortLife; // Reference from closure to shortLife.
            longLife = closure.Reference; // Reference from longLife to closure.
            // Code.
        }
    }
#endif
}

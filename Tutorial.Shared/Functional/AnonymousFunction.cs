namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static partial class Functions
    {
        internal static bool IsPositive(int int32)
        {
            return int32 > 0;
        }

        internal static void NamedFunction()
        {
            Func<int, bool> isPositive = IsPositive;
            bool result = isPositive(0);
        }
    }

    internal static partial class Functions
    {
        internal static void AnonymousFunction()
        {
            Func<int, bool> isPositive = delegate (int int32)
            {
                return int32 > 0;
            };
            bool result = isPositive(0);
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private static Func<int, bool> cachedIsPositive;

        [CompilerGenerated]
        private static bool IsPositive(int int32)
        {
            return int32 > 0;
        }

        internal static void AnonymousFunction()
        {
            Func<int, bool> isPositive;
            if (cachedIsPositive != null)
            {
                isPositive = cachedIsPositive;
            }
            else
            {
                isPositive = cachedIsPositive = new Func<int, bool>(IsPositive);
            }
            bool result = isPositive.Invoke(0);
        }
    }

    internal static partial class Functions
    {
        internal static void Lambda()
        {
            Func<int, bool> isPositive = (int int32) =>
                {
                    return int32 > 0;
                };
            bool result = isPositive(0);
        }

        internal static void ExpressionLambda()
        {
            Func<int, int, int> add = (int32A, int32B) => int32A + int32B;
            Func<int, bool> isPositive = int32 => int32 > 0;
            Action<int> traceLine = int32 => Trace.WriteLine(int32);
        }

        internal static void StatementLambda()
        {
            Func<int, int, int> add = (int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            };
            Func<int, bool> isPositive = int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            };
            Action<int> traceLine = int32 =>
            {
                Trace.WriteLine(int32);
                Trace.Flush();
            };
        }

        internal static void ConstructorCall()
        {
            Func<int, int, int> add = new Func<int, int, int>((int32A, int32B) => int32A + int32B);

            Func<int, bool> isPositive = new Func<int, bool>(int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            });
        }

#if DEMO
        internal static void CallLambdaExpression()
        {
            (int32 => int32 > 0)(1); // Define an expression lambda and call.
        }
#endif
    }

    internal static partial class Functions
    {
        internal static void CallLambdaExpressionWithConstructor()
        {
            bool result = new Func<int, bool>(int32 => int32 > 0)(1);
        }

        internal static void CallLambdaExpressionWithTypeConversion()
        {
            bool result = ((Func<int, bool>)(int32 => int32 > 0))(1);
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        [Serializable]
        private sealed class Container
        {
            public static readonly Container Singleton = new Container();

            public static Func<int, bool> cachedIsPositive;

            internal bool IsPositive(int int32)
            {
                return int32 > 0;
            }
        }

        internal static void CallLambdaExpressionWithConstructor()
        {
            Func<int, bool> isPositive;
            if (Container.cachedIsPositive != null)
            {
                isPositive = Container.cachedIsPositive;
            }
            else
            {
                isPositive = Container.cachedIsPositive = new Func<int, bool>(Container.Singleton.IsPositive);
            }
            bool result = isPositive.Invoke(1);
        }
    }

    internal static partial class Functions
    {
        internal static void CallAnonymousFunction()
        {
            new Func<int, int, int>((int32A, int32B) => int32A + int32B)(1, 2);
            new Action<int>(int32 => Trace.WriteLine(int32))(1);

            new Func<int, int, int>((int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            })(1, 2);
            new Func<int, bool>(int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            })(1);
            new Action<int>(int32 =>
            {
                Trace.WriteLine(int32);
                Trace.Flush();
            })(1);
        }
    }

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
        internal static void AnonymousFunctionClosure()
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

        internal static void CompiledAnonymousFunctionClosure()
        {
            int outer = 1;
            DisplayClass0 display = new DisplayClass0() { Outer = outer };
            display.Add(); // 3
        }
    }
}

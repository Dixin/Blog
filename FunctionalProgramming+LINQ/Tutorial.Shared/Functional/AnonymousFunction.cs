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
            if (cachedIsPositive == null)
            {
                cachedIsPositive = new Func<int, bool>(IsPositive);
            }
            isPositive = cachedIsPositive;
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
            Action<int> traceLine = int32 => int32.WriteLine();
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
                int32.WriteLine();
                return int32 > 0;
            };
            Action<int> traceLine = int32 =>
            {
                int32.WriteLine();
                Trace.Flush();
            };
        }

        internal static void ConstructorCall()
        {
            Func<int, int, int> add = new Func<int, int, int>((int32A, int32B) => int32A + int32B);
            Func<int, bool> isPositive = new Func<int, bool>(int32 =>
            {
                int32.WriteLine();
                return int32 > 0;
            });
        }

        internal static void TypeConversion()
        {
            Func<int, int, int> add = (Func<int, int, int>)((int32A, int32B) => int32A + int32B);
            Func<int, bool> isPositive = (Func<int, bool>)(int32 =>
            {
                int32.WriteLine();
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
            if (Container.cachedIsPositive == null)
            {
                Container.cachedIsPositive = new Func<int, bool>(Container.Singleton.IsPositive);
            }
            isPositive = Container.cachedIsPositive;
            bool result = isPositive.Invoke(1);
        }
    }

    internal static partial class Functions
    {
        internal static void CallAnonymousFunction()
        {
            new Func<int, int, int>((int32A, int32B) => int32A + int32B)(1, 2);
            new Action<int>(int32 => int32.WriteLine())(1);

            new Func<int, int, int>((int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            })(1, 2);
            new Func<int, bool>(int32 =>
            {
                int32.WriteLine();
                return int32 > 0;
            })(1);
            new Action<int>(int32 =>
            {
                int32.WriteLine();
                Trace.Flush();
            })(1);
        }
    }

    internal class DisplayClass
    {
        int outer = 1; // Outside the scope of method Add.

        internal int Add()
        {
            int local = 2; // Inside the scope of method Add.
            return local + this.outer; // 3.
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
                (local + outer).WriteLine();
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
                (local + this.Outer).WriteLine();
            }
        }

        internal static void CompiledAnonymousFunctionClosure()
        {
            int outer = 1;
            DisplayClass0 display = new DisplayClass0() { Outer = outer };
            display.Add(); // 3
        }
    }

#if DEMO
    internal partial class Data
    {
        private int value;

        static Data() => MethodBase.GetCurrentMethod().Name.WriteLine(); // Static constructor.

        internal Data(int value) => this.value = value; // Constructor.

        ~Data() => Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // Finalizer.

        internal bool Equals(Data other) => this.value == other.value; // Instance method.

        internal static bool Equals(Data @this, Data other) => @this.value == other.value; // Static method.

        public static Data operator +(Data data1, Data Data) => new Data(data1.value + Data.value); // Operator overload.

        public static explicit operator int(Data value) => value.value; // Conversion operator.

        public static implicit operator Data(int value) => new Data(value); // Conversion operator.

        internal int ReadOnlyValue => this.value; // Property.

        internal int ReadWriteValue
        {
            get => this.value; // Property getter.
            set => this.value = value; // Property setter.
        }

        internal int this[long index] => throw new NotImplementedException(); // Indexer.

        internal int this[int index]
        {
            get => throw new NotImplementedException(); // Indexer getter.
            set => throw new NotImplementedException(); // Indexer setter.
        }

        internal event EventHandler Created
        {
            add => Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // Event accessor.
            remove => Trace.WriteLine(MethodBase.GetCurrentMethod().Name); // Event accessor.
        }

        internal int GetValue()
        {
            int LocalFunction() => this.value; // Local function.
            return LocalFunction();
        }
    }

    internal static partial class DataExtensions
    {
        internal static bool Equals(Data @this, Data other) => @this.ReadOnlyValue == other.Value; // Extension method.
    }

    internal partial class Data : IComparable<Data>
    {
        int IComparable<Data>.CompareTo(Data other) => this.value.CompareTo(other.value); // Explicit interface implementation.
    }
#endif
}

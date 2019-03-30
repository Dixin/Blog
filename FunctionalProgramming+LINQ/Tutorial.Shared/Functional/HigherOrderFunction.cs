namespace Tutorial.Functional
{
    using System;

    internal partial class Data { }

    internal static partial class Functions
    {
        internal static Data FirstOrder(Data value)
        {
            return value;
        }

        internal static void CallFirstOrder()
        {
            Data input = default;
            Data output = FirstOrder(input);
        }
    }

    // () -> void.
    internal delegate void Function();

    internal static partial class Functions
    {
        internal static Function NamedHigherOrder(Function value)
        {
            return value;
        }

        internal static void CallHigherOrder()
        {
            Function input = default;
            Function output = NamedHigherOrder(input);
        }
    }

    internal static partial class Functions
    {
        internal static void LambdaHigherOrder()
        {
            Action firstOrder1 = () => nameof(LambdaHigherOrder).WriteLine();
            firstOrder1(); // LambdaHigherOrder

            // (() -> void) -> void
            // Input: function of type () -> void. Output: void.
            Action<Action> higherOrder1 = action => action();
            higherOrder1(firstOrder1); // firstOrder1
            higherOrder1(() => nameof(LambdaHigherOrder).WriteLine()); // LambdaHigherOrder

            Func<int> firstOrder2 = () => 1;
            firstOrder2().WriteLine(); // 1

            // () -> (() -> int)
            // Input: none. Output: function of type () -> int.
            Func<Func<int>> higherOrder2 = () => firstOrder2;
            Func<int> output2 = higherOrder2();
            output2().WriteLine(); // 1

            // int -> (() -> int)
            // Input: value of type int. Output: function of type () -> int.
            Func<int, Func<int>> higherOrder3 = int32 =>
                (() => int32 + 1);
            Func<int> output3 = higherOrder3(1);
            output3().WriteLine(); // 2

            // (() -> void, () -> int) -> (() -> bool)
            // Input: function of type () -> void, function of type () -> int. Output: function of type () -> bool.
            Func<Action, Func<int>, Func<bool>> higherOrder4 = (action, int32Factory) =>
            {
                action();
                return () => int32Factory() > 0;
            };
            Func<bool> output4 = higherOrder4(firstOrder1, firstOrder2); // LambdaHigherOrder
            output4().WriteLine(); // True
            output4 = higherOrder4(() => nameof(LambdaHigherOrder).WriteLine(), () => 0); // LambdaHigherOrder
            output4().WriteLine(); // False
        }

        internal static void AnonymousHigherOrder()
        {
            // (() -> void) -> void
            new Action<Action>(action => action())(
                () => nameof(AnonymousHigherOrder).WriteLine());

            // () -> (() -> int)
            Func<int> output2 = new Func<Func<int>>(() => (() => 1))();
            output2().WriteLine(); // 1

            // int -> (() -> int)
            Func<int> output3 = new Func<int, Func<int>>(int32 => (() => int32 + 1))(1);
            output3().WriteLine(); // 2

            // (() -> int, () -> string) -> (() -> bool)
            Func<bool> output4 = new Func<Action, Func<int>, Func<bool>>((action, int32Factory) =>
            {
                action();
                return () => int32Factory() > 0;
            })(() => nameof(LambdaHigherOrder).WriteLine(), () => 0);
            output4().WriteLine();
        }

        internal static void FilterArray(Uri[] array)
        {
            Uri[] notNull = Array.FindAll(array, uri => uri != null);
        }
    }

    internal static partial class Functions
    {
        internal static void Object()
        {
            Data value = new Data(0);
        }

        internal static void Function()
        {
            Function value1 = Function; // Named function.
            Function value2 = () => { }; // Anonymous function.
        }
    }

    internal static partial class Functions
    {
        private static Data field = new Data(0);

        private static Function namedFunctionField = Function;

        private static Function anonymousFunctionField = () => { };
    }

    internal static partial class Functions
    {
        internal static Data Function(Data value) => value;

        internal static Function Function(Function value) => value;
    }

    internal static partial class Functions
    {
        internal class OuterClass
        {
            const int Outer = 1;

            class AccessOuter
            {
                const int Local = 2;
                int sum = Local + Outer;
            }
        }

        internal static void OuterFunction()
        {
            const int Outer = 1;

            void AccessOuter()
            {
                const int Local = 2;
                int sum = Local + Outer;
            }

            Function accessOuter = () =>
            {
                const int Local = 2;
                int sum = Local + Outer;
            };
        }
    }

    internal partial class Data
    {
        internal Data Inner { get; set; }
    }

    internal static partial class Functions
    {
        internal static void NestedObject()
        {
            Data outer = new Data(0)
            {
                Inner = new Data(1)
            };
        }

        internal static void NestedFunction()
        {
            void Outer()
            {
                void Inner() { }
            }

            Function outer = () =>
            {
                Function inner = () => { };
            };
        }
    }

    internal static partial class Functions
    {
        internal static void ObjectEquality()
        {
            Data value1;
            Data value2;
            value1 = value2 = new Data(0);
            object.ReferenceEquals(value1, value2).WriteLine(); // True
            object.Equals(value1, value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True

            value1 = new Data(1);
            value2 = new Data(1);
            object.ReferenceEquals(value1, value2).WriteLine(); // False
            object.Equals(value1, value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True
        }

        internal static void FunctionEquality()
        {
            Function value1;
            Function value2;
            value1 = value2 = () => { };
            object.ReferenceEquals(value1, value2).WriteLine(); // True
            object.Equals(value1, value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True

            value1 = new Function(Function);
            value2 = new Function(Function);
            object.ReferenceEquals(value1, value2).WriteLine(); // False
            object.Equals(value1, value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True
        }
    }
}

#if DEMO
namespace System
{
    using System.Collections;

    public abstract class Array : ICollection, IEnumerable, IList, IStructuralComparable, IStructuralEquatable
    {
        public static T[] FindAll<T>(T[] array, Predicate<T> match);
    }
}
#endif
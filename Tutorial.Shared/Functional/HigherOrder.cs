namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal partial class Data { }

    internal static partial class Functions
    {
        internal static Data FirstOrder(Data value)
        {
            return value;
        }

        internal static void CallFirstOrder()
        {
            Data input = default(Data);
            Data output = FirstOrder(input);
        }
    }

    internal delegate void Function();

    internal static partial class Functions
    {
        internal static Function NamedHigherOrder(Function value)
        {
            return value;
        }

        internal static void CallHigherOrder()
        {
            Function input = default(Function);
            Function output = NamedHigherOrder(input);
        }
    }

    internal static partial class Functions
    {
        internal static void AnonymousHigherOrder()
        {
            Action firstOrder1 = () => Trace.WriteLine(nameof(firstOrder1));
            firstOrder1(); // firstOrder1

            Action<Action> higherOrder1 = action => action();
            higherOrder1(firstOrder1);  // firstOrder1
            higherOrder1(() => Trace.WriteLine(nameof(higherOrder1))); // higherOrder1

            Func<int> firstOrder2 = () => int.MaxValue;
            Trace.WriteLine(firstOrder2()); // 0x7FFFFFFF

            Func<Func<int>> higherOrder2 = () => firstOrder2;
            Func<int> output2 = higherOrder2();
            Trace.WriteLine(output2()); // 0x7FFFFFFF

            Func<int, Func<int>> higherOrder3 =
                int32 => // Input: value of type int.
                    (() => int32); // Output: function of type Func<int>.
            Func<int> output3 = higherOrder3(1);
            Trace.WriteLine(output3()); // 1

            Func<Func<int>, Func<string>, Func<bool>> higherOrder4 =
                // Input: int32Factory of type Func<int>, stringFactory of type Func<string>.
                (int32Factory, stringFactory) =>
                {
                    Trace.WriteLine(stringFactory());
                    return () => int32Factory() > 0; // Output: function of type Func<int>.
                };
            Func<bool> output4 = higherOrder4(() => 0, () => nameof(higherOrder4)); // higherOrder4
            Trace.WriteLine(output4()); // False
        }

        internal static void FilterArray(Uri[] array)
        {
            Uri[] notNull = Array.FindAll(array, uri => uri != null);
        }
    }

    internal partial class LinqToObjects
    {
        internal static void QueryMethods()
        {
            IEnumerable<int> source = new int[] { 4, 3, 2, 1, 0, -1 }; // Get source.
            IEnumerable<double> query = source
                .Where(predicate: value => value > 0)
                .OrderBy(keySelector: value => value)
                .Select(selector: value => Math.Sqrt(value)); // Create query.
            foreach (double result in query) // Execute query.
            {
                Trace.WriteLine(result);
            }
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
            Function value = Function; // Named function.
            new Function(() => { })(); // Anonymous function.
        }
    }

    internal static partial class Functions
    {
        internal static Data dataField = new Data(0);

        internal static Function namedFunctionField = Function;

        internal static Function anonymousFunctionField = () => { };
    }

    internal static partial class Functions
    {
        internal static Data InputOutput(Data value) => value;

        internal static Function InputOutput(Function value) => value;
    }

    internal static partial class Functions
    {
        internal class OuterClass
        {
            const int outer = 1;

            class AccessOuter
            {
                const int local = 2;
                int sum = local + outer;
            }
        }

        internal static void OuterFunction()
        {
            const int outer = 1;

            void AccessOuter()
            {
                const int local = 2;
                int sum = local + outer;
            }

            Function accessOuter = () =>
            {
                const int local = 2;
                int sum = local + outer;
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
                void Inner()
                {
                }
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
            Trace.WriteLine(object.ReferenceEquals(value1, value2)); // True
            Trace.WriteLine(object.Equals(value1, value2)); // True

            value1 = new Data(1);
            value2 = new Data(1);
            Trace.WriteLine(object.ReferenceEquals(value1, value2)); // False
            Trace.WriteLine(object.Equals(value1, value2)); // True
        }

        internal static void FunctionEquality()
        {
            Function value1;
            Function value2;
            value1 = value2 = () => { };
            Trace.WriteLine(object.ReferenceEquals(value1, value2)); // True
            Trace.WriteLine(object.Equals(value1, value2)); // True

            value1 = new Function(Function);
            value2 = new Function(Function);
            Trace.WriteLine(object.ReferenceEquals(value1, value2)); // False
            Trace.WriteLine(object.Equals(value1, value2)); // True
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
namespace Tutorial.Functional
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class Base { }

    internal class Derived : Base { }

    internal static partial class Variances
    {
        internal static void Substitute()
        {
            Base @base = new Base();
            @base = new Derived();
        }
    }

    internal static partial class Variances
    {
        // Derived -> Base
        internal static Base DerivedToBase(Derived input) => new Base();

        // Derived -> Derived
        internal static Derived DerivedToDerived(Derived input) => new Derived();

        // Base -> Base
        internal static Base BaseToBase(Base input) => new Base();

        // Base -> Derived
        internal static Derived BaseToDerived(Base input) => new Derived();
    }

    internal delegate Base DerivedToBase(Derived input); // Derived -> Base

    internal delegate Derived DerivedToDerived(Derived input); // Derived -> Derived

    internal delegate Base BaseToBase(Base input); // Base -> Base

    internal delegate Derived BaseToDerived(Base input); // Base -> Derived

    internal static partial class Variances
    {
        internal static void NonGeneric()
        {
            DerivedToDerived derivedToDerived = DerivedToDerived;
            Derived output = derivedToDerived(input: new Derived());
        }

        internal static void NonGenericCovariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Covariance: Derived is Base, so that DerivedToDerived is DerivedToBase.
            derivedToBase = DerivedToDerived; // Derived -> Derived

            // When calling derivedToBase, DerivedToDerived executes.
            // derivedToBase should output Base, while DerivedToDerived outputs Derived.
            // The actual Derived output is the required Base output. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericContravariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Contravariance: Derived is Base, so that BaseToBase is DerivedToBase.
            derivedToBase = BaseToBase; // Base -> Base

            // When calling derivedToBase, BaseToBase executes.
            // derivedToBase should accept Derived input, while BaseToBase accepts Base input.
            // The required Derived input is the accepted Base input. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericeCovarianceAndContravariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Covariance and contravariance: Derived is Base, so that BaseToDerived is DerivedToBase. 
            derivedToBase = BaseToDerived; // Base -> Derived

            // When calling derivedToBase, BaseToDerived executes.
            // derivedToBase should accept Derived input, while BaseToDerived accepts Base input.
            // The required Derived input is the accepted Base input.
            // derivedToBase should output Base, while BaseToDerived outputs Derived.
            // The actual Derived output is the required Base output. This always works.
            Base output = derivedToBase(input: new Derived());
        }
    }

    internal static partial class Variances
    {
#if DEMO
        internal static void NonGenericInvalidVariance()
        {
            // baseToDerived should output Derived, while BaseToBase outputs Base. 
            // The actual Base output is not the required Derived output. This cannot be compiled.
            BaseToDerived baseToDerived = BaseToBase; // Base -> Derived

            // baseToDerived should accept Base input, while DerivedToDerived accepts Derived input.
            // The required Base input is not the accepted Derived input. This cannot be compiled.
            baseToDerived = DerivedToDerived; // Derived -> Derived

            // baseToDerived should accept Base input, while DerivedToBase accepts Derived input.
            // The required Base input is not the expected Derived input.
            // baseToDerived should output Derived, while DerivedToBase outputs Base.
            // The actual Base output is not the required Derived output. This cannot be compiled.
            baseToDerived = DerivedToBase; // Derived -> Base
        }
#endif
    }

    internal delegate TOutput GenericFunc<TInput, TOutput>(TInput input);

    internal static partial class Variances
    {
        internal static void Generic()
        {
            GenericFunc<Derived, Base> derivedToBase = DerivedToBase; // GenericFunc<Derived, Base>: no variances.
            derivedToBase = DerivedToDerived; // GenericFunc<Derived, Derived>: covariance.
            derivedToBase = BaseToBase; // GenericFunc<Base, Base>: contravariance.
            derivedToBase = BaseToDerived; // GenericFunc<Base, Derived>: covariance and contravariance.
        }

#if DEMO
        internal static void FunctionImplicitConversion()
        {
            GenericFunc<Derived, Base> derivedToBase = DerivedToBase; // Derived -> Base
            GenericFunc<Derived, Derived> derivedToDerived = DerivedToDerived; // Derived -> Derived
            GenericFunc<Base, Base> baseToBase = BaseToBase; // Base -> Base
            GenericFunc<Base, Derived> baseToDerived = BaseToDerived; // Base -> Derived

            derivedToBase = derivedToDerived; // Covariance, but cannot be compiled.
            derivedToBase = baseToBase; // Contravariance, but cannot be compiled.
            derivedToBase = baseToDerived; // Covariance and contravariance, but cannot be compiled.
        }
#endif
    }

    internal delegate TOutput GenericFuncWithVariances<in TInput, out TOutput>(TInput input);

    internal static partial class Variances
    {
        internal static void GenericWithVariances()
        {
            GenericFuncWithVariances<Derived, Base> derivedToBase = DerivedToBase; // Derived -> Base: no variances.
            derivedToBase = DerivedToDerived; // Derived -> Derived: covariance.
            derivedToBase = BaseToBase; // Base -> Base: contravariance.
            derivedToBase = BaseToDerived; // Base -> Derived: covariance and contravariance.
        }

        internal static void FunctionImplicitConversion()
        {
            GenericFuncWithVariances<Derived, Base> derivedToBase = DerivedToBase; // Derived -> Base
            GenericFuncWithVariances<Derived, Derived> derivedToDerived = DerivedToDerived; // Derived -> Derived
            GenericFuncWithVariances<Base, Base> baseToBase = BaseToBase; // Base -> Base
            GenericFuncWithVariances<Base, Derived> baseToDerived = BaseToDerived; // Base -> Derived

            // Cannot be compiled without the out/in modifiers.
            derivedToBase = derivedToDerived; // Covariance.
            derivedToBase = baseToBase; // Contravariance.
            derivedToBase = baseToDerived; // Covariance and contravariance.
        }
    }

#if DEMO
    // CS1961 Invalid variance: The type parameter 'TOutput' must be covariantly valid on 'GenericFuncWithVariances<TOutput>.Invoke()'. 'TOutput' is contravariant.
    internal delegate TOutput GenericFuncWithVariances<in TOutput>();

    // CS1961 Invalid variance: The type parameter 'TInput' must be contravariantly valid on 'GenericActionWithVariances<TInput>.Invoke(TIn)'. 'TInput' is covariant.
    internal delegate void GenericActionWithVariances<out TInput>(TInput input);

    // CS1961 Invalid variance: The type parameter 'TOutput' must be covariantly valid on 'GenericFuncWithVariances<TInput, TOutput>.Invoke(TInput)'. 'TOutput' is contravariant.
    // CS1961 Invalid variance: The type parameter 'TInput' must be contravariantly valid on 'GenericFuncWithVariances<TInput, TOutput>.Invoke(TInput)'. 'TInput' is covariant.
    internal delegate TOutput GenericFuncWithVariances<out TInput, in TOutput>(TInput input);
#endif

    internal interface IOutput<out TOutput> // TOutput is covariant for all members using TOutput.
    {
        TOutput ToOutput(); // () -> TOutput

        TOutput Output { get; } // get_Output: () -> TOutput

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceCovariance(IOutput<Base> outputBase, IOutput<Derived> outputDerived)
        {
            // Covariance: Derived is Base, so that IOutput<Derived> is IOutput<Base>.
            outputBase = outputDerived;

            // When calling outputBase.ToOutput, outputDerived.ToOutput executes.
            // outputBase.ToOutput should output Base, outputDerived.ToOutput outputs Derived.
            // The actual Derived output is the required Base output. This always works.
            Base output1 = outputBase.ToOutput();

            Base output2 = outputBase.Output; // outputBase.get_Output().
        }
    }

    internal interface IInput<in TInput> // TInput is contravariant for all members using TInput.
    {
        void InputToVoid(TInput input); // TInput -> void

        TInput Input { set; } // set_Input: TInput -> void

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceContravariance(IInput<Derived> inputDerived, IInput<Base> inputBase)
        {
            // Contravariance: Derived is Base, so that IInput<Base> is IInput<Derived>.
            inputDerived = inputBase;

            // When calling inputDerived.Input, inputBase.Input executes.
            // inputDerived.Input should accept Derived input, while inputBase.Input accepts Base input.
            // The required Derived output is the accepted Base input. This always works.
            inputDerived.InputToVoid(input: new Derived());

            inputDerived.Input = new Derived();
        }
    }

    internal interface IInputOutput<in TInput, out TOutput> // TInput/TOutput is contravariant/covariant for all members using TInput/TOutput.
    {
        void InputToVoid(TInput input); // TInput -> void

        TInput Input { set; } // set_Input: TInput -> void

        TOutput ToOutput(); // () -> TOutput

        TOutput Output { get; } // get_Output: () -> TOutput

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceCovarianceAndContravariance(
            IInputOutput<Derived, Base> inputDerivedOutputBase, IInputOutput<Base, Derived> inputBaseOutputDerived)
        {
            // Covariance and contravariance: Derived is Base, so that IInputOutput<Base, Derived> is IInputOutput<Derived, Base>.
            inputDerivedOutputBase = inputBaseOutputDerived;

            inputDerivedOutputBase.InputToVoid(new Derived());
            inputDerivedOutputBase.Input = new Derived();
            Base output1 = inputDerivedOutputBase.ToOutput();
            Base output2 = inputDerivedOutputBase.Output;
        }
    }

    internal interface IInvariant<T>
    {
        T Output(); // T is covariant for Output: () -> T.

        void Input(T input); // T is contravariant for Input: T -> void.
    }

    internal static partial class Variances
    {
        internal static void OutputVariance()
        {
            // First order functions.
            Func<Base> toBase = () => new Base();
            Func<Derived> toDerived = () => new Derived();

            // Higher-order functions.
            ToFunc<Base> toToBase = () => toBase;
            ToFunc<Derived> toToDerived = () => toDerived;

            // Covariance: Derived is Base, so that ToFunc<Derived> is ToFunc<Base>.
            toToBase = toToDerived;

            // When calling toToBase, toToDerived executes.
            // toToBase should output Func<Base>, while toToDerived outputs Func<Derived>.
            // The actual Func<Derived> output is the required Func<Base> output. This always works.
            Func<Base> output = toToBase();
        }

        // () -> T:
        internal delegate TOutput Func<out TOutput>(); // Covariant output type.

        // () -> () -> T, equivalent to Func<Func<T>>:
        internal delegate Func<TOutput> ToFunc<out TOutput>(); // Covariant output type.

        // () -> () -> () -> T: Equivalent to Func<Func<Func<T>>>:
        internal delegate ToFunc<TOutput> ToToFunc<out TOutput>(); // Covariant output type.

        // () -> () -> () -> () -> T: Equivalent to Func<Func<Func<Func<T>>>>:
        internal delegate ToToFunc<TOutput> ToToToFunc<out TOutput>(); // Covariant output type.

        // ...

#if DEMO
        internal delegate void ActionToVoid<in TInput>(Action<TInput> action); // Cannot be compiled.

        internal static void InputVariance()
        {
            ActionToVoid<Derived> derivedToVoidToVoid = (Action<Derived> derivedToVoid) => { };
            ActionToVoid<Base> baseToVoidToVoid = (Action<Base> baseToVoid) => { };
            derivedToVoidToVoid = baseToVoidToVoid;
        }
#endif

        internal static void InputVariance()
        {
            // Higher-order functions.
            ActionToVoid<Derived> derivedToVoidToVoid = (Action<Derived> derivedToVoid) => { };
            ActionToVoid<Base> baseToVoidToVoid = (Action<Base> baseToVoid) => { };

            // Covariance: Derived is Base, so that ActionToVoid<Derived> is ActionToVoid<Base>.
            baseToVoidToVoid = derivedToVoidToVoid;

            // When calling baseToVoidToVoid, derivedToVoidToVoid executes.
            // baseToVoidToVoid should accept Action<Base> input, while derivedToVoidToVoid accepts Action<Derived> input.
            // The required Action<Derived> input is the accepted Action<Base> input. This always works.
            baseToVoidToVoid(default(Action<Base>));
        }

        // () -> void:
        internal delegate void Action<in TInput>(TInput input); // Contravariant input type.

        // (() -> void) -> void, equivalent to Action<Action<T>>:
        internal delegate void ActionToVoid<out TTInput>(Action<TTInput> action); // Covariant input type.

        // ((() -> void) -> void) -> void, equivalent to Action<Action<Action<T>>>:
        internal delegate void ActionToVoidToVoid<in TTInput>(ActionToVoid<TTInput> actionToVoid); // Contravariant input type.

        // (((() -> void) -> void) -> void) -> void, equivalent to Action<Action<Action<Action<T>>>>:
        internal delegate void ActionToVoidToVoidToVoid<out TTInput>(ActionToVoidToVoid<TTInput> actionToVoidToVoid); // Covariant input type.

        // ...
    }

    internal static partial class Variances
    {
        internal static void ArrayCovariance()
        {
            Base[] baseArray = new Base[3];
            Derived[] derivedArray = new Derived[3];

            baseArray = derivedArray; // Array covariance at compile time, baseArray refers to a Derived array at runtime.
            Base value = baseArray[0];
            baseArray[1] = new Derived();
            baseArray[2] = new Base(); // ArrayTypeMismatchException at runtime, Base cannot be in Derived array.
        }
    }

    internal static partial class Variances
    {
        internal static void TypesWithVariance()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            coreLibrary.GetExportedTypes()
                .Where(type => type.GetGenericArguments().Any(typeArgument =>
                {
                    GenericParameterAttributes attributes = typeArgument.GenericParameterAttributes;
                    return attributes.HasFlag(GenericParameterAttributes.Covariant)
                        || attributes.HasFlag(GenericParameterAttributes.Contravariant);
                }))
                .OrderBy(type => type.FullName)
                .WriteLines();
                // System.Action`1[T]
                // System.Action`2[T1,T2]
                // System.Action`3[T1,T2,T3]
                // System.Action`4[T1,T2,T3,T4]
                // System.Action`5[T1,T2,T3,T4,T5]
                // System.Action`6[T1,T2,T3,T4,T5,T6]
                // System.Action`7[T1,T2,T3,T4,T5,T6,T7]
                // System.Action`8[T1,T2,T3,T4,T5,T6,T7,T8]
                // System.Collections.Generic.IComparer`1[T]
                // System.Collections.Generic.IEnumerable`1[T]
                // System.Collections.Generic.IEnumerator`1[T]
                // System.Collections.Generic.IEqualityComparer`1[T]
                // System.Collections.Generic.IReadOnlyCollection`1[T]
                // System.Collections.Generic.IReadOnlyList`1[T]
                // System.Comparison`1[T]
                // System.Converter`2[TInput,TOutput]
                // System.Func`1[TResult]
                // System.Func`2[T,TResult]
                // System.Func`3[T1,T2,TResult]
                // System.Func`4[T1,T2,T3,TResult]
                // System.Func`5[T1,T2,T3,T4,TResult]
                // System.Func`6[T1,T2,T3,T4,T5,TResult]
                // System.Func`7[T1,T2,T3,T4,T5,T6,TResult]
                // System.Func`8[T1,T2,T3,T4,T5,T6,T7,TResult]
                // System.Func`9[T1,T2,T3,T4,T5,T6,T7,T8,TResult]
                // System.IComparable`1[T]
                // System.IObservable`1[T]
                // System.IObserver`1[T]
                // System.IProgress`1[T]
                // System.Predicate`1[T]
        }

        internal static void LinqToObjects(IEnumerable<Base> enumerableOfBase, IEnumerable<Derived> enumerableOfDerived)
        {
            enumerableOfBase = enumerableOfBase.Concat(enumerableOfDerived);
        }
    }
}

#if DEMO
namespace System
{
    public delegate TResult Func<out TResult>();

    public delegate TResult Func<in T, out TResult>(T arg);

    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    // ...

    public delegate void Action();

    public delegate void Action<in T>(T obj);

    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    // ...
}

namespace System.Collections.Generic
{
    public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        T this[int index] { get; set; }
        // T is covariant for get_Item: int -> T.
        // T is contravariant for set_Item: (int, T) -> void.

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}

namespace System.Collections.Generic
{
    /// <summary>Exposes the enumerator, which supports a simple iteration over a collection of a specified type.</summary>
    /// <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        T Current { get; } // T is covariant for get_Current: () –> T.
    }

    /// <summary>Exposes the enumerator, which supports a simple iteration over a collection of a specified type.</summary>
    /// <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator(); // T is covariant for IEnumerator<T>, so T is covariant for () -> IEnumerator<T>.
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second);
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>Provides functionality to evaluate queries against a specific data source wherein the type of the data is known.</summary>
    /// <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable { }
}
#endif

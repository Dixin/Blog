namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Mono.Cecil;

    internal class Base { }

    internal class Derived : Base { }

    internal static partial class Variances
    {
        internal static void Object()
        {
            Base @base1 = new Base();
            Base @base2 = new Derived();
        }
    }

    internal static partial class Variances
    {
        internal static Base DerivedToBase(Derived input) => new Base();

        internal static Derived DerivedToDerived(Derived input) => new Derived();

        internal static Base BaseToBase(Base input) => new Base();

        internal static Derived BaseToDerived(Base input) => new Derived();
    }

    internal delegate Base DerivedToBase(Derived input);

    internal delegate Derived DerivedToDerived(Derived input);

    internal delegate Base BaseToBase(Base input);

    internal delegate Derived BaseToDerived(Base input);

    internal static partial class Variances
    {
        internal static void NonGeneric()
        {
            DerivedToBase derivedToBase = DerivedToBase;
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericCovariance()
        {
            DerivedToDerived derivedToDerived = DerivedToDerived;

            // Covariance: Derived is Base => DerivedToDerived is DerivedToBase.
            DerivedToBase derivedToBase = DerivedToDerived;

            // When calling derivedToBase, DerivedToDerived executes.
            // derivedToBase should output Base, while DerivedToDerived outputs Derived.
            // The actual Derived output is the required Base output. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericContravariance()
        {
            BaseToBase baseToBase = BaseToBase;

            // Contravariance: Derived is Base => BaseToBase is DerivedToBase.
            DerivedToBase derivedToBase = BaseToBase;

            // When calling derivedToBase function, BaseToBase executes.
            // derivedToBase should have Derived input, while BaseToBase expects Base input.
            // The required Derived input is the expected Base input. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericeCovarianceAndContravariance()
        {
            BaseToDerived baseToDerived = BaseToDerived;

            // Covariance and contravariance: Derived is Base => BaseToDerived is DerivedToBase. 
            DerivedToBase derivedToBase = BaseToDerived;

            // When calling derivedToBase function, BaseToDerived executes.
            // derivedToBase should have Derived input, while BaseToDerived expects Base input.
            // derivedToBase should output Base, while BaseToDerived outputs Derived.
            Base output = derivedToBase(input: new Derived());
        }
    }

    internal static partial class Variances
    {
#if DEMO
        internal static void NonGenericInvalidVariance()
        {
            // baseToDerived1 should output Derived, while BaseToBase outputs Base. 
            // The actual Base output is not the required Derived output. This does not work.
            BaseToDerived baseToDerived1 = BaseToBase;

            // baseToDerived2 should have Base input, while DerivedToBase expects Derived input.
            // The required Base input is not the expected Derived input. This does not work.
            BaseToDerived baseToDerived2 = DerivedToBase;

            // baseToDerived3 should have Base input, while DerivedToDerived expects Derived input.
            // baseToDerived3 should output Derived, while DerivedToDerived outputs Base.
            BaseToDerived baseToDerived3 = DerivedToDerived;
        }
#endif
    }

    internal delegate TOutput GenericFunc<TInput, TOutput>(TInput input);

    internal static partial class Variances
    {
        internal static void Generic()
        {
            GenericFunc<Derived, Base> derivedToBase1 = DerivedToBase; // No variances.
            GenericFunc<Derived, Base> derivedToBase2 = DerivedToDerived; // Covariance.
            GenericFunc<Derived, Base> derivedToBase3 = BaseToBase; // Contravariance.
            GenericFunc<Derived, Base> derivedToBase4 = BaseToDerived; // Covariance and contravariance.
        }

#if DEMO
        internal static void FunctionImplicitConversion()
        {
            GenericFunc<Derived, Base> derivedToBase = DerivedToBase;
            GenericFunc<Derived, Derived> derivedToDerived = DerivedToDerived;
            GenericFunc<Base, Base> baseToBase = BaseToBase;
            GenericFunc<Base, Derived> baseToDerived = BaseToDerived;

            derivedToBase = derivedToDerived; // Covariance, cannot be compiled.
            derivedToBase = baseToBase; // Contravariance, cannot be compiled.
            derivedToBase = baseToDerived; // Covariance and contravariance, cannot be compiled.
        }
#endif
    }

    internal delegate TOutput FuncWithVariances<in TInput, out TOutput>(TInput input);

    internal static partial class Variances
    {
        internal static void GenericWithVariances()
        {
            FuncWithVariances<Derived, Base> derivedToBase1 = DerivedToBase; // No variances.
            FuncWithVariances<Derived, Base> derivedToBase2 = DerivedToDerived; // Covariance.
            FuncWithVariances<Derived, Base> derivedToBase3 = BaseToBase; // Contravariance.
            FuncWithVariances<Derived, Base> derivedToBase4 = BaseToDerived; // Covariance and contravariance.
        }

        internal static void FunctionImplicitConversion2()
        {
            // Can be compiled with or without out/in keywords.
            FuncWithVariances<Derived, Base> derivedToBase = DerivedToBase;
            FuncWithVariances<Derived, Derived> derivedToDerived = DerivedToDerived;
            FuncWithVariances<Base, Base> baseToBase = BaseToBase;
            FuncWithVariances<Base, Derived> baseToDerived = BaseToDerived;

            // Cannot be compiled without out/in keywords.
            derivedToBase = derivedToDerived; // Covariance.
            derivedToBase = baseToBase; // Contravariance.
            derivedToBase = baseToDerived; // Covariance and contravariance.
        }
    }

#if DEMO
    // CS1961 Invalid variance: The type parameter 'TOutput' must be covariantly valid on 'FuncFuncWithVariances<TOutput>.Invoke()'. 'TOutput' is contravariant.
    internal delegate TOutput FuncWithVariances<in TOutput>();

    // CS1961 Invalid variance: The type parameter 'TInput' must be contravariantly valid on 'ActionFuncWithVariances<TInput>.Invoke(TIn)'. 'TInput' is covariant.
    internal delegate void ActionWithVariances<out TInput>(TInput input);

    // CS1961 Invalid variance: The type parameter 'TOutput' must be covariantly valid on 'FuncFuncWithVariances<TInput, TOutput>.Invoke(TInput)'. 'TOutput' is contravariant.
    // CS1961 Invalid variance: The type parameter 'TInput' must be contravariantly valid on 'FuncFuncWithVariances<TInput, TOutput>.Invoke(TInput)'. 'TInput' is covariant.
    internal delegate TOutput FuncFuncWithVariances<out TInput, in TOutput>(TInput input);
#endif

    internal interface IOutput<out TOutput> // TOutput is covariant for all members using TOutput.
    {
        TOutput ToOutput();

        TOutput Output { get; } // TOutput get_Output();

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceCovariance(IOutput<Base> outputBase, IOutput<Derived> outputDerived)
        {
            // Covariance: Derived is Base => IOutput<Derived> is IOutput<Base>.
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
        void InputToVoid(TInput input);

        TInput Input { set; } // TInput set_Input();

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceContravariance(IInput<Derived> inputDerived, IInput<Base> inputBase)
        {
            // Contravariance: Derived is Base => IInput<Base> is IInput<Derived>.
            inputDerived = inputBase;

            // When calling inputDerived.Input, inputBase.Input executes.
            // inputDerived.Input should have Derived input, while inputBase.Input expects Base input.
            // The required Derived output is the expected Base input. This always works.
            inputDerived.InputToVoid(input: new Derived());

            inputDerived.Input = new Derived();
        }
    }

    internal interface IInputOutput<in TInput, out TOutput> // TInput/TOutput is contravariant/covariant for all members using TInput/TOutput.
    {
        void InputToVoid(TInput input);

        TInput Input { set; } // TInput set_Input();

        TOutput ToOutput();

        TOutput Output { get; } // TOutput get_Output();

        void TypeParameterNotUsed();
    }

    internal static partial class Variances
    {
        internal static void GenericInterfaceCovarianceAndContravariance(
            IInputOutput<Derived, Base> inputDerivedOutputBase, IInputOutput<Base, Derived> inputBaseOutputDerived)
        {
            // Covariance and contravariance: Derived is Base => IInputOutput<Base, Derived> is IInputOutput<Derived, Base>.
            inputDerivedOutputBase = inputBaseOutputDerived;

            inputDerivedOutputBase.InputToVoid(new Derived());
            inputDerivedOutputBase.Input = new Derived();
            Base output1 = inputDerivedOutputBase.ToOutput();
            Base output2 = inputDerivedOutputBase.Output;
        }
    }

    internal interface IInvariant<T>
    {
        T Output(); // T is covariant for Output (Func<T>).

        void Input(T input); // T is contravariant for Input (Action<T>).
    }

    internal static partial class Variances
    {
        internal static void OutputVariance()
        {
            // First order functions.
            Func<Base> toBase = () => new Base();
            Func<Derived> toDerived = () => new Derived();

            // Higher-order funcitons.
            ToFunc<Base> toToBase = () => toBase;
            ToFunc<Derived> toToDerived = () => toDerived;

            // Covariance: Derived is Base => ToFunc<Derived> is ToFunc<Base>.
            toToBase = toToDerived;

            // When calling toToBase function, toToDerived executes.
            // toToBase should output Func<Base>, while toToDerived outputs Func<Derived>.
            // The actual Func<Derived> output is the required Func<Base> output. This always works.
            Func<Base> output = toToBase();
        }

        internal delegate TOutput Func<out TOutput>(); // Covariant output type.

        // Equivalent to Func<Func<T>>
        internal delegate Func<TOutput> ToFunc<out TOutput>(); // Covariant output type.

        // Equivalent to Func<Func<Func<T>>>
        internal delegate ToFunc<TOutput> ToToFunc<out TOutput>(); // Covariant output type.

        // Equivalent to Func<Func<Func<Func<T>>>>
        internal delegate ToToFunc<TOutput> ToToToFunc<out TOutput>(); // Covariant output type.

        // ...

#if DEMO
        internal delegate void ActionToVoid<in TTInput>(Action<TTInput> action); 

        internal static void InputVariance()
        {
            ActionToVoid<Derived> derivedToVoidToVoid = (Action<Derived> derivedToVoid) => { };
            ActionToVoid<Base> baseToVoidToVoid = (Action<Base> baseToVoid) => { };
            derivedToVoidToVoid = baseToVoidToVoid;
        }
#endif

        internal static void InputVariance()
        {
            // Higher-order funcitons.
            ActionToVoid<Derived> derivedToVoidToVoid = (Action<Derived> derivedToVoid) => { };
            ActionToVoid<Base> baseToVoidToVoid = (Action<Base> baseToVoid) => { };

            // Covariance: Derived is Base => ActionToVoid<Derived> is ActionToVoid<Base>.
            baseToVoidToVoid = derivedToVoidToVoid;

            // When calling baseToVoidToVoid function, derivedToVoidToVoid executes.
            // baseToVoidToVoid should have Action<Base> input, while derivedToVoidToVoid expects Action<Derived> input.
            // The required Action<Derived> input is the expected Action<Base> input. This always works.
            baseToVoidToVoid(default(Action<Base>));
        }

        internal delegate void Action<in TInput>(TInput input); // Contravariant input type.

        // Equivalent to Action<Action<T>>
        internal delegate void ActionToVoid<out TTInput>(Action<TTInput> action); // Covariant input type.

        // Equivalent to Action<Action<Action<T>>>
        internal delegate void ActionToVoidToVoid<in TTInput>(ActionToVoid<TTInput> actionToVoid); // Contravariant input type.

        // Equivalent to Action<Action<Action<Action<T>>>>
        internal delegate void ActionToVoidToVoidToVoid<out TTInput>(ActionToVoidToVoid<TTInput> actionToVoidToVoid); // Covariant input type.

        // ...
    }

    internal static partial class Variances
    {
        internal static void ArrayCovariance()
        {
            Base[] baseArray = new Base[3];
            Derived[] derivedArray = new Derived[3];

            baseArray = derivedArray;// Array covariance, baseArray refers to a Derived array at runtime.
            Base value = baseArray[0];
            baseArray[1] = new Derived();
            baseArray[2] = new Base(); // ArrayTypeMismatchException, Base cannot be in Derived array.
        }
    }

    internal static partial class Variances
    {
        internal static IEnumerable<TypeDefinition> GetTypesWithVariance(AssemblyDefinition assembly)
        {
            try
            {
                return assembly.Modules.SelectMany(module => module.GetTypes())
                    .Where(type => type.IsPublic && type.HasGenericParameters && type.GenericParameters.Any(argument =>
                        !argument.IsNonVariant));
            }
            catch (TypeLoadException)
            {
                return Enumerable.Empty<TypeDefinition>();
            }
        }

        internal static IEnumerable<AssemblyDefinition> GetAssemblies(string directory) =>
            Directory.EnumerateFiles(directory, "*.dll")
                .Select(file =>
                {
                    try
                    {
                        return AssemblyDefinition.ReadAssembly(file);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                    catch (BadImageFormatException)
                    {
                        return null;
                    }
                })
                .Where(assembly => assembly != null);

        internal static IEnumerable<TypeDefinition> GetTypesWithVariance()
        {
            string coreLibraryPath = typeof(object).GetTypeInfo().Assembly.Location;
            string coreLibraryDirectory = Path.GetDirectoryName(coreLibraryPath);
            return GetAssemblies(coreLibraryDirectory)
                .SelectMany(GetTypesWithVariance)
                .OrderBy(type => type.Name);
        }

        internal static void LinqToObjects(IEnumerable<Base> baseEnumerable, IEnumerable<Derived> derivedEnumerable)
        {
            baseEnumerable = baseEnumerable.Concat(derivedEnumerable);
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
        // T get_Item(int index);
        // void set_Item(int index, T value);

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
        T Current { get; } // T get_Current();
        // T is covariant for function type Func<T>.
    }

    /// <summary>Exposes the enumerator, which supports a simple iteration over a collection of a specified type.</summary>
    /// <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator(); // Func<T> GetEnumerator();
        // T is covariant for higher-order function type Func<Func<T>>.
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
    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable
    {
    }
}
#endif

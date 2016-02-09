namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class Base
    {
    }

    internal class Derived : Base
    {
    }

    internal partial class Methods
    {
        internal static Base DerivedIn_BaseOut(Derived @in)
        {
            return new Base();
        }

        internal static Derived DerivedIn_DerivedOut(Derived @in)
        {
            return new Derived();
        }

        internal static Base BaseIn_BaseOut(Base @in)
        {
            return new Base();
        }

        internal static Derived BaseIn_DerivedOut(Base @in)
        {
            return new Derived();
        }
    }

    internal static partial class NonGenericDelegate
    {
        internal delegate Base DerivedIn_BaseOut(Derived @in);

        internal static void Bind()
        {
            // Binding: DerivedIn_BaseOut delegate type and DerivedIn_BaseOut method have exactly the same signature.
            DerivedIn_BaseOut derivedIn_BaseOut = Methods.DerivedIn_BaseOut;

            // When calling derivedIn_BaseOut delegate instance, DerivedIn_BaseOut method executes.
            Base @out = derivedIn_BaseOut(@in: new Derived());
        }
    }

    internal static partial class NonGenericDelegate
    {
        internal static void Covariance()
        {
            // Covariance: Derived "is a" Base => DerivedIn_DerivedOut "is a" DerivedIn_BaseOut.
            DerivedIn_BaseOut derivedIn_DerivedOut = Methods.DerivedIn_DerivedOut;

            // When calling derivedIn_BaseOut delegate instance, DerivedIn_DerivedOut method executes.
            // derivedIn_BaseOut should output a Base object, while DerivedIn_DerivedOut outputs a Derived object.
            // The actual Derived object "is a" required Base output. This binding always works.
            Base @out = derivedIn_DerivedOut(@in: new Derived());
        }
    }

    internal static partial class NonGenericDelegate
    {
        internal static void Contravariance()
        {
            // Contravariance: Derived is a Base => BaseIn_BaseOut is a DerivedIn_BaseOut.
            DerivedIn_BaseOut derivedIn_BaseOut = Methods.BaseIn_BaseOut;

            // When calling derivedIn_BaseOut delegate instance, BaseIn_BaseOut method executes.
            // derivedIn_BaseOut should have a Derived input, while BaseIn_BaseOut requires a Base input.
            // The actual Derived object "is a" required Base input. This binding always works.
            Base @out = derivedIn_BaseOut(@in: new Derived());
        }
    }
    internal static partial class NonGenericDelegate
    {
        internal static void CovarianceAndContravariance()
        {
            // Covariance and contravariance: Derived is a Base => BaseIn_DerivedOut is a DerivedIn_BaseOut. 
            DerivedIn_BaseOut derivedIn_BaseOut = Methods.BaseIn_DerivedOut;

            // When calling derivedInBaseOut delegate instance, BaseIn_DerivedOut method executes.
            // derivedIn_BaseOut should have a Derived input, while BaseIn_DerivedOut requires a Base input.
            // derivedIn_BaseOut should output a Base object, while BaseIn_DerivedOut outputs a Derived object. 
            // This binding always works.
            Base @out = derivedIn_BaseOut(@in: new Derived());
        }
    }

    internal static partial class NonGenericDelegate
    {
        internal delegate Derived BaseIn_DerivedOut(Base @base);

        internal static void InvalidVariance()
        {
#if ERROR
        // baseIn_DerivedOut should output a Derived object, while BaseIn_DerivedOut outputs a Base object. 
        // Base is not Derived, the following binding cannot be compiled.
        BaseIn_DerivedOut baseIn_DerivedOut1 = Methods.BaseIn_BaseOut;

        // baseIn_DerivedOut should have a Base input, while DerivedIn_BaseOut required a Derived output.
        // Base is not a Derived, the following binding cannot be compiled.
        BaseIn_DerivedOut baseIn_DerivedOut2 = Methods.DerivedIn_BaseOut;

        // baseIn_DerivedOut should have a Base input, while DerivedIn_DerivedOut required a Derived input.
        // baseIn_DerivedOut should output a Derived object, while derivedIn_DerivedOut outputs a Base object. 
        // Base is not a Derived, the following binding cannot be compiled.
        BaseIn_DerivedOut baseIn_DerivedOut3 = Methods.DerivedIn_DerivedOut;
#endif
        }
    }

    internal static partial class GenericDelegate
    {
        internal delegate TOut Func<TIn, TOut>(TIn @in);
    }

    internal static partial class GenericDelegate
    {
        internal static void BindMethods()
        {
            // Bind.
            Func<Derived, Base> derivedIn_BaseOut1 = Methods.DerivedIn_BaseOut;

            // Covariance.
            Func<Derived, Base> derivedIn_BaseOut2 = Methods.DerivedIn_DerivedOut;

            // Contravariance.
            Func<Derived, Base> derivedIn_BaseOut3 = Methods.BaseIn_BaseOut;

            // Covariance and contravariance.
            Func<Derived, Base> derivedIn_BaseOut4 = Methods.BaseIn_DerivedOut;
        }
    }

    internal static partial class GenericDelegate
    {
        internal static void BindLambdas()
        {
            Func<Derived, Base> derivedIn_BaseOut = (Derived @in) => new Base();
            Func<Derived, Derived> derivedIn_DerivedOut = (Derived @in) => new Derived();
            Func<Base, Base> baseIn_BaseOut = (Base @in) => new Base();
            Func<Base, Derived> baseIn_DerivedOut = (Base @in) => new Derived();

#if ERROR
        // Covariance.
        derivedIn_BaseOut = derivedIn_DerivedOut;

        // Contravariance.
        derivedIn_BaseOut = baseIn_BaseOut;

        // Covariance and contravariance.
        derivedIn_BaseOut = baseIn_DerivedOut;
#endif
        }
    }

    internal static partial class GenericDelegateWithVariances
    {
        internal delegate TOut Func<in TIn, out TOut>(TIn @in);
    }

    internal static partial class GenericDelegateWithVariances
    {
        internal static void BindMethods()
        {
            // Bind.
            Func<Derived, Base> derivedIn_BaseOut1 = Methods.DerivedIn_BaseOut;

            // Covariance.
            Func<Derived, Base> derivedIn_BaseOut2 = Methods.DerivedIn_DerivedOut;

            // Contravariance.
            Func<Derived, Base> derivedIn_BaseOut3 = Methods.BaseIn_BaseOut;

            // Covariance and contravariance.
            Func<Derived, Base> derivedIn_BaseOut4 = Methods.BaseIn_DerivedOut;
        }

        internal static void BindLambdas()
        {
            Func<Derived, Base> derivedIn_BaseOut = (Derived @in) => new Base();
            Func<Derived, Derived> derivedIn_DerivedOut = (Derived @in) => new Derived();
            Func<Base, Base> baseIn_BaseOut = (Base @in) => new Base();
            Func<Base, Derived> baseIn_DerivedOut = (Base @in) => new Derived();

            // Covariance.
            derivedIn_BaseOut = derivedIn_DerivedOut;

            // Contravariance.
            derivedIn_BaseOut = baseIn_BaseOut;

            // Covariance and ontravariance.
            derivedIn_BaseOut = baseIn_DerivedOut;
        }
    }

    internal static partial class GenericDelegateWithVariances
    {
#if ERROR
        // CS1961 Invalid variance: The type parameter 'TOut' must be covariantly valid on 'GenericDelegateWithVariances.Func<TOut>.Invoke()'. 'TOut' is contravariant.
        internal delegate TOut Func<in TOut>();

        // CS1961 Invalid variance: The type parameter 'TIn' must be contravariantly valid on 'GenericDelegateWithVariances.Action<TIn>.Invoke(TIn)'. 'TIn' is covariant.
        internal delegate void Action<out TIn>(TIn @in);

        // CS1961 Invalid variance: The type parameter 'TOut' must be covariantly valid on 'GenericDelegateWithVariances.Func<TIn, TOut>.Invoke(TIn)'. 'TOut' is contravariant.
        // CS1961 Invalid variance: The type parameter 'TIn' must be contravariantly valid on 'GenericDelegateWithVariances.Func<TIn, TOut>.Invoke(TIn)'. 'TIn' is covariant.
        internal delegate TOut Func<out TIn, in TOut>(TIn @in);
#endif
    }

    internal static partial class GenericInterface
    {
        internal interface IOut<TOut> // TOut is only used as output.
        {
            TOut Out1(); // TOut is covariant for Out1 (Func<TOut>).

            TOut Out2(object @in); // TOut is covariant for Out2 (Func<object, TOut>).

            TOut Out3 { get; } // TOut is covariant for Out3's getter (Func<object, TOut>).
        }

        internal interface IIn<TIn> // TIn is only used as input.
        {
            void In1(TIn @in); // TIn is contravariant for In1 (Action<TIn>).

            object In2(TIn @in); // TIn is contravariant for In2 (Func<TIn, object>).

            TIn In3 { set; } // TIn is contravariant for In3's setter (Action<TIn>).
        }

        internal interface IIn_Out<T>
        {
            T Out(); // T is covariant for Out (Func<T>).

            void In(T @in); // T is contravaraint for In (Action<T>).
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal interface IOut<out TOut> // TOut is covariant for all members of interface.
        {
            TOut Out1();

            TOut Out2(object @in);

            TOut Out3 { get; } // TOut get_Out3();
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal static void Covariance()
        {
            IOut<Base> baseOut = default(IOut<Base>);
            IOut<Derived> derivedOut = default(IOut<Derived>);

            // Covariance: Derived "is a" Base => IOut<Derived> "is a" IOut<Base>.
            baseOut = derivedOut;

            // So that, when calling baseOut.Out1, the underlying derivedOut.Out1 executes.
            // derivedOut.Out1 method (Func<Derived>) "is a" baseOut.Out1 method (Func<Base>).
            Base out1 = baseOut.Out1();

            // When calling baseOut.Out2, the underlying derivedOut.Out2 executes.
            // derivedOut.Out2 (Func<object, Derived>) "is a" baseOut.Out2 (Func<object, Base>).
            Base out2 = baseOut.Out2(@in: new object());

            // Out3 property is getter only. The getter is a get_Out3 method (Func<TOut>).
            // derivedOut.Out3 getter (Func<Derived>) "is a" baseOut.Out3 getter (Func<Base>).
            Base out3 = baseOut.Out3;

            // So, IOut<Derived> interface "is an" IOut<Base> interface. Above binding always works.
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal interface IIn<in TIn> // TIn is contravariant for all members of interface.
        {
            void In1(TIn @in);

            object In2(TIn @in);

            TIn In3 { set; } // void set_In3(TIn @in);
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal static void Contravariance()
        {
            IIn<Derived> derivedIn = default(IIn<Derived>);
            IIn<Base> baseIn = default(IIn<Base>);

            // Contravariance: Derived "is a" Base => IIn<Base> "is a" IIn<Derived>.
            derivedIn = baseIn;

            // When calling derivedIn.In1, the underlying baseIn.In1 executes.
            // baseIn.In1 method (Action<Base>) "is a" derivedIn.In1 method (Action<Derived>).
            derivedIn.In1(new Derived());

            // When calling derivedIn.In2, the underlying baseIn.In2 executes.
            // baseIn.In2 (Func<Base, object>) "is a" derivedIn.In2 (Func<Derived, object>).
            object @out = derivedIn.In2(new Derived());

            // In3 property is setter only. The setter is a set_In3 method (Action<TOut>).
            // baseIn.In3 setter (Action<Base>) "is a" derivedIn.In3 setter (Action<Base>).
            derivedIn.In3 = new Derived();

            // So, IIn<Base> interface "is an" IIn<Derived> interface. Above binding always works.
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal interface IIn_Out<in TIn, out TOut>
        {
            void In(TIn @in);
            TOut Out();
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal static void CovarianceAndContravariance()
        {
            IIn_Out<Derived, Base> derivedIn_BaseOut = default(IIn_Out<Derived, Base>);
            IIn_Out<Base, Derived> baseIn_DerivedOut = default(IIn_Out<Base, Derived>);

            // Covariance and contravariance: IIn_Out<Base, Derived> "is a" IIn_Out<Derived, Base>.
            derivedIn_BaseOut = baseIn_DerivedOut;
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal static void IEnumerableCovariance()
        {
            IEnumerable<Derived> derivedEnumerable = Enumerable.Empty<Derived>();
            IEnumerable<Base> baseEnumerable = Enumerable.Empty<Base>();

            // IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second);
            baseEnumerable = baseEnumerable.Concat(derivedEnumerable);
        }
    }

    internal static partial class GenericInterfaceWithVariances
    {
        internal static void IEnumerable()
        {
            IEnumerable<Derived> derivedEnumerable = Enumerable.Empty<Derived>();
            IEnumerable<Base> baseEnumerable = Enumerable.Empty<Base>();
            baseEnumerable = baseEnumerable.Concat(derivedEnumerable);
            baseEnumerable = baseEnumerable.Concat(derivedEnumerable.Cast<Base>());
        }
    }

    internal static partial class HigherOrderFunction
    {
        internal static void FirstOrder_HigherOrder()
        {
            { Action action = () => { }; } // First-order function.
            Action<Action> actionIn = action => action(); // Higher-order function

            Func<object> func = () => new object(); // First-order function.
            Func<Func<object>> funcOut = () => func; // Higher-order function
        }
    }

    internal static partial class HigherOrderFunction
    {
        // System.Action<T>.
        internal delegate void Action<in TIn>(TIn @in);

        internal static void ContravarianceForFirstOrder()
        {
            // First-order functions.
            Action<Derived> derivedIn = (Derived @in) => { };
            Action<Base> baseIn = (Base @in) => { };

            // Contravariance of input: Action<Base> "is a" Action<Derived>.
            // Or: T is contravariant for Action<in T>.
            derivedIn = baseIn;
        }
    }

    internal static partial class HigherOrderFunction
    {
#if ERROR
        internal delegate void ActionIn<in T>(Action<T> action);

        internal static void ContravarianceOfInput()
        {
            // Higher-order funcitons:
            ActionIn<Derived> derivedInIn = (Action<Derived> derivedIn) => derivedIn(new Derived());
            ActionIn<Base> baseInIn = (Action<Base> baseIn) => baseIn(new Base());

            // Regarding Action<Base> "is a" ActionIn<Derived>,
            // assumes there is still contravariance of input,
            // which is, ActionIn<Base> "is a" ActionIn<Derived>
            derivedInIn = baseInIn;

            // When calling baseInIn, derivedInIn executes.
            // baseInIn should have a Action<Base> input, while derivedInIn requires a Action<Derived> input.
            // The actual Action<Base> "is a" required Action<Derived>. This binding should always works.
            baseInIn(new Action<Base>((Base @in) => { }));
        }
#endif
    }

    internal static partial class HigherOrderFunction
    {
        // Action<Action<T>>
        internal delegate void ActionIn<out T>(Action<T> action);

        internal static void CovarianceOfInput() // Not contravariance.
        {
            // Higher-order funcitons:
            ActionIn<Derived> derivedInIn = (Action<Derived> derivedIn) => derivedIn(new Derived());
            ActionIn<Base> baseInIn = (Action<Base> baseIn) => baseIn(new Base());

            // Not derivedInIn = baseInIn;
            baseInIn = derivedInIn;

            // When calling baseInIn, derivedInIn executes.
            // baseInIn should have a Action<Base> input, while derivedInIn requires a Action<Derived> input.
            // The actual Action<Base> "is a" required Action<Derived>. This binding always works.
            baseInIn(new Action<Base>((Base @in) => { }));
        }
    }

    internal static partial class HigherOrderFunction
    {
        internal delegate Func<TOut> FuncOut<out TOut>();

        internal static void CovarianceOfOutput()
        {
            // First order functions.
            Func<Base> baseOut = () => new Base();
            Func<Derived> derivedOut = () => new Derived();
            // T is covarianct for Func<T>.
            baseOut = derivedOut;

            // Higher-order funcitons:
            FuncOut<Base> baseOutOut = () => baseOut;
            FuncOut<Derived> derivedOutOut = () => derivedOut;

            // Covariance of output: FuncOut<Derived> "is a" FuncOut<Base>
            baseOutOut = derivedOutOut;

            // When calling baseOutOut, derivedOutOut executes.
            // baseOutOut should output a Func<Base>, while derivedOutOut outputs a Func<Derived>.
            // The actual Func<Derived> "is a" required Func<Base>. This binding always works.
            baseOut = baseOutOut();
        }
    }

    internal static class OutputCovarianceForHigherOrder
    {
        internal delegate T Func<out T>(); // Covariant T as output.

        // Func<Func<T>>
        internal delegate Func<T> FuncOut<out T>(); // Covariant T as output.

        // Func<Func<Func<T>>>
        internal delegate FuncOut<T> FuncOutOut<out T>(); // Covariant T as output.

        // Func<Func<Func<Func<T>>>>
        internal delegate FuncOutOut<T> FuncOutOutOut<out T>(); // Covariant T as output.

        // ...
    }

    internal static class InputVarianceReversalForHigherOrder
    {
        internal delegate void Action<in T>(T @in); // Contravariant T as input.

        // Action<Action<T>>
        internal delegate void ActionIn<out T>(Action<T> action); // Covariant T as input.

        // Action<Action<Action<T>>>
        internal delegate void ActionInIn<in T>(ActionIn<T> actionIn); // Contravariant T as input.

        // Action<Action<Action<Action<T>>>>
        internal delegate void ActionInInIn<out T>(ActionInIn<T> actionInIn); // Covariant T as input.

        // ...
    }

    internal static partial class Array
    {
        internal static void Covariance()
        {
            // IList<Base> baseArray = new Base[2];
            Base[] baseArray = new Base[2];

            // IList<Derived> derivedArray = new Derived[3];
            Derived[] derivedArray = new Derived[2];

            // T of IList<T> is invariant,
            // so logically binding IList<derivedArray> to IList<Base> could not be compiled.
            // But C# compiles it, to be compliant with Java :(
            baseArray = derivedArray; // Array covariance.

            // At runtime, baseArray refers to a Derived array.
            // So A Derived object can be an element of baseArray[0].
            baseArray[0] = new Derived() /* as object */;

            // At runtime, baseArray refers to a Derived array.
            // A Base object "is not a" Derivd object.
            // And ArrayTypeMismatchException is thrown at runtime.
            baseArray[1] = new Base() /* as object */;
        }
    }

    internal static partial class Array
    {
        internal static void ProcessArray(Base[] array)
        {
            array[0] = new Base(); // ArrayTypeMismatchException.
        }

        internal static void CallProcessArray()
        {
            Derived[] array = new Derived[1];
            ProcessArray(array); // Array covariance. Compliable.
        }
    }

    internal static partial class Array
    {
        internal static void ValueType()
        {
            object[] objectArray = new object[1];
            int[] int32Array = new int[1];
#if ERROR
            // No covariance.
            objectArray = int32Array;
#endif
        }
    }

    public static partial class ReflectionHelper
    {
        public static IEnumerable<Type> GetTypesWithVariance(Assembly assembly)
        {
            try
            {
                return assembly.ExportedTypes.Where(type =>
                    type.IsGenericTypeDefinition && type.GetGenericArguments().Any(argument =>
                        (argument.GenericParameterAttributes & GenericParameterAttributes.Covariant)
                        == GenericParameterAttributes.Covariant
                        || (argument.GenericParameterAttributes & GenericParameterAttributes.Contravariant)
                        == GenericParameterAttributes.Contravariant));
            }
            catch (TypeLoadException)
            {
                return Enumerable.Empty<Type>();
            }
        }
    }

    public static partial class ReflectionHelper
    {
        public static IEnumerable<Assembly> GetAssemblies
            (string directory) => Directory.EnumerateFiles(directory, "*.dll")
                .Select(file =>
                    {
                        try
                        {
                            return Assembly.Load(AssemblyName.GetAssemblyName(file));
                        }
                        catch (BadImageFormatException)
                        {
                            return null;
                        }
                    })
                .Where(assembly => assembly != null);
    }

    public static partial class ReflectionHelper
    {
        public static IEnumerable<Type> GetTypesWithVariance()
        {
            string mscorlibPath = typeof(object).Assembly.CodeBase;
            string directory = Path.GetDirectoryName(new Uri(mscorlibPath).AbsolutePath);
            return GetAssemblies(directory)
                .SelectMany(GetTypesWithVariance)
                .OrderBy(type => type.Name);
        }
    }
}
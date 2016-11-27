namespace Dixin.Linq.CategoryTheory
{
#if DEMO
    public interface IFunctor<in TSourceCategory, out TTargetCategory, TFunctor<>>
        where TSourceCategory : ICategory<TSourceCategory>
        where TTargetCategory : ICategory<TTargetCategory>
        where TFunctor<> : IFunctor<TSourceCategory, TTargetCategory, TFunctor<>>
        {
            IMorphism<TFunctor<TSource>, TFunctor<TResult>, TTargetCategory> Select<TSource, TResult>(
                IMorphism<TSource, TResult, TSourceCategory> selector);
        }

    public interface IEndofunctor<TCategory, TEndofunctor<>>
        : IFunctor<TCategory, TCategory, TEndofunctor<>>
        where TCategory : ICategory<TCategory>
        where TEndofunctor<> : IFunctor<TEndofunctor, TEndofunctor<>>
    {
        IMorphism<TEndofunctor<TSource>, TEndofunctor<TResult>, TCategory> Select<TSource, TResult>(
            IMorphism<TSource, TResult, TCategory> selector);
    }

    // EnumerableFunctor<>: DotNet -> DotNet 
    public class EnumerableFunctor<T> : IFunctor<DotNet, DotNet, EnumerableFunctor<>>
    {
        public IMorphism<EnumerableFunctor<TSource>, EnumerableFunctor<TResult>, DotNet> Select<TSource, TResult>(
            IMorphism<TSource, TResult, DotNet> selector)
        {
            // ...
        }
    }

    public interface IBinaryFunctor<in TSourceCategory1, in TSourceCategory2, out TTargetCategory, TBinaryFunctor< , >>
        where TSourceCategory1 : ICategory<TSourceCategory1>
        where TSourceCategory2 : ICategory<TSourceCategory2>
        where TTargetCategory : ICategory<TTargetCategory>
        where TBinaryFunctor< , > : IBinaryFunctor<TSourceCategory1, TSourceCategory2, TTargetCategory, TBinaryFunctor< , >>
    {
        IMorphism<TBinaryFunctor<TSource1, TSource2>, TBinaryFunctor<TResult1, TResult2>, TTargetCategory> Select<TSource1, TSource2, TResult1, TResult2>(
            IMorphism<TSource1, TResult1, TSourceCategory1> selector1, IMorphism<TSource2, TResult2, TSourceCategory2> selector2);
    }

    public interface IMonoidalCategory<TMonoidalCategory, out TBinaryFunctor< , >> 
        : ICategory<TMonoidalCategory>
        where TBinaryFunctor< , > : IBinaryFunctor<TMonoidalCategory, TMonoidalCategory, TMonoidalCategory, TBinaryFunctor< , >>
    {
        TBinaryFunctor<T1, T2> x<T1, T2>(T1 value1, T2 value2);
    }

    public interface IMonoidalFunctor<in TSourceCategory, out TTargetCategory, TSourceBinaryFunctor< , >, TTargetBinaryFunctor< , >, TSourceUnit, TTargetUnit, TMonoidalFunctor<>> 
        : IFunctor<TSourceCategory, TTargetCategory, TMonoidalFunctor<>>
        where TSourceCategory : IMonoidalCategory<TSourceCategory, TSourceBinaryFunctor< , >>
        where TTargetCategory : IMonoidalCategory<TTargetCategory, TTargetBinaryFunctor< , >>
        where TSourceBinaryFunctor< , > : IBinaryFunctor<TSourceCategory, TSourceCategory, TSourceCategory, TSourceBinaryFunctor< , >>
        where TTargetBinaryFunctor< , > : IBinaryFunctor<TTargetCategory, TTargetCategory, TTargetCategory, TTargetBinaryFunctor< , >>
        where TMonoidalFunctor<> : IMonoidalFunctor<TSourceCategory, TTargetCategory, TSourceBinaryFunctor< , >, TTargetBinaryFunctor< , >, MonoidalFunctor<>>
    {
        // φ: TTargetBinaryFunctor<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> => TMonoidalFunctor<TSourceBinaryFunctor<T1, T2>>
        TMonoidalFunctor<TSourceBinaryFunctor<T1, T2>> Binary<T1, T2>(
            TTargetBinaryFunctor<TMonoidalFunctor<T1>, TMonoidalFunctor<T2>> binaryFunctor);

        // ι: TTargetUnit -> TMonoidalFunctor<TSourceUnit>
        TMonoidalFunctor<TSourceUnit> Unit(TTargetUnit unit);
    }

    // Does not compile.
    public interface IMonad<TCategory, TBinaryFunctor< , >, TUnit, TMonad<>>
        : IMonoidalFunctor<TCategory, TCategory, TBinaryFunctor< , >, TBinaryFunctor< , >, TUnit, TUnit, TMonad<>>
        where TMonad<> : IMonad<TCategory, TBinaryFunctor< , >, TBinaryFunctor< , >, TMonad<>>
        where TCategory : IMonoidalCategory<TCategory, TBinaryFunctor< , >>
    {
        // Select: (TSource -> TResult) -> (TMonad<TSource> -> TMonad<TResult>)

        // φ: TBinaryFunctor<TMonad<T1>, TMonad<T2>> => TMonad<TBinaryFunctor<T1, T2>>

        // ι: TUnit -> TMonad<TUnit>

        // μ: TMonad<> ∘ TMonad<> => TMonad<>
        TMonad<TSource> Flatten<TSource>(TMonad<TMonad<TSource>> source);

        // η: Id<T> => TMonad<T>, equivalent to T -> TMonad<T>
        TMonad<TSource> Monad<TSource>(TSource value);
    }

    // Does not compile.
    public interface IDotNetMonad<TDotNetMonad<>> 
        : IMonad<DotNet, Lazy< , >, Unit, TDotNetMonad<>>
        where TDotNetMonad<> : IDotNetMonad<TDotNetMonad<>>
    {
        // Select: (TSource -> TResult) -> (TDotNetMonad<TSource> -> TDotNetMonad<TResult>)

        // φ: Lazy<TDotNetMonad<T1>, TDotNetMonad<T2>> => TDotNetMonad<Lazy<T1, T2>>

        // ι: TUnit -> TDotNetMonad<TUnit>

        // μ: TDotNetMonad<> ∘ TDotNetMonad<> => TDotNetMonad<>

        // η: Lazy<T> => TDotNetMonad<T>, equivalent to T -> TDotNetMonad<T>
    }

    public interface IApplicativeFunctor<TApplicativeFunctor<>> // Lax monoidal endofunctor in DotNet category.
        : IFunctor<DotNet, DotNet, TApplicativeFunctor<>>
        where TApplicativeFunctor<> : IApplicativeFunctor<TApplicativeFunctor<>>
    {
        TApplicativeFunctor<TResult> Apply<TSource, TResult>(
            TApplicativeFunctor<Func<TSource, TResult>> selectorWrapper, TApplicativeFunctor<TSource> source);

        TApplicativeFunctor<T> Pure<T>(T value);
    }

    public interface IDotNetMonoidalFunctor<T> // F<>
        : IMonoidalFunctor<DotNet, DotNet, Lazy< , >, Lazy< , >, Unit, Unit, IDotNetMonoidalFunctor<>>
    {
        // φ: Lazy<F<T1>, F<T2>> => F<Lazy<T1, T2>>
        // IDotNetMonoidalFunctor<Lazy<T1, T2>> Binary<T1, T2>(
        //     Lazy<IDotNetMonoidalFunctor<T1>, IDotNetMonoidalFunctor<T2>> binaryFunctor);

        // φ: Lazy<F<T1>, F<T2>> => F<Lazy<T1, T2>>
        // is equivalent to
        // φ: (F<T1>, F<T2>>) => F<Lazy<T1, T2>>
        IDotNetMonoidalFunctor<Lazy<T1, T2>> Binary<T1, T2>(
            IDotNetMonoidalFunctor<T1> functor1, IDotNetMonoidalFunctor<T2> functor2);

        // ι: Unit -> F<Unit>
        // IDotNetMonoidalFunctor<Unit> Unit(Unit unit);
    }
#endif
}

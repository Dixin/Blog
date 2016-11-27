namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface ICategory<TObject, TMorphism>
    {
        IEnumerable<TObject> Objects { get; }

        TMorphism Compose(TMorphism morphism2, TMorphism morphism1);

        TMorphism Id(TObject @object);
    }

    public class Int32Category : ICategory<int, BinaryExpression>
    {
        public IEnumerable<int> Objects
        {
            get
            {
                for (int int32 = int.MinValue; int32 <= int.MaxValue; int32++)
                {
                    yield return int32;
                }
            }
        }

        public BinaryExpression Compose(BinaryExpression morphism2, BinaryExpression morphism1) =>
            Expression.LessThanOrEqual(morphism2.Left, morphism1.Right); // (Y <= Z) ∘ (X <= Y) => X <= Z.

        public BinaryExpression Id(int @object) =>
            Expression.GreaterThanOrEqual(Expression.Constant(@object), Expression.Constant(@object)); // X <= X.
    }

    public static partial class FuncExtensions
    {
        public static Func<TSource, TResult> o<TSource, TMiddle, TResult>( // After.
            this Func<TMiddle, TResult> function2, Func<TSource, TMiddle> function1) =>
                value => function2(function1(value));
    }

    public partial class DotNetCategory : ICategory<Type, Delegate>
    {
        public IEnumerable<Type> Objects =>
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.ExportedTypes);

        public Delegate Compose(Delegate morphism2, Delegate morphism1) =>
            // return (Func<TSource, TResult>)Functions.Compose<TSource, TMiddle, TResult>(
            //    (Func<TMiddle, TResult>)morphism2, (Func<TSource, TMiddle>)morphism1);
            (Delegate)typeof(FuncExtensions).GetMethod(nameof(FuncExtensions.o))
                .MakeGenericMethod( // TSource, TMiddle, TResult.
                    morphism1.Method.GetParameters().Single().ParameterType,
                    morphism1.Method.ReturnType,
                    morphism2.Method.ReturnType)
                .Invoke(null, new object[] { morphism2, morphism1 });

        public Delegate Id(Type @object) => // Functions.Id<TSource>
            Delegate.CreateDelegate(
                type: typeof(Func<,>).MakeGenericType(@object, @object),
                method: typeof(Functions).GetMethod(nameof(Functions.Id)).MakeGenericMethod(@object));
    }
}

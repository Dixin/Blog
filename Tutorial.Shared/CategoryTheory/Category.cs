namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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

#if DEMO
    public static partial class FuncExtensions
    {
        public static Func<TSource, TResult> o<TSource, TMiddle, TResult>( // After.
            this Func<TMiddle, TResult> function2, Func<TSource, TMiddle> function1) =>
                value => function2(function1(value));
    }
#endif

    public partial class DotNetCategory : ICategory<Type, Delegate>
    {
        public IEnumerable<Type> Objects =>
            SelfAndReferences(typeof(DotNetCategory).GetTypeInfo().Assembly)
                .SelectMany(assembly => assembly.GetExportedTypes());

        public Delegate Compose(Delegate morphism2, Delegate morphism1) =>
            // return (Func<TSource, TResult>)Functions.Compose<TSource, TMiddle, TResult>(
            //    (Func<TMiddle, TResult>)morphism2, (Func<TSource, TMiddle>)morphism1);
            (Delegate)typeof(Tutorial.FuncExtensions).GetTypeInfo().GetMethod(nameof(Tutorial.FuncExtensions.o))
                .MakeGenericMethod( // TSource, TMiddle, TResult.
                    morphism1.GetMethodInfo().GetParameters().Single().ParameterType,
                    morphism1.GetMethodInfo().ReturnType,
                    morphism2.GetMethodInfo().ReturnType)
                .Invoke(null, new object[] { morphism2, morphism1 });

        public Delegate Id(Type @object) => // Functions.Id<TSource>
            typeof(Functions).GetTypeInfo().GetMethod(nameof(Functions.Id)).MakeGenericMethod(@object)
                .CreateDelegate(typeof(Func<,>).MakeGenericType(@object, @object));

        private static IEnumerable<Assembly> SelfAndReferences(
            Assembly self, HashSet<Assembly> selfAndReferences = null)
        {
            selfAndReferences = selfAndReferences ?? new HashSet<Assembly>();
            if (selfAndReferences.Add(self))
            {
                self.GetReferencedAssemblies().ForEach(reference => 
                    SelfAndReferences(Assembly.Load(reference), selfAndReferences));
                return selfAndReferences;
            }
            return Enumerable.Empty<Assembly>(); // Circular or duplicate reference.
        }
    }
}

namespace Dixin.Linq.CSharp
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    internal static partial class ExpressionTree
    {
        internal static void ExpressionLambda()
        {
            // Func<int, bool> isPositive = int32 => int32 > 0;
            Expression<Func<int, bool>> isPositiveExpression = int32 => int32 > 0;
        }
    }

    internal static partial class ExpressionTree
    {
        internal static void CompiledExpressionLambda()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(int), "int32"); // int32 parameter.
            ConstantExpression constantExpression = Expression.Constant(0, typeof(int)); // 0
            BinaryExpression greaterThanExpression = Expression.GreaterThan(
                left: parameterExpression, right: constantExpression); // int32 > 0

            Expression<Func<int, bool>> isPositiveExpression = Expression.Lambda<Func<int, bool>>(
                body: greaterThanExpression, // ... => int32 > 0
                parameters: parameterExpression); // int32 => ...
        }

#if DEMO
        internal static void StatementLambda()
        {
            Expression<Func<int, bool>> isPositiveExpression = int32 =>
            {
                Console.WriteLine(int32);
                return int32 > 0;
            };
        }
#endif

        internal static void StatementLambda()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(int), "int32"); // int32 parameter.
            Expression<Func<int, bool>> isPositiveExpression = Expression.Lambda<Func<int, bool>>(
                // ... => {
                Expression.Block(
                    // Trace.WriteLine(int32);
                    Expression.Call(new Action<int>(Console.WriteLine).Method, parameterExpression),
                    // return int32 > 0;
                    Expression.GreaterThan(parameterExpression, Expression.Constant(0, typeof(int)))),
                // }
                parameterExpression); // int32 => ...
        }

        internal static void ArithmeticalExpression()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;
        }
    }
}

#if DEMO
namespace System.Linq.Expressions
{
    using System.Collections.ObjectModel;
    using System.Reflection;

    public abstract partial class Expression
    {
        public virtual ExpressionType NodeType { get; }

        public virtual Type Type { get; }

        // Other members.
    }

    public class ParameterExpression : Expression
    {
        public string Name { get; }

        // Other members.
    }

    public class ConstantExpression : Expression
    {
        public object Value { get; }

        // Other members.
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; }

        public Expression Right { get; }

        // Other members.
    }

    public abstract class LambdaExpression : Expression
    {
        public Expression Body { get; }

        public ReadOnlyCollection<ParameterExpression> Parameters { get; }

        // Other members.
    }

    public sealed class Expression<TDelegate> : LambdaExpression
    {
        public TDelegate Compile();

        // Other members.
    }

    public abstract partial class Expression
    {
        public static ParameterExpression Parameter(Type type, string name);

        public static ConstantExpression Constant(object value, Type type);

        public static BinaryExpression GreaterThan(Expression left, Expression right);

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters);
    }

    public abstract partial class Expression
    {
        public static BinaryExpression Add(Expression left, Expression right);

        public static BinaryExpression Subtract(Expression left, Expression right);

        public static BinaryExpression Multiply(Expression left, Expression right);

        public static BinaryExpression Divide(Expression left, Expression right);

        public static BinaryExpression Equal(Expression left, Expression right);

        public static UnaryExpression ArrayLength(Expression array);

        public static UnaryExpression Not(Expression expression);

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse);

        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments);

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments);

        public static BlockExpression Block(params Expression[] expressions);

        // Other members.
    }
}
#endif

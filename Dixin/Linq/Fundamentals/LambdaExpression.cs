namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal static partial class Anonymous
    {
        internal static bool IsPositive(int int32)
        {
            return int32 > 0;
        }

        internal static void Delegate()
        {
            Func<int, bool> isPositive1 = new Func<int, bool>(IsPositive);
            Func<int, bool> isPositive2 = IsPositive;
        }

        internal static void AnonymousMethod()
        {
            Func<int, bool> isPositive = delegate (int int32)
                {
                    return int32 > 0;
                };

            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
                {
                    Trace.WriteLine(e.ExceptionObject);
                };
        }

        internal static void Lambda()
        {
            Func<int, bool> isPositive = (int int32) =>
                {
                    return int32 > 0;
                };

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
                {
                    Trace.WriteLine(e.ExceptionObject);
                };
        }

        internal static void ExpressionLambda()
        {
            Func<int, bool> isPositive = int32 => int32 > 0;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Trace.WriteLine(e.ExceptionObject);
        }

        internal static void StatementLambda()
        {
            Func<int, bool> isPositive = int32 =>
                {
                    Console.WriteLine(int32);
                    return int32 > 0;
                };
        }

        internal static void CallAnonymousMethod()
        {
            bool isPositive = new Func<int, bool>(delegate (int int32) { return int32 > 0; })(1);

            new Action<bool>(delegate (bool value) { Trace.WriteLine(value); })(isPositive);
        }

        internal static void CallLambda()
        {
            bool isPositive = new Func<int, bool>(int32 => int32 > 0)(1);

            new Action<bool>(value => Trace.WriteLine(value))(isPositive);
        }
    }

    internal static class CompiledAnonymous
    {
        [CompilerGenerated]
        private static Func<int, bool> cachedAnonymousMethodDelegate0;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler cachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static bool AnonymousMethod0(int int32)
        {
            return int32 > 0;
        }

        [CompilerGenerated]
        private static void AnonymousMethod1(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
        }

        internal static void AnonymousMethod()
        {
            Func<int, bool> isPositive = cachedAnonymousMethodDelegate0
                ?? (cachedAnonymousMethodDelegate0 = new Func<int, bool>(AnonymousMethod0));
            AppDomain.CurrentDomain.UnhandledException += cachedAnonymousMethodDelegate1
                ?? (cachedAnonymousMethodDelegate1 = new UnhandledExceptionEventHandler(AnonymousMethod1));
        }
    }

    internal static partial class ExpressionTree
    {
        internal static void ExpressionLambda()
        {
            Expression<Func<int, bool>> isPositiveExpression = int32 => int32 > 0;
        }
    }

    internal static partial class ExpressionTree
    {
        internal static void CompiledExpressionLambda()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(int), "int32"); // int32
            ConstantExpression constantExpression = Expression.Constant(0, typeof(int)); // 0
            BinaryExpression greaterThanExpression = Expression.GreaterThan(
                left: parameterExpression, right: constantExpression); // int32 > 0

            Expression<Func<int, bool>> isPositiveExpression = Expression.Lambda<Func<int, bool>>(
                body: greaterThanExpression, // => int32 > 0
                parameters: parameterExpression); // int32 =>
        }

#if DEMO
        internal static void StatementLambda()
        {
            Expression<Func<int, bool>> statementLambda1 = int32 => { return int32 > 0; };

            Expression<Func<int, bool>> statementLambda2 = int32 =>
                {
                    Console.WriteLine(int32);
                    return int32 > 0;
                };
        }
#endif

        internal static void StatementLambda()
        {
            // For single statement, syntactic sugar works.
            Expression<Func<int, bool>> statementLambda1 = int32 => int32 > 0;

            // Above lambda expression is compiled to:
            ParameterExpression int32Parameter = Expression.Parameter(typeof(int), "int32");
            Expression<Func<int, bool>> compiledStatementLambda1 = Expression.Lambda<Func<int, bool>>(
                Expression.GreaterThan(int32Parameter, Expression.Constant(0, typeof(int))), // int32 > 0
                int32Parameter); // int32 =>

            // For multiple statements, syntactic sugar is not available. The expression tree has to be built manually.
            Expression<Func<int, bool>> statementLambda2 = Expression.Lambda<Func<int, bool>>(
                // {
                Expression.Block(
                    // Console.WriteLine(int32);
                    Expression.Call(new Action<int>(Console.WriteLine).Method, int32Parameter),
                    // return int32 > 0;
                    Expression.GreaterThan(int32Parameter, Expression.Constant(0, typeof(int)))),
                // }
                int32Parameter); // int32 =>
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

        // Other methods.
    }
}
#endif

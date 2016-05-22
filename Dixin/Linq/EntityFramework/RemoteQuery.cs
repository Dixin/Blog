namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    using Dixin.Linq.Fundamentals;

    using Array = System.Array;

    public class InfixVisitor : BinaryArithmeticExpressionVisitor<string>
    {
        protected override string VisitAdd
            (BinaryExpression add, LambdaExpression expression) => this.VisitBinary(add, "+", expression);

        protected override string VisitConstant
            (ConstantExpression constant, LambdaExpression expression) => constant.Value.ToString();

        protected override string VisitDivide
            (BinaryExpression divide, LambdaExpression expression) => this.VisitBinary(divide, "/", expression);

        protected override string VisitMultiply
            (BinaryExpression multiply, LambdaExpression expression) => this.VisitBinary(multiply, "*", expression);

        protected override string VisitParameter
            (ParameterExpression parameter, LambdaExpression expression) => $"@{parameter.Name}";

        protected override string VisitSubtract
            (BinaryExpression subtract, LambdaExpression expression) => this.VisitBinary(subtract, "-", expression);

        private string VisitBinary
            (BinaryExpression binary, string @operator, LambdaExpression expression) =>
                $"({this.VisitNode(binary.Left, expression)} {@operator} {this.VisitNode(binary.Right, expression)})";
    }

    internal static partial class ExpressionTree
    {
        internal static void Translate()
        {
            InfixVisitor infixVisitor = new InfixVisitor();
            Expression<Func<double, double, double>> expression1 = (a, b) => a * a + b * b;
            string infixExpression1 = infixVisitor.VisitBody(expression1);
            Trace.WriteLine(infixExpression1); // ((@a * @a) + (@b * @b))

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;
            string infixExpression2 = infixVisitor.VisitBody(expression2);
            Trace.WriteLine(infixExpression2); // (((@a + @b) - ((@c * @d) / 2)) + (@e * 3))
        }
    }

    public static partial class BinaryArithmeticTranslator
    {
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal static double ExecuteSql(
            string connection,
            string arithmeticExpression,
            IEnumerable<KeyValuePair<string, double>> parameters)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            using (SqlCommand command = new SqlCommand($"SELECT {arithmeticExpression}", sqlConnection))
            {
                sqlConnection.Open();
                parameters.ForEach(parameter => command.Parameters.AddWithValue(parameter.Key, parameter.Value));
                return (double)command.ExecuteScalar();
            }
        }
    }

    public static partial class BinaryArithmeticTranslator
    {
        private static readonly InfixVisitor InfixVisitor = new InfixVisitor();

        public static TDelegate Sql<TDelegate>(
            Expression<TDelegate> expression, string connection = ConnectionStrings.LocalDb)
            where TDelegate : class
        {
            DynamicMethod dynamicMethod = new DynamicMethod(
                string.Empty,
                expression.ReturnType,
                expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                typeof(BinaryArithmeticTranslator).Module);
            EmitIL(dynamicMethod.GetILGenerator(), InfixVisitor.VisitBody(expression), expression, connection);
            return dynamicMethod.CreateDelegate(typeof(TDelegate)) as TDelegate;
        }

        private static void EmitIL<TDelegate>(ILGenerator ilGenerator, string infixExpression, Expression<TDelegate> expression, string connection)
        {
            // Dictionary<string, double> dictionary = new Dictionary<string, double>();
            ilGenerator.DeclareLocal(typeof(Dictionary<string, double>));
            ilGenerator.Emit(
                OpCodes.Newobj,
                typeof(Dictionary<string, double>).GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc_0);

            for (int index = 0; index < expression.Parameters.Count; index++)
            {
                // dictionary.Add($"@{expression.Parameters[i].Name}", args[i]);
                ilGenerator.Emit(OpCodes.Ldloc_0); // dictionary.
                ilGenerator.Emit(OpCodes.Ldstr, $"@{expression.Parameters[index].Name}");
                ilGenerator.Emit(OpCodes.Ldarg_S, index);
                ilGenerator.Emit(
                    OpCodes.Callvirt,
                    typeof(Dictionary<string, double>).GetMethod(
                        nameof(Dictionary<string, double>.Add),
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod));
            }

            // BinaryArithmeticTanslator.ExecuteSql(connection, expression, dictionary);
            ilGenerator.Emit(OpCodes.Ldstr, connection);
            ilGenerator.Emit(OpCodes.Ldstr, infixExpression);
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(
                OpCodes.Call,
                typeof(BinaryArithmeticTranslator).GetMethod(
                    nameof(ExecuteSql),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod));

            // Returns the result of ExecuteSql.
            ilGenerator.Emit(OpCodes.Ret);
        }
    }

    internal static partial class ExpressionTree
    {
        internal static void ExecuteSql()
        {
            Expression<Func<double, double, double>> expression1 = (a, b) => a * a + b * b;
            Func<double, double, double> local1 = expression1.Compile();
            Trace.WriteLine(local1(1, 2)); // 5
            Func<double, double, double> remote1 = BinaryArithmeticTranslator.Sql(expression1);
            Trace.WriteLine(remote1(1, 2)); // 5

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;
            Func<double, double, double, double, double, double> local2 = expression2.Compile();
            Trace.WriteLine(local2(1, 2, 3, 4, 5)); // 12
            Func<double, double, double, double, double, double> remote2 = BinaryArithmeticTranslator.Sql(expression2);
            Trace.WriteLine(remote2(1, 2, 3, 4, 5)); // 12
        }
    }
}

#if DEMO
namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IQueryable : IEnumerable
    {
        Expression Expression { get; }

        Type ElementType { get; }

        IQueryProvider Provider { get; }
    }

    public interface IOrderedQueryable : IQueryable, IEnumerable
    {
    }

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable
    {
    }

    public interface IOrderedQueryable<out T> : IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable
    {
    }
}

namespace System.Linq
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        // More query methods...
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector);

        public static IQueryable<TSource> Concat<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2);

        // More query methods...
    }
}

namespace System.Linq
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Enumerable
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);
    }

    public static class Queryable
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);
    }
}
#endif

namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    using Tutorial.Functional;

    using Array = System.Array;

    internal class InfixVisitor : BinaryArithmeticExpressionVisitor<string>
    {
        internal override string VisitBody(LambdaExpression expression) => $"SELECT {base.VisitBody(expression)};";

        protected override string VisitAdd(
            BinaryExpression add, LambdaExpression expression) => this.VisitBinary(add, "+", expression);

        protected override string VisitConstant(
           ConstantExpression constant, LambdaExpression expression) => constant.Value.ToString();

        protected override string VisitDivide(
            BinaryExpression divide, LambdaExpression expression) => this.VisitBinary(divide, "/", expression);

        protected override string VisitMultiply(
            BinaryExpression multiply, LambdaExpression expression) => this.VisitBinary(multiply, "*", expression);

        protected override string VisitParameter(
            ParameterExpression parameter, LambdaExpression expression) => $"@{parameter.Name}";

        protected override string VisitSubtract(
            BinaryExpression subtract, LambdaExpression expression) => this.VisitBinary(subtract, "-", expression);

        private string VisitBinary(
            BinaryExpression binary, string @operator, LambdaExpression expression) =>
                $"({this.VisitNode(binary.Left, expression)} {@operator} {this.VisitNode(binary.Right, expression)})";
    }

    internal static partial class ExpressionTree
    {
        internal static void Sql()
        {
            InfixVisitor infixVisitor = new InfixVisitor();
            Expression<Func<double, double, double>> expression1 = (a, b) => a * a + b * b;
            string infixExpression1 = infixVisitor.VisitBody(expression1);
            infixExpression1.WriteLine(); // SELECT ((@a * @a) + (@b * @b));

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;
            string infixExpression2 = infixVisitor.VisitBody(expression2);
            infixExpression2.WriteLine(); // SELECT (((@a + @b) - ((@c * @d) / 2)) + (@e * 3));
        }
    }

    public static partial class BinaryArithmeticTranslator
    {
        internal static double ExecuteScalar(
            string connection,
            string command,
            IDictionary<string, double> parameters)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            using (SqlCommand sqlCommand = new SqlCommand(command, sqlConnection))
            {
                sqlConnection.Open();
                parameters.ForEach(parameter => sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value));
                return (double)sqlCommand.ExecuteScalar();
            }
        }
    }

    public static partial class BinaryArithmeticTranslator
    {
        private static readonly InfixVisitor InfixVisitor = new InfixVisitor();

#if !__IOS__
        public static TDelegate Sql<TDelegate>(
            this Expression<TDelegate> expression, string connection) where TDelegate : class
        {
            DynamicMethod dynamicMethod = new DynamicMethod(
                string.Empty,
                expression.ReturnType,
                expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                typeof(BinaryArithmeticTranslator).Module);
            EmitIL(dynamicMethod.GetILGenerator(), InfixVisitor.VisitBody(expression), expression, connection);
            return (TDelegate)(object)dynamicMethod.CreateDelegate(typeof(TDelegate));
        }
#endif

        private static void EmitIL<TDelegate>(
            ILGenerator ilGenerator, string infixExpression, Expression<TDelegate> expression, string connection)
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
                    nameof(ExecuteScalar),
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
            local1(1, 2).WriteLine(); // 5
#if !__IOS__
            Func<double, double, double> remote1 = expression1.Sql(ConnectionStrings.AdventureWorks);
            remote1(1, 2).WriteLine(); // 5
#endif

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;
            Func<double, double, double, double, double, double> local2 = expression2.Compile();
            local2(1, 2, 3, 4, 5).WriteLine(); // 12
#if !__IOS__
            Func<double, double, double, double, double, double> remote2 = expression2.Sql(ConnectionStrings.AdventureWorks);
            remote2(1, 2, 3, 4, 5).WriteLine(); // 12
#endif
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

    public interface IOrderedQueryable : IQueryable, IEnumerable { }

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable { }

    public interface IOrderedQueryable<out T> : IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable { }
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

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source);

        // Other members.
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector);

        public static IQueryable<TSource> Concat<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2);

        public static IQueryable<TResult> Cast<TResult>(this IQueryable source);

        // Other members.
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

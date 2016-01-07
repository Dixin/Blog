namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    using Dixin.Common;
    using Dixin.Linq.Fundamentals;
    using Dixin.Properties;

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

    public static partial class AnonymousFunction
    {
        public static void String()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            InfixVisitor infixVisitor = new InfixVisitor();
            string infixExpression = infixVisitor.VisitBody(infix); // "add(sub(add(a, b), div(mul(c, d), 2)), mul(e, 3))"
        }
    }

    public static partial class BinaryArithmeticTanslator
    {
        private static readonly InfixVisitor InfixVisitor = new InfixVisitor();

        public static TDelegate Translate<TDelegate>(Expression<TDelegate> expression, string connection = null)
            where TDelegate : class
        {
            if (connection == null)
            {
                connection = Settings.Default.AdventureWorksConnectionString;
                AppDomainData.SetDefaultDataDirectory();
            }

            DynamicMethod dynamicMethod = new DynamicMethod(
                string.Empty,
                expression.ReturnType,
                expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                typeof(BinaryArithmeticTanslator).Module);
            EmitIL(dynamicMethod.GetILGenerator(), InfixVisitor.VisitBody(expression), expression, connection);
            return dynamicMethod.CreateDelegate(typeof(TDelegate)) as TDelegate;
        }

        private static void EmitIL<TDelegate>(ILGenerator ilGenerator, string infixExpression, Expression<TDelegate> expression, string connection)
        {
            // Dictionary<string, double> dictionary = new Dictionary<string, double>();
            ilGenerator.DeclareLocal(typeof(Dictionary<string, double>));
            ilGenerator.Emit(
                OpCodes.Newobj,
                typeof(Dictionary<string, double>).GetConstructor(new Type[0]));
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
                        $"{nameof(Dictionary<string, double>.Add)}",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod));
            }

            // BinaryArithmeticTanslator.ExecuteQuery(connection, sql, dictionary);
            ilGenerator.Emit(OpCodes.Ldstr, connection);
            ilGenerator.Emit(OpCodes.Ldstr, $"SELECT {infixExpression}");
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(
                OpCodes.Call,
                typeof(BinaryArithmeticTanslator).GetMethod(
                    nameof(ExecuteQuery),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod));

            // Returns the result.
            ilGenerator.Emit(OpCodes.Ret);
        }

        internal static double ExecuteQuery(
            string connection,
            string sql,
            IEnumerable<KeyValuePair<string, double>> parameters)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            using (SqlCommand command = new SqlCommand(sql, sqlConnection))
            {
                sqlConnection.Open();
                foreach (KeyValuePair<string, double> parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                return (double)command.ExecuteScalar();
            }
        }
    }

    public static partial class BinaryArithmeticTanslator
    {
        static BinaryArithmeticTanslator()
        {
            AppDomainData.SetDefaultDataDirectory();
        }
    }
}

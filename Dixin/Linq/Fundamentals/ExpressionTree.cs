namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection.Emit;

    public abstract class BinaryArithmeticExpressionVisitor<TResult>
    {
        public TResult VisitBody(LambdaExpression expression) => this.VisitNode(expression.Body, expression);

        protected TResult VisitNode(Expression node, LambdaExpression expression)
        {
            // Processes the 6 types of node.
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    return this.VisitAdd(node as BinaryExpression, expression);

                case ExpressionType.Constant:
                    return this.VisitConstant(node as ConstantExpression, expression);

                case ExpressionType.Divide:
                    return this.VisitDivide(node as BinaryExpression, expression);

                case ExpressionType.Multiply:
                    return this.VisitMultiply(node as BinaryExpression, expression);

                case ExpressionType.Parameter:
                    return this.VisitParameter(node as ParameterExpression, expression);

                case ExpressionType.Subtract:
                    return this.VisitSubtract(node as BinaryExpression, expression);

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }

        protected abstract TResult VisitAdd(BinaryExpression add, LambdaExpression expression);

        protected abstract TResult VisitConstant(ConstantExpression constant, LambdaExpression expression);

        protected abstract TResult VisitDivide(BinaryExpression divide, LambdaExpression expression);

        protected abstract TResult VisitMultiply(BinaryExpression multiply, LambdaExpression expression);

        protected abstract TResult VisitParameter(ParameterExpression parameter, LambdaExpression expression);

        protected abstract TResult VisitSubtract(BinaryExpression subtract, LambdaExpression expression);
    }

    public class PrefixVisitor : BinaryArithmeticExpressionVisitor<string>
    {
        protected override string VisitAdd
            (BinaryExpression add, LambdaExpression expression) => this.VisitBinary(add, "add", expression);

        protected override string VisitConstant
            (ConstantExpression constant, LambdaExpression expression) => constant.Value.ToString();

        protected override string VisitDivide
            (BinaryExpression divide, LambdaExpression expression) => this.VisitBinary(divide, "div", expression);

        protected override string VisitMultiply
            (BinaryExpression multiply, LambdaExpression expression) =>
                this.VisitBinary(multiply, "mul", expression);

        protected override string VisitParameter
            (ParameterExpression parameter, LambdaExpression expression) => parameter.Name;

        protected override string VisitSubtract
            (BinaryExpression subtract, LambdaExpression expression) =>
                this.VisitBinary(subtract, "sub", expression);

        private string VisitBinary // Recursive: operator(left, right)
            (BinaryExpression binary, string @operator, LambdaExpression expression) =>
                $"{@operator}({this.VisitNode(binary.Left, expression)}, {this.VisitNode(binary.Right, expression)})";
    }

    public static partial class AnonymousFunction
    {
        public static void String()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            PrefixVisitor prefixVisitor = new PrefixVisitor();
            string prefix = prefixVisitor.VisitBody(infix); // "add(sub(add(a, b), div(mul(c, d), 2)), mul(e, 3))"
        }
    }

    public class PostfixVisitor : BinaryArithmeticExpressionVisitor<IEnumerable<Tuple<OpCode, double?>>>
    {
        protected override IEnumerable<Tuple<OpCode, double?>> VisitAdd
            (BinaryExpression add, LambdaExpression expression) => this.VisitBinary(add, OpCodes.Add, expression);

        protected override IEnumerable<Tuple<OpCode, double?>> VisitConstant(
            ConstantExpression constant, LambdaExpression expression)
        {
            yield return Tuple.Create(OpCodes.Ldc_R8, (double?)constant.Value);
        }

        protected override IEnumerable<Tuple<OpCode, double?>> VisitDivide
            (BinaryExpression divide, LambdaExpression expression) =>
                this.VisitBinary(divide, OpCodes.Div, expression);

        protected override IEnumerable<Tuple<OpCode, double?>> VisitMultiply
            (BinaryExpression multiply, LambdaExpression expression) =>
                this.VisitBinary(multiply, OpCodes.Mul, expression);

        protected override IEnumerable<Tuple<OpCode, double?>> VisitParameter(
            ParameterExpression parameter, LambdaExpression expression)
        {
            int index = expression.Parameters.IndexOf(parameter);
            yield return Tuple.Create(OpCodes.Ldarg_S, (double?)index);
        }

        protected override IEnumerable<Tuple<OpCode, double?>> VisitSubtract
            (BinaryExpression subtract, LambdaExpression expression) =>
                this.VisitBinary(subtract, OpCodes.Sub, expression);

        private IEnumerable<Tuple<OpCode, double?>> VisitBinary // Recursive: left, right, operator
            (BinaryExpression binary, OpCode postfix, LambdaExpression expression) =>
                this.VisitNode(binary.Left, expression)
                    .Concat(this.VisitNode(binary.Right, expression))
                    .Concat(EnumerableEx.Return(Tuple.Create(postfix, (double?)null))); // left, right, postfix
    }

    public static partial class AnonymousFunction
    {
        public static void IL()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            PostfixVisitor postfixVisitor = new PostfixVisitor();
            IEnumerable<Tuple<OpCode, double?>> postfix = postfixVisitor.VisitBody(infix);
            foreach (Tuple<OpCode, double?> code in postfix)
            {
                Trace.WriteLine($"{code.Item1} {code.Item2}");
            }
        }
    }

    public static class BinaryArithmeticCompiler
    {
        private static readonly PostfixVisitor PostfixVisitor = new PostfixVisitor();

        public static TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
            where TDelegate : class
        {
            DynamicMethod dynamicMethod = new DynamicMethod(
                string.Empty,
                expression.ReturnType,
                expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                typeof(BinaryArithmeticCompiler).Module);
            EmitIL(dynamicMethod.GetILGenerator(), PostfixVisitor.VisitBody(expression));
            return dynamicMethod.CreateDelegate(typeof(TDelegate)) as TDelegate;
        }

        private static void EmitIL(ILGenerator ilGenerator, IEnumerable<Tuple<OpCode, double?>> codes)
        {
            foreach (Tuple<OpCode, double?> code in codes)
            {
                if (code.Item2.HasValue)
                {
                    if (code.Item1 == OpCodes.Ldarg_S)
                    {
                        ilGenerator.Emit(code.Item1, (int)code.Item2.Value); // ldarg.s (int)index
                    }
                    else
                    {
                        ilGenerator.Emit(code.Item1, code.Item2.Value); // ldc.r8 (double)constant
                    }
                }
                else
                {
                    ilGenerator.Emit(code.Item1); // add, sub, mul, div
                }
            }

            ilGenerator.Emit(OpCodes.Ret); // Returns the result.
        }
    }

    public static partial class AnonymousFunction
    {
        public static void Method()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            Func<double, double, double, double, double, double> method = BinaryArithmeticCompiler.Compile(infix);
            double result = method(1, 2, 3, 4, 5); // 12
        }

        public static void BuiltInCompile()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2 + e * 3;

            Func<double, double, double, double, double, double> method = infix.Compile();
            double result = method(1, 2, 3, 4, 5); // 12
        }

        public static void TypeInference()
        {
            // Anonymous method with a int parameter, and returns a bool value.
            Func<int, bool> predicate1 = number => number > 0;

            // Expression tree with a int parameter, and returns a bool value.
            Expression<Func<int, bool>> predicate2 = number => number > 0;

#if ERROR
            var predicate3 = number => number > 0;
            dynamic predicate4 = number => number > 0;
#endif
        }
    }
}

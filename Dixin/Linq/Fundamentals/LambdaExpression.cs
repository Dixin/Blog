namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static partial class Anonymous
    {
        public static bool IsPositive(int number)
        {
            return number > 0;
        }

        public static void Delegate()
        {
            Func<int, bool> predicate1 = new Func<int, bool>(IsPositive);
            Func<int, bool> predicate2 = IsPositive;
        }

        public static void AnonymousMethod()
        {
            Func<int, bool> isPositive = delegate (int number)
                {
                    return number > 0;
                };

            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
                {
                    Trace.WriteLine(e.ExceptionObject);
                };
        }

        public static void Lambda()
        {
            Func<int, bool> isPositive = (int number) =>
                {
                    return number > 0;
                };

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
                {
                    Trace.WriteLine(e.ExceptionObject);
                };
        }

        public static void ExpressionLambda()
        {
            Func<int, bool> isPositive = number => number > 0;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Trace.WriteLine(e.ExceptionObject);
        }

        public static void StatementLambda()
        {
            Func<int, bool> predicate = number =>
                {
                    Trace.WriteLine(number);
                    return number > 0;
                };
        }

        public static void CallAnonymousMethod()
        {
            bool isPositive = new Func<int, bool>(delegate (int number) { return number > 0; })(1);

            new Action<bool>(delegate (bool value) { Trace.WriteLine(value); })(isPositive);
        }

        public static void CallLambda()
        {
            bool isPositive = new Func<int, bool>(number => number > 0)(1);

            new Action<bool>(value => Trace.WriteLine(value))(isPositive);
        }
    }

    public static class CompiledAnonymous
    {
        [CompilerGenerated]
        private static Func<int, bool> cachedAnonymousMethodDelegate0;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler cachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static bool AnonymousMethod0(int number)
        {
            return number > 0;
        }

        [CompilerGenerated]
        private static void AnonymousMethod1(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
        }

        public static void AnonymousMethod()
        {
            Func<int, bool> isPositive = cachedAnonymousMethodDelegate0
                ?? (cachedAnonymousMethodDelegate0 = AnonymousMethod0);
            AppDomain.CurrentDomain.UnhandledException += cachedAnonymousMethodDelegate1
                ?? (cachedAnonymousMethodDelegate1 = AnonymousMethod1);
        }
    }

    public static partial class AnonymousFunction
    {
        public static void ExpressionTree()
        {
            Expression<Func<int, bool>> predicate = number => number > 0;
        }

#if ERROR
        public static void ExpressionTree()
        {
            Expression<Func<int, bool>> statementLambda2 = number =>
                {
                    Trace.WriteLine(number);
                    return number > 0;
                };
    }
#endif

        public static void CompiledExpressionTree()
        {
            ParameterExpression number = Expression.Parameter(typeof(int), "number"); // number
            ConstantExpression _0 = Expression.Constant(0, typeof(int)); // 0
            BinaryExpression numberGreaterThan1 = Expression.GreaterThan(number, _0); // number > 0

            Expression<Func<int, bool>> predicate = Expression.Lambda<Func<int, bool>>(
                numberGreaterThan1, // => number > 0
                number); // number =>
        }

#if ERROR
        public static void Statement()
        {
            Expression<Func<int, bool>> statementLambda1 = number => { return number > 0; };

            Expression<Func<int, bool>> statementLambda2 = number =>
                    {
                        Trace.WriteLine(number);
                        return number > 0;
                    };
        }
#endif

        public static void Statement()
        {
            {
                // Single statement. Syntactic sugar can be used.
                Expression<Func<int, bool>> statementLambda1 = number => number > 0;
            }
            {
                // Two statements. Syntactic sugar is not available.
                // Parameter "number".
                ParameterExpression number = Expression.Parameter(typeof(int), nameof(number));
                Expression<Func<int, bool>> statementLambda2 = Expression.Lambda<Func<int, bool>>(
                    // {
                    Expression.Block(
                        Expression.Call(
                            // Trace.WriteLine(
                            typeof(Trace).GetMethod(nameof(Trace.WriteLine), new Type[] { typeof(object) }),
                            // number// );
                            Expression.TypeAs(number, typeof(object))), // Boxing must be handled manually.
                                                                        // return number > 0;
                        Expression.GreaterThan(number, Expression.Constant(0, typeof(int)))),
                    // }
                    number);
            }
        }
    }
}

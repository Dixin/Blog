namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;

    internal static partial class Functions
    {
        internal static void FirstOrderHigherOrder()
        {
            Func<int, int, int> add2 = (a, b) => a + b;
            int add2Result = add2(1, 2);
            Func<int, Func<int, int>> curriedAdd2 = a => new Func<int, int>(b => a + b);
            Func<int, int> add1 = curriedAdd2(1); // Equivalent to: b => 1 + b.
            int curriedAdd2Result = add1(2);
        }

        internal static void CurryFunc3()
        {
            Func<int, int, int> add2 = (a, b) => a + b;
            int add2Result = add2(1, 2);
            Func<int, Func<int, int>> curriedAdd2 = a => b => a + b;
            int curriedAdd2Result = curriedAdd2(1)(2);
        }

        internal static void CallCurriedAnonymous()
        {
            int add2Result = new Func<int, int, int>((a, b) => a + b)(1, 2);
            int curriedAdd2Result = new Func<int, Func<int, int>>(a => b => a + b)(1)(2);
        }

        internal static void CurryFunc<T1, T2, T3, TN, TResult>()
        {
            Func<T1, T2, T3, /* T4, ... */ TN, TResult> function =
                (value1, value2, value3, /* ... */ valueN) => default(TResult);
            Func<T1, Func<T2, Func<T3, /* Func<T4, ... */ Func<TN, TResult> /* ... */>>> curriedFunction =
                value1 => value2 => value3 => /* value4 => ... */ valueN => default(TResult);
        }

        internal static void CurryFunc()
        {
            Func<int, int, int> add2 = (a, b) => a + b;
            int add2Result = add2(1, 2);
            Func<int, Func<int, int>> curriedAdd2 = add2.Curry();
            int curriedAdd2Result = curriedAdd2(1)(2);

            Func<int, int, int, int> add3 = (a, b, c) => a + b + c;
            int add3Result = add3(1, 2, 3);
            Func<int, Func<int, Func<int, int>>> curriedAdd3 = add3.Curry();
            int curriedAdd3Result = curriedAdd3(1)(2)(3);
        }

        internal static void CurryAction()
        {
            Action<int, int> traceAdd2 = (a, b) => Trace.WriteLine(a + b);
            traceAdd2(1, 2);
            Func<int, Action<int>> curriedTraceAdd2 = a => b => Trace.WriteLine(a + b);
            curriedTraceAdd2(1)(2);

            Action<int, int, int> traceAdd3 = (a, b, c) => Trace.WriteLine(a + b + c);
            traceAdd3(1, 2, 3);
            Func<int, Func<int, Action<int>>> curriedTraceAdd3 = a => b => c => Trace.WriteLine(a + b + c);
            curriedTraceAdd3(1)(2)(3);
        }

        internal static void CurryAction<T1, T2, T3, TN>()
        {
            Action<T1, T2, T3, /* T4, ... */ TN> function =
                (value1, value2, value3, /* ... */ valueN) => { };
            Func<T1, Func<T2, Func<T3, /* Func<T4, ... */ Action<TN> /* ... */>>> curriedFunction =
                value1 => value2 => value3 => /* value4 => ... */ valueN => { };
        }

        internal static void OperatorAssociativity()
        {
            Func<int, Func<int, int>> curriedAdd2 = a => (b => a + b);
            curriedAdd2 = a => b => a + b;
            Func<int, Func<int, Func<int, int>>> curriedAdd3 = a => (b => (c => a + b + c));
            curriedAdd3 = a => b => c => a + b + c;
        }

        internal static void OperatorAssociativity<T1, T2, T3, TN, TResult>()
        {
            Func<T1, Func<T2, Func<T3, /* Func<T4, ... */ Func<TN, TResult> /* ... */>>> curriedFunction =
                value1 => (value2 => (value3 => /* (value4 => ... */ valueN => default(TResult) /* )... */));
            curriedFunction = value1 => value2 => value3 => /* value4 => ... */ valueN => default(TResult);
        }

        internal static void PartialApplication()
        {
            Func<int, int, int> add2 = (a, b) => a + b;
            Func<int, int> add1 = add2.Partial(1);
            int add2Result = add1(2);

            Action<int, int> traceAdd2 = (a, b) => Trace.WriteLine(a + b);
            Action<int> traceAdd1 = traceAdd2.Partial(1);
            traceAdd1(2);
        }

        internal static void Uncurry()
        {
            Func<int, Func<int, Func<int, int>>> curriedAdd3 = a => (b => (c => a + b + c));
            Func<int, int, int, int> add3 = curriedAdd3.Uncurry();
            int add3Result = add3(1, 2, 3);

            Func<int, Func<int, Action<int>>> curriedTraceAdd3 = a => b => c => Trace.WriteLine(a + b + c);
            Action<int, int, int> traceAdd3 = curriedTraceAdd3.Uncurry();
            traceAdd3(1, 2, 3);
        }
    }
}

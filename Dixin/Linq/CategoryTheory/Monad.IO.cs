namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;

    using Dixin.Common;

    using Microsoft.FSharp.Core;

    public delegate T IO<out T>();

    // [Pure]
    public static partial class IOExtensions
    {
        // Required by LINQ.
        public static IO<TResult> SelectMany<TSource, TSelector, TResult>
            (this IO<TSource> source,
             Func<TSource, IO<TSelector>> selector,
             Func<TSource, TSelector, TResult> resultSelector) =>
                () =>
                    {
                        TSource sourceValue = source();
                        return resultSelector(sourceValue, selector(sourceValue)());
                    };

        // Not required, just for convenience.
        public static IO<TResult> SelectMany<TSource, TResult>
            (this IO<TSource> source, Func<TSource, IO<TResult>> selector) =>
                source.SelectMany(selector, Functions.False);
    }

    // [Pure]
    public static partial class IOExtensions
    {
        // η: T -> IO<T>
        public static IO<T> IO<T>
            (this T value) => () => value;

        // Select: (TSource -> TResult) -> (IO<TSource> -> IO<TResult>)
        public static IO<TResult> Select<TSource, TResult>
            (this IO<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).IO());
    }

    [Pure]
    public static partial class IO
    {
        public static IO<Unit> Action
            (Action action) => action.AsIO();

        public static Func<T, IO<Unit>> Action<T>
            (this Action<T> action) => action.AsIO();

        public static Func<T1, T2, IO<Unit>> Action<T1, T2>
            (this Action<T1, T2> action) => action.AsIO();

        public static Func<T1, T2, T3, IO<Unit>> Action<T1, T2, T3>
            (this Action<T1, T2, T3> action) => action.AsIO();

        public static Func<T1, T2, T3, T4, IO<Unit>> Action<T1, T2, T3, T4>
            (this Action<T1, T2, T3, T4> action) => action.AsIO();

        // ...

        public static IO<T> Func<T>
            (this Func<T> function) => function.AsIO();

        public static Func<T, IO<TResult>> Func<T, TResult>
            (this Func<T, TResult> function) => function.AsIO();

        public static Func<T1, T2, IO<TResult>> Func<T1, T2, TResult>
            (this Func<T1, T2, TResult> function) => function.AsIO();

        public static Func<T1, T2, T3, IO<TResult>> Func<T1, T2, T3, TResult>
            (this Func<T1, T2, T3, TResult> function) => function.AsIO();

        public static Func<T1, T2, T3, T4, IO<TResult>> Func<T1, T2, T3, T4, TResult>
            (this Func<T1, T2, T3, T4, TResult> function) => function.AsIO();

        // ...
    }

    [Pure]
    public static partial class IOExtensions
    {
        public static IO<Unit> AsIO
            (this Action action) =>
                () =>
                    {
                        action();
                        return null;
                    };

        public static Func<T, IO<Unit>> AsIO<T>
            (this Action<T> action) => arg =>
                () =>
                    {
                        action(arg);
                        return null;
                    };

        public static Func<T1, T2, IO<Unit>> AsIO<T1, T2>
            (this Action<T1, T2> action) => (arg1, arg2) =>
                () =>
                    {
                        action(arg1, arg2);
                        return null;
                    };

        public static Func<T1, T2, T3, IO<Unit>> AsIO<T1, T2, T3>
            (this Action<T1, T2, T3> action) => (arg1, arg2, arg3) =>
                () =>
                    {
                        action(arg1, arg2, arg3);
                        return null;
                    };

        public static Func<T1, T2, T3, T4, IO<Unit>> AsIO<T1, T2, T3, T4>
            (this Action<T1, T2, T3, T4> action) => (arg1, arg2, arg3, arg4) =>
                () =>
                    {
                        action(arg1, arg2, arg3, arg4);
                        return null;
                    };

        // ...

        public static IO<TResult> AsIO<TResult>
            (this Func<TResult> function) =>
                () => function();

        public static Func<T, IO<TResult>> AsIO<T, TResult>
            (this Func<T, TResult> function) => arg =>
                () => function(arg);

        public static Func<T1, T2, IO<TResult>> AsIO<T1, T2, TResult>
            (this Func<T1, T2, TResult> function) => (arg1, arg2) =>
                () => function(arg1, arg2);

        public static Func<T1, T2, T3, IO<TResult>> AsIO<T1, T2, T3, TResult>
            (this Func<T1, T2, T3, TResult> function) => (arg1, arg2, arg3) =>
                () => function(arg1, arg2, arg3);

        public static Func<T1, T2, T3, T4, IO<TResult>> AsIO<T1, T2, T3, T4, TResult>
            (this Func<T1, T2, T3, T4, TResult> function) => (arg1, arg2, arg3, arg4) =>
                () => function(arg1, arg2, arg3, arg4);

        // ...
    }

    // Impure.
    public static partial class IOQuery
    {
        public static void AsIO()
        {
            IO<string> consoleReadLine = new Func<string>(Console.ReadLine).AsIO();
            Func<string, IO<Unit>> consoleWriteLine = new Action<string>(Console.WriteLine).AsIO();

            Func<string, IO<string>> fileReadAllText = new Func<string, string>(File.ReadAllText).AsIO();
            Func<string, string, IO<Unit>> fileWriteAllText = new Action<string, string>(File.WriteAllText).AsIO();

            Func<string, IO<bool>> fileExists = new Func<string, bool>(File.Exists).AsIO();
            // ...
        }

        public static void IOFuncAction()
        {
            IO<string> consoleReadLine = IO.Func(Console.ReadLine);
            Func<string, IO<Unit>> consoleWriteLine = IO.Action<string>(Console.WriteLine);

            Func<string, IO<string>> fileReadAllText = IO.Func<string, string>(File.ReadAllText);
            Func<string, string, IO<Unit>> fileWriteAllText = IO.Action<string, string>(File.WriteAllText);

            Func<string, IO<bool>> fileExists = IO.Func<string, bool>(File.Exists);
            // ...
        }

        public static void FileIO()
        {
            IO<Tuple<bool, string>> query =
                // 1. Read file name from console.
                from fileName in IO.Func(Console.ReadLine)
                // 2. Write confirmation message to console.
                let message = $"{fileName}? y/n"
                from _ in IO.Action<string>(Console.WriteLine)(message)
                // 3. Read confirmation from console.
                from confirmation in IO.Func(Console.ReadLine)
                // 4. If confirmed, read the file.
                let isConfirmed = "y".EqualsIgnoreCase(confirmation)
                from text in isConfirmed ? IO.Func<string, string>(File.ReadAllText)(fileName) : string.Empty.IO()
                // 5. Write text to console.
                from __ in IO.Action<string>(Console.WriteLine)(text)
                // 6. Returns text as query result.
                select new Tuple<bool, string>(isConfirmed, text); // Deferred and lazy.
            Tuple<bool, string> result = query(); // Execution.
        }

        public static void NetworkIO()
        {
            IO<Unit> query =
                // 1. Read URL from console.
                from url in IO.Func(Console.ReadLine)
                    // 2. Download string from Internet.
                from text in IO.Func(() => new WebClient().DownloadString(url))
                    // 3. Write string to console.
                let length = 1000
                let message = text.Length <= length ? text : $"{text.Substring(0, length)}..."
                from unit in IO.Action<string>(Console.WriteLine)(message)
                select (Unit)null; // Deferred and lazy.
            query(); // Execution.
        }
    }
}

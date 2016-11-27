namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.IO;
    using System.Net;

    using Dixin.Common;

    using Microsoft.FSharp.Core;

    using static Dixin.Linq.CategoryTheory.Functions;

    public delegate T IO<out T>();

    public static partial class IOExtensions
    {
        public static IO<TResult> SelectMany<TSource, TSelector, TResult>(
            this IO<TSource> source,
            Func<TSource, IO<TSelector>> selector,
            Func<TSource, TSelector, TResult> resultSelector) =>
                () =>
                {
                    TSource value = source();
                    return resultSelector(value, selector(value)());
                };

        // Wrap: TSource -> IO<TSource>
        public static IO<TSource> IO<TSource>(this TSource value) => () => value;

        public static IO<TResult> Select<TSource, TResult>(
            this IO<TSource> source,
            Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).IO(), False);
    }

    public static partial class IOExtensions
    {
        public static IO<Unit> IOAction(this Action action) =>
            () =>
            {
                action();
                return default(Unit);
            };

        public static Func<T, IO<Unit>> IOAction<T>(this Action<T> action) => 
            value => () =>
            {
                action(value);
                return default(Unit);
            };

        public static Func<T1, T2, IO<Unit>> IOAction<T1, T2>(this Action<T1, T2> action) =>
            (value1, value2) => () =>
            {
                action(value1, value2);
                return default(Unit);
            };

        public static Func<T1, T2, T3, IO<Unit>> IOAction<T1, T2, T3>(this Action<T1, T2, T3> action) => 
            (value1, value2, value3) => () =>
            {
                action(value1, value2, value3);
                return default(Unit);
            };

        public static Func<T1, T2, T3, T4, IO<Unit>> IOAction<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action) => 
            (value1, value2, value3, value4) => () =>
            {
                action(value1, value2, value3, value4);
                return null;
            };

        // ...

        public static IO<TResult> IOFunc<TResult>(this Func<TResult> function) =>
            () => function();

        public static Func<T, IO<TResult>> IOFunc<T, TResult>(this Func<T, TResult> function) => 
            value => () => function(value);

        public static Func<T1, T2, IO<TResult>> IOFunc<T1, T2, TResult>(this Func<T1, T2, TResult> function) => 
            (value1, value2) => () => function(value1, value2);

        public static Func<T1, T2, T3, IO<TResult>> IOFunc<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> function) => 
                (value1, value2, value3) => () => function(value1, value2, value3);

        public static Func<T1, T2, T3, T4, IO<TResult>> IOFunc<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function) => 
                (value1, value2, value3, value4) => () => function(value1, value2, value3, value4);

        // ...
    }

    // Impure.
    public static partial class IOExtensions
    {
        internal static void IOFunctions()
        {
            IO<string> consoleReadLine = IOFunc(Console.ReadLine);
            Func<string, IO<Unit>> consoleWriteLine = IOAction<string>(Console.WriteLine);
            Func<string, IO<string>> fileReadAllText = IOFunc<string, string>(File.ReadAllText);
            Func<string, string, IO<Unit>> fileWriteAllText = IOAction<string, string>(File.WriteAllText);
            Func<string, IO<bool>> fileExists = IOFunc<string, bool>(File.Exists);
        }

        internal static void FileIO()
        {
            IO<Tuple<bool, string>> query =
                // 1. Read file name from console.
                from fileName in IOFunc(Console.ReadLine)
                // 2. Write confirmation message to console.
                let message = $"{fileName}? y/n"
                from _ in IOAction<string>(Console.WriteLine)(message)
                // 3. Read confirmation from console.
                from confirmation in IOFunc(Console.ReadLine)
                // 4. If confirmed, read the file.
                let isConfirmed = "y".EqualsIgnoreCase(confirmation)
                from text in isConfirmed ? IOFunc<string, string>(File.ReadAllText)(fileName) : string.Empty.IO()
                // 5. Write text to console.
                from __ in IOAction<string>(Console.WriteLine)(text)
                // 6. Returns text as query result.
                select isConfirmed.Tuple(text); // Define query.
            Tuple<bool, string> result = query(); // Execute query.
        }

        internal static void NetworkIO()
        {
            using (WebClient webClient = new WebClient())
            {
                IO<Unit> query =
                    // 1. Read URL from console.
                    from url in IOFunc(Console.ReadLine)
                    // 2. Download string from Internet.
                    from text in IOFunc(() => webClient.DownloadString(url))
                    // 3. Write string to console.
                    let length = 1000
                    let message = text.Length <= length ? text : $"{text.Substring(0, length)}..."
                    from _ in IOAction<string>(Console.WriteLine)(message)
                    select _; // Define query.
                query(); // Execute query.
            }
        }
    }
}

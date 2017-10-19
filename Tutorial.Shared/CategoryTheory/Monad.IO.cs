namespace Tutorial.CategoryTheory
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    // IO: () -> T
    public delegate T IO<out T>();

    public static partial class IOExtensions
    {
        internal static string Impure()
        {
            string filePath = Console.ReadLine();
            string fileContent = File.ReadAllText(filePath);
            return fileContent;
        }

        internal static IO<string> Pure()
        {
            IO<string> filePath = () => Console.ReadLine();
            IO<string> fileContent = () => File.ReadAllText(filePath());
            return fileContent;
        }

        internal static void IO()
        {
            string result1 = Impure(); // IO is produced.
            IO<string> resultWrapper = Pure(); // IO is not produced.

            string result2 = resultWrapper(); // IO is produced.
        }
    }

    public static partial class IOExtensions
    {
        // SelectMany: (IO<TSource>, TSource -> IO<TSelector>, (TSource, TSelector) -> TResult) -> IO<TResult>
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

        // Select: (IO<TSource>, TSource -> TResult) -> IO<TResult>
        public static IO<TResult> Select<TSource, TResult>(
            this IO<TSource> source, Func<TSource, TResult> selector) =>
                source.SelectMany(value => selector(value).IO(), (value, result) => result);
    }

    public static partial class IOExtensions
    {
        public static IO<TResult> IO<TResult>(Func<TResult> function) =>
            () => function();

        public static IO<Unit> IO(Action action) =>
            () =>
            {
                action();
                return default;
            };
    }

    public static partial class IOExtensions
    {
        internal static void Workflow()
        {
            IO<int> query = from unit1 in IO(() => Console.WriteLine("File path:")) // IO<Unit>.
                            from filePath in IO(Console.ReadLine) // IO<string>.
                            from unit2 in IO(() => Console.WriteLine("File encoding:")) // IO<Unit>.
                            from encodingName in IO(Console.ReadLine) // IO<string>.
                            let encoding = Encoding.GetEncoding(encodingName)
                            from fileContent in IO(() => File.ReadAllText(filePath, encoding)) // IO<string>.
                            from unit3 in IO(() => Console.WriteLine("File content:")) // IO<Unit>.
                            from unit4 in IO(() => Console.WriteLine(fileContent)) // IO<Unit>.
                            select fileContent.Length; // Define query.
            int result = query(); // Execute query.
        }

        internal static async Task WorkflowAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                IO<Task> query = from unit1 in IO(() => Console.WriteLine("URI:")) // IO<Unit>. 
                                 from uri in IO(Console.ReadLine) // IO<string>.
                                 from unit2 in IO(() => Console.WriteLine("File path:")) // IO<Unit>.
                                 from filePath in IO(Console.ReadLine) // IO<string>.
                                 from downloadStreamTask in IO(async () =>
                                     await httpClient.GetStreamAsync(uri)) // IO<Task<Stream>>.
                                 from writeFileTask in IO(async () => 
                                     await (await downloadStreamTask).CopyToAsync(File.Create(filePath))) // IO<Task>.
                                 from messageTask in IO(async () =>
                                     {
                                         await writeFileTask;
                                         Console.WriteLine($"Downloaded {uri} to {filePath}");
                                     }) // IO<Task>.
                                 select messageTask; // Define query.
                await query(); // Execute query.
            }
        }
    }
}

namespace Tutorial.ParallelLinq
{
#if NETFX
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    using static Functions;

    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;
    using Stopwatch = System.Diagnostics.Stopwatch;
#else
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;

    using static Functions;

    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;
    using Stopwatch = System.Diagnostics.Stopwatch;
#endif

    internal static partial class Performance
    {
        private static void OrderByTest(Func<int, int> keySelector, int count, int run)
        {
            int[] source = EnumerableX.RandomInt32(count: count).ToArray();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Enumerable.Range(0, run).ForEach(_ =>
            {
                int[] sequential = source.OrderBy(keySelector).ToArray();
            });
            stopwatch.Stop();
            $"Sequential:{stopwatch.ElapsedMilliseconds}".WriteLine();

            stopwatch.Restart();
            Enumerable.Range(0, run).ForEach(_ =>
            {
                int[] parallel1 = source.AsParallel().OrderBy(keySelector).ToArray();
            });
            stopwatch.Stop();
            $"Parallel:{stopwatch.ElapsedMilliseconds}".WriteLine();
        }
    }

    internal static partial class Performance
    {
        internal static void OrderByTestForCount()
        {
            OrderByTest(keySelector: value => value, count: 5, run: 10_000);    
            // Sequential:11    Parallel:1422
            OrderByTest(keySelector: value => value, count: 5_000, run: 100);
            // Sequential:114   Parallel:107
            OrderByTest(keySelector: value => value, count: 500_000, run: 100);
            // Sequential:18210 Parallel:8204
        }

        internal static void OrderByTestForKeySelector()
        {
            OrderByTest(
                keySelector: value => value + ComputingWorkload(iteration: 1), 
                count: Environment.ProcessorCount, run: 100_000);
            // Sequential:37   Parallel:2218
            OrderByTest(
                keySelector: value => value + ComputingWorkload(iteration: 10_000), 
                count: Environment.ProcessorCount, run: 1_000);
            // Sequential:115  Parallel:125
            OrderByTest(
                keySelector: value => value + ComputingWorkload(iteration: 100_000), 
                count: Environment.ProcessorCount, run: 100);
            // Sequential:1240 Parallel:555
        }
    }

    internal static partial class Performance
    {
        private static void DownloadTest(string[] uris)
        {
            uris.Visualize(uri => Download(uri)); // Sequential with no concurrency.

            uris.AsParallel()
                .WithDegreeOfParallelism(10) // Parallel with max concurrency.
                .Visualize(uri => Download(uri));

            using (Markers.EnterSpan(-3, nameof(ParallelEnumerableX.ForceParallel)))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(nameof(ParallelEnumerableX.ForceParallel));
                uris.ForceParallel(
                    uri =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, uri))
                        {
                            Download(uri);
                        }
                    },
                    forcedDegreeOfParallelism: 10); // Parallel with forced concurrency.
            }
        }

        internal static void RunDownloadSmallFilesTest()
        {
            string[] thumbnails = 
                XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "thumbnail")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(thumbnails);
        }

        internal static void RunDownloadLargeFilesTest()
        {
            string[] contents = 
                XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "content")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(contents);
        }

        internal static void ReadFiles()
        {
            string coreLibraryPath = typeof(object).Assembly.Location; // ANDROID returns "mscorlib.dll".
#if !ANDROID
            string coreLibraryDirectory = Path.GetDirectoryName(coreLibraryPath);
            string[] files = Directory.GetFiles(coreLibraryDirectory);

            files.Visualize(file => File.ReadAllBytes(file));
            files.AsParallel().Visualize(file => File.ReadAllBytes(file));
#endif
        }
    }
}

namespace Tutorial.ParallelLinq
{
#if NETFX
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Xml.Linq;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    using static Functions;
    using static Tutorial.LinqToObjects.EnumerableX;
    using static Tutorial.LinqToXml.Modeling;

    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;
    using Stopwatch = System.Diagnostics.Stopwatch;
#else
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Xml.Linq;

    using static Functions;
    using static Tutorial.LinqToObjects.EnumerableX;
    using static Tutorial.LinqToXml.Modeling;

    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;
    using Stopwatch = System.Diagnostics.Stopwatch;
#endif

    internal static partial class Performance
    {
        private static void OrderByTest(int count, int run, Func<int, int> keySelector)
        {
            int[] source = EnumerableX.RandomInt32().Take(count).ToArray();
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
        internal static void RunOrderByTest()
        {
            OrderByTest(5, 10_000, value => value);    // Sequential:11    Parallel:1422
            OrderByTest(5_000, 100, value => value);   // Sequential:114   Parallel:107
            OrderByTest(500_000, 100, value => value); // Sequential:18210 Parallel:8204

            OrderByTest(Environment.ProcessorCount, 10, value => value + Compute()); // Sequential:1605  Parallel:737
        }
    }

    internal static partial class Performance
    {
        private static void DownloadTest(string[] uris)
        {
            uris.Visualize(uri => Functions.Download(uri)); // Sequential with no concurrency.

            uris.AsParallel()
                .WithDegreeOfParallelism(10) // Parallel with max concurrency.
                .Visualize(uri => Functions.Download(uri));

            using (Markers.EnterSpan(-3, nameof(ParallelEnumerableX.ForceParallel)))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(nameof(ParallelEnumerableX.ForceParallel));
                uris.ForceParallel(
                    uri =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, uri))
                        {
                            Functions.Download(uri);
                        }
                    },
                    forcedDegreeOfParallelism: 10); // Parallel with forced concurrency.
            }
        }

        internal static void RunDownloadSmallFilesTest()
        {
            string[] thumbnails = 
                LoadXDocument("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "thumbnail")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(thumbnails);
        }

        internal static void RunDownloadLargeFilesTest()
        {
            string[] contents = 
                LoadXDocument("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "content")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(contents);
        }

        internal static void ReadFiles()
        {
            string coreLibraryPath = typeof(object).GetTypeInfo().Assembly.Location;
            string coreLibraryDirectory = Path.GetDirectoryName(coreLibraryPath);
            string[] files = Directory.GetFiles(coreLibraryDirectory);

            files.Visualize(file => File.ReadAllBytes(file));
            files.AsParallel().Visualize(file => File.ReadAllBytes(file));
        }
    }
}

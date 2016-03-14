namespace Dixin.Linq.Parallel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using static HelperMethods;

    internal static partial class Performance
    {
        internal static void Benchmark()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            Stopwatch stopwatch = Stopwatch.StartNew();
            source.ForEach(value => Computing(value));
            stopwatch.Stop();
            Trace.WriteLine($"Sequential LINQ: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            source.AsParallel().ForAll(value => Computing(value));
            stopwatch.Stop();
            Trace.WriteLine($"Parallel LINQ: {stopwatch.ElapsedMilliseconds}");
        }

        internal static void Visualize()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            source.Visualize(value => Computing(value));
            source.AsParallel().Visualize(value => Computing(value));
        }
    }

    internal static partial class Performance
    {
        private static void QueryArray(int length, int count)
        {
            int[][] arrays = RandomArrays(length, count);
            new int[0].OrderBy(value => value).ForEach(); // Warm up.
            new int[0].AsParallel().OrderBy(value => value).ForAll(_ => { }); // Warm up.
            arrays.Visualize( // array.OrderBy(value => value)
                array => array.Visualize(Enumerable.OrderBy, value => value),
                Visualizer.Sequential,
                0);
            arrays.Visualize( // array.AsParallel().OrderBy(value => value)
                array => array.AsParallel().Visualize(ParallelEnumerable.OrderBy, value => value),
                Visualizer.Parallel,
                1);
        }

        internal static void QuerySmallArray() => QueryArray(5, 20000);

        internal static void QueryMediumArray() => QueryArray(100, 1000);

        internal static void QueryLargeArray() => QueryArray(5000, 20);
    }

    internal static partial class Performance
    {
        private static void Download(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadData(url);
            }
        }

        internal static void DownloadSmallFiles()
        {
            string[] urls = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Root.Element("channel").Elements("item")
                .Select(item => item.Element(XNamespace.Get("http://search.yahoo.com/mrss/") + "thumbnail").Attribute("url").Value)
                .ToArray();

            urls.Visualize(Download);
            urls.AsParallel().Visualize(Download);
        }

        internal static void DownloadLargeFiles()
        {
            string[] urls = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Root.Element("channel").Elements("item")
                .Select(item => item.Element((XNamespace)"http://search.yahoo.com/mrss/" + "content").Attribute("url").Value)
                .ToArray();

            urls.Visualize(Download);
            urls.AsParallel().Visualize(Download);
        }

        internal static void ReadFiles()
        {
            string mscorlibPath = typeof(object).Assembly.Location;
            string gacPath = Path.GetDirectoryName(mscorlibPath);
            string[] files = Directory.GetFiles(gacPath);

            files.Visualize(file => File.ReadAllBytes(file));
            files.AsParallel().Visualize(file => File.ReadAllBytes(file));
        }
    }
}

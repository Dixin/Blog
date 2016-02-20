namespace Dixin.Linq.Parallel
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    internal static partial class Performance
    {
        private static void Computing(int value) => Enumerable.Repeat(value, 200000000).ForEach();

        internal static void Linq()
        {
            int[] source = Enumerable.Range(0, 16).ToArray();
            Stopwatch stopwatch = Stopwatch.StartNew();
            source.ForEach(value => Computing(value));
            stopwatch.Stop();
            Trace.WriteLine($"Sequential LINQ: {stopwatch.ElapsedMilliseconds}");

            stopwatch = Stopwatch.StartNew();
            source.AsParallel().ForAll(value => Computing(value));
            stopwatch.Stop();
            Trace.WriteLine($"Parallel LINQ: {stopwatch.ElapsedMilliseconds}");
        }

        internal static void VisualizeLinq()
        {
            int[] source = Enumerable.Range(0, 8).ToArray();
            Visualize.Sequential(source, Computing);
            Visualize.Parallel(source, Computing);
        }
    }

    public static class ArrayHelper
    {
        public static int[][] RandomArrays(int minValue, int maxValue, int length, int count)
            => Enumerable
                .Range(0, count)
                .Select(_ => EnumerableX.RandomInt32(minValue, maxValue).Take(length).ToArray())
                .ToArray();
    }

    internal static partial class Performance
    {
        internal static int[] SequentialComputing
            (int[] array, MarkerSeries markerSeries, int category) => array.OrderBy(value =>
                {
                    using (markerSeries.EnterSpan(nameof(Enumerable.OrderBy)))
                    {
                        return value;
                    }
                }).ToArray();

        internal static int[] ParallelComputing
            (int[] array, MarkerSeries markerSeries, int category) => array.AsParallel().OrderBy(value =>
                {
                    using (markerSeries.EnterSpan(nameof(Enumerable.OrderBy)))
                    {
                        return value;
                    }
                }).ToArray();

        internal static void QuerySmallArray()
        {
            int[][] arrays = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 5, 2000);

            Visualize.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            Visualize.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
        }

        internal static void QueryMediumArray()
        {
            int[][] arrays = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 2000, 50);
            Visualize.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            Visualize.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
        }

        internal static void QueryLargeArray()
        {
            int[][] arrays = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 50000, 2);
            Visualize.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            Visualize.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
        }
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

            Visualize.Sequential(urls, Download);
            Visualize.Parallel(urls, Download);
        }

        internal static void DownloadLargeFiles()
        {
            string[] urls = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Root.Element("channel").Elements("item")
                .Select(item => item.Element((XNamespace)"http://search.yahoo.com/mrss/" + "content").Attribute("url").Value)
                .ToArray();

            Visualize.Sequential(urls, Download);
            Visualize.Parallel(urls, Download);
        }

        internal static void ReadFiles()
        {
            string mscorlibPath = typeof(object).Assembly.Location;
            string gacPath = Path.GetDirectoryName(mscorlibPath);
            string[] files = Directory.GetFiles(gacPath);

            Visualize.Sequential(files, file => File.ReadAllBytes(file));
            Visualize.Parallel(files, file => File.ReadAllBytes(file));
        }
    }
}

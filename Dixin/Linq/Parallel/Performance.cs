namespace Dixin.Linq.Parallel
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

    public static class ArrayHelper
    {
        public static int[][] RandomArrays(int minValue, int maxValue, int length, int count)
            => Enumerable
                .Range(0, count)
                .Select(_ => EnumerableX.RandomInt32(minValue, maxValue).Take(length).ToArray())
                .ToArray();

        public static int[] RandomArray(int minValue, int maxValue, int length)
            => EnumerableX.RandomInt32(minValue, maxValue).Take(length).ToArray();
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

            VisualizerHelper.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            VisualizerHelper.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
        }

        internal static void QueryMediumArray()
        {
            int[][] arrays = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 2000, 50);
            VisualizerHelper.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            VisualizerHelper.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
        }

        internal static void QueryLargeArray()
        {
            int[][] arrays = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 50000, 2);
            VisualizerHelper.Run(arrays, array => array.OrderBy(value => value).ToArray(), false, "Sequential");
            VisualizerHelper.Run(arrays, array => array.AsParallel().OrderBy(value => value).ToArray(), false, "Parallel");
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

            VisualizerHelper.Sequential(urls, Download);
            VisualizerHelper.Parallel(urls, Download);
        }

        internal static void DownloadLargeFiles()
        {
            string[] urls = XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Root.Element("channel").Elements("item")
                .Select(item => item.Element((XNamespace)"http://search.yahoo.com/mrss/" + "content").Attribute("url").Value)
                .ToArray();

            VisualizerHelper.Sequential(urls, Download);
            VisualizerHelper.Parallel(urls, Download);
        }

        internal static void ReadFiles()
        {
            string mscorlibPath = typeof(object).Assembly.Location;
            string gacPath = Path.GetDirectoryName(mscorlibPath);
            string[] files = Directory.GetFiles(gacPath);

            VisualizerHelper.Sequential(files, file => File.ReadAllBytes(file));
            VisualizerHelper.Parallel(files, file => File.ReadAllBytes(file));
        }
    }
}

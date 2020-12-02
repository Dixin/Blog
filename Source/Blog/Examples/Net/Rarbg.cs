namespace Examples.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Examples.Linq;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;

    internal static class Rarbg
    {
        private const int SaveFrequency = 50;

        private static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(100);

        private static readonly TimeSpan DomWait = TimeSpan.FromMilliseconds(100);

        public static IWebDriver Start(string profile = @"D:\Temp\Chrome Profile")
        {
            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={profile}");
            ChromeDriver webDriver = new(options);
            return webDriver;
        }

        internal static async Task DownloadSummaryAsync(IEnumerable<string> urls, string jsonPath, int degreeOfParallelism = 4)
        {
            ConcurrentDictionary<string, RarbgMetadata[]> allSummaries = File.Exists(jsonPath)
                ? new(JsonSerializer.Deserialize<Dictionary<string, RarbgMetadata[]>>(await File.ReadAllTextAsync(jsonPath)) ?? throw new InvalidOperationException(jsonPath))
                : new();
            await urls.ParallelForEachAsync(async (url, index) => await DownloadSummaryAsync(url, jsonPath, allSummaries, index + 1), degreeOfParallelism);
            SaveJson(jsonPath, allSummaries);
        }

        private static async Task DownloadSummaryAsync(string url, string jsonPath, ConcurrentDictionary<string, RarbgMetadata[]> allSummaries, int partitionIndex)
        {
            try
            {
                using IWebDriver webDriver = Start(@$"D:\Temp\Chrome Profile {partitionIndex}");
                webDriver.Url = url;
                new WebDriverWait(webDriver, DefaultWait).Until(e => e.FindElement(By.Id("pager_links")));
                webDriver.Url = url;
                IWebElement pager = new WebDriverWait(webDriver, TimeSpan.FromSeconds(100)).Until(e => e.FindElement(By.Id("pager_links")));
                int pageIndex = 1;
                do
                {
                    await Task.Delay(DomWait);
                    Trace.WriteLine($"{partitionIndex}:{pageIndex} Start {webDriver.Url}");

                    webDriver
                        .FindElement(By.CssSelector("table.lista2t"))
                        .FindElements(By.CssSelector("tr.lista2"))
                        .Select(row =>
                        {
                            ReadOnlyCollection<IWebElement> cells = row.FindElements(By.TagName("td"));
                            string[] texts = cells[1].Text.Trim().Split(Environment.NewLine);
                            string title = texts[0].Trim();
                            ReadOnlyCollection<IWebElement> links = cells[1].FindElements(By.TagName("a"));
                            string link = links[0].GetAttribute("href");
                            string imdbId = links.Count > 1
                                ? links[1].GetAttribute("href").Replace("https://rarbg.to/torrents.php?imdb=", string.Empty).Trim()
                                : string.Empty;

                            string[] genres = new string[0];
                            string imdbRating = string.Empty;
                            if (texts.Length > 1)
                            {
                                string[] descriptions = texts[1].Trim().Split(" IMDB: ");
                                if (descriptions.Length > 0)
                                {
                                    genres = descriptions[0].Split(", ").Select(genre => genre.Trim()).ToArray();
                                }

                                if (descriptions.Length > 1)
                                {
                                    imdbRating = descriptions[1].Replace("/10", string.Empty).Trim();
                                }
                            }

                            int seed = int.TryParse(cells[4].Text.Trim(), out int seedValue) ? seedValue : -1;
                            int leech = int.TryParse(cells[5].Text.Trim(), out int leechValue) ? leechValue : -1;
                            return new RarbgMetadata(link, title, imdbId, imdbRating, genres, cells[2].Text.Trim(), cells[3].Text.Trim(), seed, leech, cells[7].Text.Trim());
                        })
                        .ForEach(summary =>
                        {
                            lock (AddItemLock)
                            {
                                allSummaries[summary.ImdbId] = allSummaries.ContainsKey(summary.ImdbId)
                                    ? allSummaries[summary.ImdbId].Where(existing => !string.Equals(existing.Title, summary.Title, StringComparison.OrdinalIgnoreCase)).Append(summary).ToArray()
                                    : new[] { summary };
                            }
                        });

                    if (pageIndex % SaveFrequency == 0)
                    {
                        SaveJson(jsonPath, allSummaries);
                    }

                    Trace.WriteLine($"{partitionIndex}:{pageIndex} End {webDriver.Url}");
                    pageIndex++;
                } while (webDriver.HasNextPage(ref pager));

                webDriver.Close();
                webDriver.Quit();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }
            finally
            {
                SaveJson(jsonPath, allSummaries);
            }
        }

        private static readonly object SaveJsonLock = new();

        private static void SaveJson(string jsonPath, IDictionary<string, RarbgMetadata[]> allSummaries)
        {
            string jsonString = JsonSerializer.Serialize(allSummaries, new() { WriteIndented = true });
            lock (SaveJsonLock)
            {
                File.WriteAllText(jsonPath, jsonString);
            }
        }

        private static readonly object AddItemLock = new();

        private static bool HasNextPage(this IWebDriver webDriver, ref IWebElement pager)
        {
            ReadOnlyCollection<IWebElement> nextPage = pager.FindElements(By.CssSelector("a[title='next page']"));
            if (nextPage.Count > 0)
            {
                webDriver.Url = nextPage[0].GetAttribute("href");
                pager = new WebDriverWait(webDriver, DefaultWait).Until(e => e.FindElement(By.Id("pager_links")));
                return true;
            }

            return false;
        }
    }
}

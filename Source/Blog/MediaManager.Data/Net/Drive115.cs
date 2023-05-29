namespace Examples.Net;

using System.Collections.ObjectModel;
using CsQuery;
using Examples.Common;
using Examples.IO;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

internal static class Drive115
{
    internal static List<Drive115OfflineTask> DownloadOfflineTasks(string url, Func<string, string, bool>? predicate = null, Action<string, string>? action = null, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        using IWebDriver parentFrame = WebDriverHelper.Start(isLoadingAll: true);
        parentFrame.Url = url;

        using IWebDriver? offlineTasksFrame = new WebDriverWait(parentFrame, WebDriverHelper.DefaultManualWait).Until(e => e.SwitchTo().Frame("wangpan"));
        List<Drive115OfflineTask> offlineTasks = new();
        string firstTask = string.Empty;
        for (int page = 1; ; page++)
        {
            log($"Start of page {page}.");
            new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(e =>
            {
                ReadOnlyCollection<IWebElement> tasksElements = e.FindElements(By.CssSelector("#js-warp li"));
                if (tasksElements.IsEmpty())
                {
                    return false;
                }

                if (firstTask.Equals(tasksElements.First().Text)) // StaleElementException.
                {
                    return false;
                }

                firstTask = tasksElements.First().Text;
                return true;
            });
            IWebElement currentPageElement = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(e => e.FindElement(By.CssSelector("#js-page div.con span.current")));
            IWebElement offlineTaskListElement = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(e => e.FindElement(By.Id("js-warp")));
            CQ listItemsCQ = offlineTaskListElement.GetAttribute("innerHTML");
            offlineTasks.AddRange(listItemsCQ.Select(listItemDom =>
            {
                CQ listItemCQ = listItemDom.Cq();
                string link = listItemCQ.Find("div.file-operate a[task_popup='copy']").Attr("cp_href") ?? string.Empty;
                string title = listItemCQ.Find("div.file-name").Text();
                if (predicate is not null && predicate(title, link) && action is not null)
                {
                    action(title, link);
                }

                string size = listItemCQ.Find("div.file-size").Text();
                bool isSuccessful = listItemCQ.Find("div.file-process i.ifst-success").Length > 0;
                bool isBlocked = listItemCQ.Find("div.desc-tips").Css("display") is "block";
                Drive115OfflineTask task = new(link, title, size, isSuccessful, isBlocked, page);
                log(JsonSerializer.Serialize(task, new JsonSerializerOptions() { WriteIndented = true }));
                return task;
            }));

            Debug.Assert(int.TryParse(currentPageElement.Text, out int currentPage) && currentPage == page);
            ReadOnlyCollection<IWebElement> paginationElements = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(e => e.FindElements(By.CssSelector("#js-page div.page-links a")));
            IWebElement? nextPageElement = paginationElements.SingleOrDefault(element => element.Text.EqualsOrdinal($"{page + 1}") && element.GetAttribute("start").EqualsOrdinal($"{30 * page}"));
            if (nextPageElement is null)
            {
                return offlineTasks;
            }

            log($"End of page {page}.");
            Retry.Incremental(() => nextPageElement.Click());
        }
    }

    internal static async Task WriteOfflineTasksAsync(string url, string path, string keyword = "", Action<string>? log = null)
    {
        List<Drive115OfflineTask> tasks = DownloadOfflineTasks(
            url,
            keyword.IsNullOrWhiteSpace()
                ? (_, _) => false
                : (title, link) => title.ContainsIgnoreCase(keyword) || link.ContainsIgnoreCase(keyword),
            (_, _) => Debugger.Break(),
            log);
        string jsonText = JsonSerializer.Serialize(tasks.ToArray(), new JsonSerializerOptions() { WriteIndented = true });
        await FileHelper.WriteTextAsync(path, jsonText);
    }
}

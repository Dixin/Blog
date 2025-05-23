﻿namespace MediaManager.Net;

using System.Collections.ObjectModel;
using CsQuery;
using Examples.Common;
using Examples.Net;
using MediaManager.IO;
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
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        using IWebDriver? offlineTasksFrame = new WebDriverWait(parentFrame, WebDriverHelper.DefaultManualWait).Until(driver => driver.SwitchTo().Frame("wangpan"));
        List<Drive115OfflineTask> offlineTasks = [];
        string firstTask = string.Empty;
        for (int page = 1; ; page++)
        {
            log($"Start of page {page}.");
            new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(driver =>
            {
                //ReadOnlyCollection<IWebElement> tasksElements = driver.FindElements(By.CssSelector("#js-warp li"));
                if (driver.FindElements(By.CssSelector("#js-warp li")).IsEmpty())
                {
                    return false;
                }

                string elementText = Retry.FixedInterval(() => driver.FindElements(By.CssSelector("#js-warp li")).First().Text);
                if (firstTask.Equals(elementText)) // StaleElementException.
                {
                    return false;
                }

                firstTask = elementText;
                return true;
            });
            IWebElement currentPageElement = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.CssSelector("#js-page div.con span.current")));
            IWebElement offlineTaskListElement = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElement(By.Id("js-warp")));
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
                log(JsonHelper.Serialize(task));
                return task;
            }));

            Debug.Assert(int.TryParse(currentPageElement.Text, out int currentPage) && currentPage == page);
            ReadOnlyCollection<IWebElement> paginationElements = new WebDriverWait(offlineTasksFrame, WebDriverHelper.DefaultManualWait).Until(driver => driver.FindElements(By.CssSelector("#js-page div.page-links a")));
            IWebElement? nextPageElement = paginationElements.SingleOrDefault(element => element.Text.EqualsOrdinal($"{page + 1}") && element.GetAttribute("start").EqualsOrdinal($"{30 * page}"));
            if (nextPageElement is null)
            {
                return offlineTasks;
            }

            log($"End of page {page}.");
            Retry.Incremental(nextPageElement.Click);
        }
    }

    internal static async Task WriteOfflineTasksAsync(string url, string path, Action<string>? log = null, params string[] keywords)
    {
        log ??= Logger.WriteLine;

        List<Drive115OfflineTask> tasks = DownloadOfflineTasks(
            url,
            keywords.IsEmpty()
                ? (_, _) => false
                : (title, link) => keywords.Any(keyword => title.ContainsIgnoreCase(keyword) || link.ContainsIgnoreCase(keyword)),
            (_, _) => Debugger.Break(),
            log);
        await JsonHelper.SerializeToFileAsync(tasks.ToArray(), path);
    }
}

namespace MediaManager.Net;

using CsQuery;
using Examples.Linq;
using Examples.Net;

internal static class Pictures
{
    private static string ReplaceInvalidFileNameChars(string text) =>
        Path.GetInvalidFileNameChars().Aggregate(text, (current, character) => current.Replace(character, '_'));

    internal static async Task DownloadAsync(string rootDirectory)
    {
        string[] sets = "https://www.jliji.com/set/13847\nhttps://www.jliji.com/set/13823\nhttps://www.jliji.com/set/13821\nhttps://www.jliji.com/set/13819\nhttps://www.jliji.com/set/13686\nhttps://www.jliji.com/set/13685\nhttps://www.jliji.com/set/13684\nhttps://www.jliji.com/set/13683\nhttps://www.jliji.com/set/13327\nhttps://www.jliji.com/set/1763\nhttps://www.jliji.com/set/2005\nhttps://www.jliji.com/set/2010\nhttps://www.jliji.com/set/2141\nhttps://www.jliji.com/set/2183\nhttps://www.jliji.com/set/18805\nhttps://www.jliji.com/set/15776\nhttps://www.jliji.com/set/15333\nhttps://www.jliji.com/set/21328\nhttps://www.jliji.com/set/21324\nhttps://www.jliji.com/set/21315\nhttps://www.jliji.com/set/21267\nhttps://www.jliji.com/set/21223\nhttps://www.jliji.com/set/21211\nhttps://www.jliji.com/set/21108\nhttps://www.jliji.com/set/21049\nhttps://www.jliji.com/set/20803\nhttps://www.jliji.com/set/20802\nhttps://www.jliji.com/set/20704\nhttps://www.jliji.com/set/20606\nhttps://www.jliji.com/set/19329\nhttps://www.jliji.com/set/19206\nhttps://www.jliji.com/set/18681\nhttps://www.jliji.com/set/18331\nhttps://www.jliji.com/set/17986\nhttps://www.jliji.com/set/17881\nhttps://www.jliji.com/set/17588\nhttps://www.jliji.com/set/16714\nhttps://www.jliji.com/set/16874\nhttps://www.jliji.com/set/16712\nhttps://www.jliji.com/set/15670\nhttps://www.jliji.com/set/15669\nhttps://www.jliji.com/set/15668\nhttps://www.jliji.com/set/15667\nhttps://www.jliji.com/set/14820\nhttps://www.jliji.com/set/14735\nhttps://www.jliji.com/set/14563\nhttps://www.jliji.com/set/14498\nhttps://www.jliji.com/set/14497\nhttps://www.jliji.com/set/13986\nhttps://www.jliji.com/set/13985\nhttps://www.jliji.com/set/13983\nhttps://www.jliji.com/set/13934\nhttps://www.jliji.com/set/13930\nhttps://www.jliji.com/set/13929\nhttps://www.jliji.com/set/13928\nhttps://www.jliji.com/set/13877\nhttps://www.jliji.com/set/13862"
            .Split("\n")
            .Distinct()
            .ToArray();
        using HttpClient httpClient = new HttpClient().AddEdgeHeaders();
        await sets.ForEachAsync(async set =>
        {
            string file = Path.Combine(rootDirectory, $"{set.Split('/').Last()}.html");
            string html;
            if (File.Exists(file))
            {
                html = await File.ReadAllTextAsync(file);
            }
            else
            {
                html = await httpClient.GetStringAsync($"{set}?lang=zh");
                await File.WriteAllTextAsync(file, html);
            }
            CQ cq = CQ.Create(html);
            string title = cq.Find("title").Text();
            string directory = Path.Combine(rootDirectory, ReplaceInvalidFileNameChars(title).Replace(" ", "_"));
            string[] imageUrls = cq.Find(".gallery-item img").Map(img => img.GetAttribute("data-full")).ToArray();
            string countText = Regex.Match(cq.Find(".set-detail-meta span").Text(), @"/\s+(\d+)\s+张图片").Groups[1].Value;
            int count = int.Parse(countText);
            if (count != imageUrls.Length)
            {
                Console.WriteLine($"Warning: Count mismatch for set {set}. Expected {count}, but found {imageUrls.Length} images.");
                Debugger.Break();
            }
            Directory.CreateDirectory(directory);
            await imageUrls.ForEachAsync(async imageUrl =>
            {
                string imageFile = Path.Combine(directory, Path.GetFileName(imageUrl));
                //if (!File.Exists(imageFile))
                {
                    byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);
                    await File.WriteAllBytesAsync(imageFile, imageData);
                }
            });
        });
    }
}
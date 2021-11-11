namespace Examples.IO;

using Examples.Common;
using Examples.Net;

internal static partial class Video
{
    internal static void BackupMetadata(string directory, string flag = DefaultBackupFlag)
    {
        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .ForEach(metadata => File.Copy(metadata, PathHelper.AddFilePostfix(metadata, $"{Delimiter}{flag}")));
    }

    internal static void RestoreMetadata(string directory, string flag = DefaultBackupFlag)
    {
        Directory
            .GetFiles(directory, XmlMetadataSearchPattern, SearchOption.AllDirectories)
            .Where(nfo => nfo.EndsWithOrdinal($"{Delimiter}{flag}{XmlMetadataExtension}"))
            .Where(nfo => File.Exists(nfo.Replace($"{Delimiter}{flag}{XmlMetadataExtension}", XmlMetadataExtension)))
            .ForEach(nfo => FileHelper.Move(nfo, Path.Combine(Path.GetDirectoryName(nfo) ?? throw new InvalidOperationException(nfo), (Path.GetFileNameWithoutExtension(nfo) ?? throw new InvalidOperationException(nfo)).Replace($"{Delimiter}{flag}", string.Empty) + Path.GetExtension(nfo)), true));
    }

    internal static void DeleteFeaturettesMetadata(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= TraceLog;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string featurettes = Path.Combine(movie, Featurettes);
                if (Directory.Exists(featurettes))
                {
                    string[] metadataFiles = Directory.GetFiles(featurettes, XmlMetadataSearchPattern, SearchOption.AllDirectories);
                    metadataFiles
                        .Do(log)
                        .Where(metadataFile => !isDryRun)
                        .ForEach(File.Delete);
                    // TODO.
                }
            });
    }

    private static void CreateSeasonEpisodeMetadata(string seasonDirectory, Func<string, string>? getTitle = null, Func<string, int, string>? getEpisode = null, Func<string, string>? getSeason = null, bool overwrite = false)
    {
        Directory.GetFiles(seasonDirectory, VideoSearchPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(video => video)
            .ForEach((video, index) =>
            {
                string metadataPath = PathHelper.ReplaceExtension(video, XmlMetadataExtension);
                if (!overwrite && File.Exists(metadataPath))
                {
                    return;
                }

                XDocument metadata = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<episodedetails>
  <plot />
  <outline />
  <lockdata>false</lockdata>
  <title></title>
  <episode></episode>
  <season></season>
</episodedetails>");
                string title = getTitle?.Invoke(video) ?? string.Empty;
                string episode = getEpisode?.Invoke(video, index) ?? string.Empty;
                string season = getSeason?.Invoke(video) ?? string.Empty;
                if (title.IsNullOrWhiteSpace() || episode.IsNullOrWhiteSpace() || season.IsNullOrWhiteSpace())
                {
                    Match match = Regex.Match(Path.GetFileNameWithoutExtension(video), @"\.S([0-9]+)E([0-9]+)\.(.*)");
                    if (match.Success)
                    {
                        if (season.IsNullOrWhiteSpace())
                        {
                            season = match.Groups[1].Value.TrimStart('0');
                        }

                        if (episode.IsNullOrWhiteSpace())
                        {
                            episode = match.Groups[2].Value.TrimStart('0');
                        }

                        if (title.IsNullOrWhiteSpace())
                        {
                            title = match.Groups[3].Value.Replace("BluRay.", string.Empty).Replace("WEBRip.", string.Empty).Replace("1080p.", string.Empty).Replace("720p.", string.Empty).Replace("ffmpeg.", string.Empty);
                        }
                    }
                }

                if (season.IsNullOrWhiteSpace())
                {
                    season = "1";
                }

                if (episode.IsNullOrWhiteSpace())
                {
                    episode = (index + 1).ToString();
                }

                if (title.IsNullOrWhiteSpace())
                {
                    title = $"Episode {episode}";
                }

                metadata.Root!.Element("title"!)!.Value = title;
                metadata.Root!.Element("episode"!)!.Value = episode;
                metadata.Root!.Element("season"!)!.Value = season;
                metadata.Save(metadataPath);
            });
    }

    internal static void CreateTVEpisodeMetadata(string tvDirectory, Func<string, string>? getTitle = null, Func<string, int, string>? getEpisode = null, Func<string, string>? getSeason = null, bool overwrite = false)
    {
        Directory
            .EnumerateDirectories(tvDirectory)
            .Where(season => !Path.GetFileName(season).EqualsIgnoreCase(Featurettes))
            .OrderBy(season => season)
            .ForEach(season => CreateSeasonEpisodeMetadata(season, getTitle, getEpisode, getSeason, overwrite));
    }

    internal static void SyncRating(string directory, int level = 2, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= TraceLog;
        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                string metadataPath = Directory.GetFiles(movie, XmlMetadataSearchPattern).First();
                XDocument metadata = XDocument.Load(metadataPath);
                XElement? ratingElement = metadata.Root!.Element("rating");
                if (Imdb.TryLoad(movie, out ImdbMetadata? imdbMetadata) && imdbMetadata.AggregateRating?.RatingValue is not null)
                {
                    // IMDB has rating.
                    string jsonRatingFormatted = imdbMetadata.AggregateRating.RatingValue.Replace(".0", string.Empty);
                    if ((ratingElement?.Value).EqualsOrdinal(jsonRatingFormatted))
                    {
                        return;
                    }

                    log($"{movie} rating {ratingElement?.Value} is set to {jsonRatingFormatted}.");
                    if (!isDryRun)
                    {
                        metadata.Root!.SetElementValue("rating", jsonRatingFormatted);
                        metadata.Save(metadataPath);
                    }
                }
                else
                {
                    // IMDB has no rating.
                    if (ratingElement is null)
                    {
                        return;
                    }

                    log($"{movie} rating {ratingElement.Value} is removed.");
                    if (!isDryRun)
                    {
                        ratingElement.Remove();
                        metadata.Save(metadataPath);
                    }
                }
            });
    }
}
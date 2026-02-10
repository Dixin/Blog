namespace MediaManager.IO;

using CsQuery;
using Examples.Common;
using Examples.IO;
using Examples.Linq;
using Examples.Net;
using Examples.Text;
using MediaManager.Net;
using Spectre.Console;

internal static partial class Video
{
    internal static void BackupMetadata(string directory, string flag = DefaultBackupFlag, bool overwrite = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory
            .GetFiles(directory, TmdbMetadata.NfoSearchPattern, SearchOption.AllDirectories)
            .Where(metadata => !PathHelper.GetFileNameWithoutExtension(metadata).EndsWithIgnoreCase($"{Delimiter}{flag}"))
            .Select(metadata => (Metadata: metadata, Backup: PathHelper.AddFilePostfix(metadata, $"{Delimiter}{flag}")))
            .Where(metadata => overwrite || !File.Exists(metadata.Backup))
            .Do(metadata => log(metadata.Backup))
            .ForEach(metadata => File.Copy(metadata.Metadata, metadata.Backup, overwrite));
    }

    internal static void RestoreMetadata(string directory, string flag = DefaultBackupFlag)
    {
        Directory
            .GetFiles(directory, TmdbMetadata.NfoSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.EndsWithOrdinal($"{Delimiter}{flag}{TmdbMetadata.NfoExtension}"))
            .Where(file => File.Exists(file.Replace($"{Delimiter}{flag}{TmdbMetadata.NfoExtension}", TmdbMetadata.NfoExtension)))
            .ForEach(file => FileHelper.Move(file, Path.Combine(PathHelper.GetDirectoryName(file), $"{PathHelper.GetFileNameWithoutExtension(file).Replace($"{Delimiter}{flag}", string.Empty)}{PathHelper.GetExtension(file)}"), true));
    }

    internal static void DeleteFeaturettesMetadata(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        EnumerateDirectories(directory, level)
            .ForEach(movie =>
            {
                string featurettes = Path.Combine(movie, Featurettes);
                if (Directory.Exists(featurettes))
                {
                    string[] metadataFiles = Directory.GetFiles(featurettes, TmdbMetadata.NfoSearchPattern, SearchOption.AllDirectories);
                    metadataFiles
                        .Do(log)
                        .Where(_ => !isDryRun)
                        .ForEach(FileHelper.Delete);
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
                string metadataPath = PathHelper.ReplaceExtension(video, TmdbMetadata.NfoExtension);
                if (!overwrite && File.Exists(metadataPath))
                {
                    return;
                }

                XDocument metadata = XDocument.Parse("""
                    <?xml version="1.0" encoding="utf-8" standalone="yes"?>
                    <episodedetails>
                      <plot />
                      <outline />
                      <lockdata>false</lockdata>
                      <title></title>
                      <episode></episode>
                      <season></season>
                    </episodedetails>
                    """);
                string title = getTitle?.Invoke(video) ?? string.Empty;
                string episode = getEpisode?.Invoke(video, index) ?? string.Empty;
                string season = getSeason?.Invoke(video) ?? string.Empty;
                if (title.IsNullOrWhiteSpace() || episode.IsNullOrWhiteSpace() || season.IsNullOrWhiteSpace())
                {
                    Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(video), @"\.S([0-9]+)E([0-9]+)\.(.*)");
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
                            title = match.Groups[3].Value.Replace("BluRay.", string.Empty).Replace("WEBRip.", string.Empty).Replace("1080p.", string.Empty).Replace("720p.", string.Empty).Replace($"{FfmpegHelper.Executable}{Delimiter}", string.Empty);
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

                metadata.Root!.Element("title")!.Value = title;
                metadata.Root!.Element("episode")!.Value = episode;
                metadata.Root!.Element("season")!.Value = season;
                metadata.Save(metadataPath);
            });
    }

    internal static void CreateTVEpisodeMetadata(string tvDirectory, Func<string, string>? getTitle = null, Func<string, int, string>? getEpisode = null, Func<string, string>? getSeason = null, bool overwrite = false)
    {
        Directory
            .EnumerateDirectories(tvDirectory)
            .Where(season => !PathHelper.GetFileName(season).EqualsIgnoreCase(Featurettes))
            .OrderBy(season => season)
            .ForEach(season => CreateSeasonEpisodeMetadata(season, getTitle, getEpisode, getSeason, overwrite));
    }

    internal static void UpdateXmlRating(string directory, int level = DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level)
            .ToArray()
            .ForEach(movie =>
            {
                ImdbMetadata.TryLoad(movie, out ImdbMetadata? imdbMetadata);
                string? jsonRatingFormatted = imdbMetadata?.AggregateRating?.RatingValue.Replace(".0", string.Empty).Trim();
                Directory
                    .GetFiles(movie, TmdbMetadata.NfoSearchPattern, SearchOption.TopDirectoryOnly)
                    .ForEach(xmlMetadataFile =>
                    {
                        XDocument metadata = XDocument.Load(xmlMetadataFile);
                        XElement? ratingElement = metadata.Root!.Element("rating");
                        if (jsonRatingFormatted is null)
                        {
                            if (ratingElement is null)
                            {
                                return;
                            }

                            log($"Rating {ratingElement.Value} is removed for {movie}.");
                            if (!isDryRun)
                            {
                                ratingElement.Remove();
                                metadata.Save(xmlMetadataFile);
                            }
                        }
                        else
                        {
                            if (jsonRatingFormatted.EqualsOrdinal(ratingElement?.Value))
                            {
                                return;
                            }

                            log($"Rating {ratingElement?.Value} is set to {jsonRatingFormatted} for {movie}.");
                            if (!isDryRun)
                            {
                                metadata.Root!.SetElementValue("rating", jsonRatingFormatted);
                                metadata.Save(xmlMetadataFile);
                            }
                        }
                    });
            });
    }

    internal static void MoveMetadata(string directory, string cacheDirectory, string metadataDirectory, int level = DefaultDirectoryLevel)
    {
        string[] cacheFiles = Directory.GetFiles(cacheDirectory, ImdbCacheSearchPattern);
        string[] metadataFiles = Directory.GetFiles(metadataDirectory, ImdbMetadataSearchPattern);

        EnumerateDirectories(directory, level).ForEach(movie =>
        {
            string[] files = Directory.GetFiles(movie);
            string? imdbId = files
                .Where(file => file.IsTmdbNfoMetadata() && !PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile))
                .Select(xml => XDocument.Load(xml).Root?.Element("imdbid")?.Value ?? string.Empty)
                .Distinct()
                .SingleOrDefault(string.Empty);

            if (imdbId.IsNullOrWhiteSpace() && !ImdbMetadata.TryGet(files, out string? _, out imdbId))
            {
                return;
            }

            cacheFiles
                .Where(file =>
                {
                    string name = PathHelper.GetFileNameWithoutExtension(file);
                    return name.EqualsIgnoreCase(imdbId) || name.StartsWithIgnoreCase($"{imdbId}{Delimiter}");
                })
                .ToArray()
                .ForEach(file => FileHelper.TryMoveToDirectory(file, movie));

            metadataFiles
                .Where(file => file.HasImdbId(imdbId))
                .ToArray()
                .ForEach(file => FileHelper.TryMoveToDirectory(file, movie));
        });
    }

    private static async Task<bool> DownloadImdbMetadataAsync(string directory, PlayWrightWrapper? playWrightWrapper, Lock? @lock = null, bool overwrite = false, bool useCache = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        HashSet<string> files = DirectoryHelper.GetFilesOrdinalIgnoreCase(directory);
        string jsonFile = files.SingleOrDefault(IsImdbMetadata, string.Empty);
        string imdbId;
        if (jsonFile.IsNotNullOrWhiteSpace())
        {
            if (ImdbMetadata.TryGet(jsonFile, out string? jsonImdbId))
            {
                imdbId = jsonImdbId;
            }
            else
            {
                log($"Skip {directory.EscapeMarkup()}.");
                return false;
            }
        }
        else
        {
            string[] nfoImdbIds = files
                .Where(IsTmdbNfoMetadata)
                .Select(file => file.TryLoadNfoImdbId(out string? nfoImdbId) ? nfoImdbId : string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (nfoImdbIds.Length == 0)
            {
                log($"!No JSON or XML metadata in {directory}.");
                return false;
            }

            if (nfoImdbIds.Length > 1)
            {
                log($"[red]!Inconsistent IMDB ids {string.Join(", ", nfoImdbIds)} in {directory.EscapeMarkup()}.[/]");
                return false;
            }

            imdbId = nfoImdbIds.Single();
            if (!imdbId.IsImdbId())
            {
                imdbId = NotExistingFlag;
            }
        }

        Debug.Assert(imdbId.IsImdbId() || imdbId.EqualsOrdinal(NotExistingFlag));
        log($"Start {directory.EscapeMarkup()}");
        await TmdbMetadata.WriteTmdbXmlMetadataAsync(directory, files, overwrite, log, cancellationToken);
        return await DownloadImdbMetadataAsync(imdbId, directory, directory, [jsonFile], files, playWrightWrapper, @lock, overwrite, useCache, log, cancellationToken);
    }

    internal static async Task<bool> DownloadImdbMetadataAsync(
        string imdbId, string metadataDirectory, string cacheDirectory, string[] metadataFiles, HashSet<string> cacheFiles,
        PlayWrightWrapper? playWrightWrapper, Lock? @lock = null, bool overwrite = false, bool useCache = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        long startingTimestamp = Stopwatch.GetTimestamp();
        if (metadataFiles.Select(PathHelper.GetFileNameWithoutExtension).SequenceEqual([NotExistingFlag], StringComparer.OrdinalIgnoreCase))
        {
            Debug.Assert(imdbId.EqualsOrdinal(NotExistingFlag));
            log($"Skip {imdbId}.");
            return false;
        }

        const string Advisories = nameof(Advisories);
        const string Awards = nameof(Awards);
        const string Connections = nameof(Connections);
        const string CrazyCredits = nameof(CrazyCredits);
        const string Credits = nameof(Credits);
        const string Goofs = nameof(Goofs);
        const string Keywords = nameof(Keywords);
        const string Quotes = nameof(Quotes);
        const string Releases = nameof(Releases);
        const string Soundtracks = nameof(Soundtracks);
        const string Trivia = nameof(Trivia);
        const string Versions = nameof(Versions);

        string imdbFile = Path.Combine(cacheDirectory, $"{imdbId}{ImdbCacheExtension}");
        string advisoriesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Advisories}{ImdbCacheExtension}");
        string awardsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Awards}{ImdbCacheExtension}");
        string connectionsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Connections}{ImdbCacheExtension}");
        string crazyCreditsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{CrazyCredits}{ImdbCacheExtension}");
        string creditsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Credits}{ImdbCacheExtension}");
        string goofsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Goofs}{ImdbCacheExtension}");
        string keywordsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Keywords}{ImdbCacheExtension}");
        string quotesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Quotes}{ImdbCacheExtension}");
        string releasesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Releases}{ImdbCacheExtension}");
        string soundtracksFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Soundtracks}{ImdbCacheExtension}");
        string triviaFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Trivia}{ImdbCacheExtension}");
        string versionsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Versions}{ImdbCacheExtension}");

        if (imdbId.EqualsOrdinal(NotExistingFlag))
        {
            //await new string[] { imdbFile, advisoriesFile, connectionsFile, creditsFile, keywordsFile, releasesFile }
            //    .Where(file => !cacheFiles.ContainsIgnoreCase(file))
            //    .ForEachAsync(async file => await File.WriteAllTextAsync(file, string.Empty, cancellationToken), cancellationToken);
            string notExistingJsonFile = Path.Combine(metadataDirectory, $"{NotExistingFlag}{ImdbMetadata.Extension}");
            if (!File.Exists(notExistingJsonFile))
            {
                await File.WriteAllTextAsync(notExistingJsonFile, string.Empty, cancellationToken);
            }

            return false;
        }

        string jsonFile = metadataFiles.SingleOrDefault(file => file.HasImdbId(imdbId), string.Empty);
        if (jsonFile.IsNotNullOrWhiteSpace() && !overwrite)
        {
            log($"Skip {imdbId}.");
            return false;
        }

        const string Parent = nameof(Parent);
        string parentImdbFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{ImdbCacheExtension}");
        string parentAdvisoriesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Advisories}{ImdbCacheExtension}");
        string parentAwardsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Awards}{ImdbCacheExtension}");
        string parentConnectionsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Connections}{ImdbCacheExtension}");
        string parentCrazyCreditsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{CrazyCredits}{ImdbCacheExtension}");
        string parentCreditsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Credits}{ImdbCacheExtension}");
        string parentGoofsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Goofs}{ImdbCacheExtension}");
        string parentKeywordsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Keywords}{ImdbCacheExtension}");
        string parentQuotesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Quotes}{ImdbCacheExtension}");
        string parentReleasesFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Releases}{ImdbCacheExtension}");
        string parentSoundtracksFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Soundtracks}{ImdbCacheExtension}");
        string parentTriviaFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Trivia}{ImdbCacheExtension}");
        string parentVersionsFile = Path.Combine(cacheDirectory, $"{imdbId}{Delimiter}{Parent}{Delimiter}{Versions}{ImdbCacheExtension}");

        (
            ImdbMetadata imdbMetadata,

            string imdbUrl, string imdbHtml,
            string advisoriesUrl, string advisoriesHtml,
            string awardsUrl, string awardsHtml,
            string connectionsUrl, string connectionsHtml,
            string crazyCreditsUrl, string crazyCreditsHtml,
            string creditsUrl, string creditsHtml,
            string goofsUrl, string goofsHtml,
            string keywordsUrl, string keywordsHtml,
            string quotesUrl, string quotesHtml,
            string releasesUrl, string releasesHtml,
            string soundtracksUrl, string soundtracksHtml,
            string triviaUrl, string triviaHtml,
            string versionsUrl, string versionsHtml,

            string parentImdbUrl, string parentImdbHtml,
            string parentAdvisoriesUrl, string parentAdvisoriesHtml,
            string parentAwardsUrl, string parentAwardsHtml,
            string parentConnectionsUrl, string parentConnectionsHtml,
            string parentCrazyCreditsUrl, string parentCrazyCreditsHtml,
            string parentCreditsUrl, string parentCreditsHtml,
            string parentGoofsUrl, string parentGoofsHtml,
            string parentKeywordsUrl, string parentKeywordsHtml,
            string parentQuotesUrl, string parentQuotesHtml,
            string parentReleasesUrl, string parentReleasesHtml,
            string parentSoundtracksUrl, string parentSoundtracksHtml,
            string parentTriviaUrl, string parentTriviaHtml,
            string parentVersionsUrl, string parentVersionsHtml
        ) = await Imdb.DownloadAsync(
            imdbId,

            useCache ? imdbFile : string.Empty,
            useCache ? advisoriesFile : string.Empty,
            useCache ? awardsFile : string.Empty,
            useCache ? connectionsFile : string.Empty,
            useCache ? crazyCreditsFile : string.Empty,
            useCache ? creditsFile : string.Empty,
            useCache ? goofsFile : string.Empty,
            useCache ? keywordsFile : string.Empty,
            useCache ? quotesFile : string.Empty,
            useCache ? releasesFile : string.Empty,
            useCache ? soundtracksFile : string.Empty,
            useCache ? triviaFile : string.Empty,
            useCache ? versionsFile : string.Empty,

            useCache ? parentImdbFile : string.Empty,
            useCache ? parentAdvisoriesFile : string.Empty,
            useCache ? parentAwardsFile : string.Empty,
            useCache ? parentConnectionsFile : string.Empty,
            useCache ? parentCrazyCreditsFile : string.Empty,
            useCache ? parentCreditsFile : string.Empty,
            useCache ? parentGoofsFile : string.Empty,
            useCache ? parentKeywordsFile : string.Empty,
            useCache ? parentQuotesFile : string.Empty,
            useCache ? parentReleasesFile : string.Empty,
            useCache ? parentSoundtracksFile : string.Empty,
            useCache ? parentTriviaFile : string.Empty,
            useCache ? parentVersionsFile : string.Empty,

            playWrightWrapper, cacheFiles, log, cancellationToken);
        Debug.Assert(imdbHtml.IsNotNullOrWhiteSpace());
        if (imdbMetadata.Regions.IsEmpty())
        {
            log($"[yellow]!Location is missing for {imdbId}: {cacheDirectory.EscapeMarkup()}.[/]");
        }

        if (!imdbId.EqualsIgnoreCase(imdbMetadata.ImdbId))
        {
            log($"Redirected {imdbId} to {imdbMetadata.ImdbId}.");
            imdbFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{ImdbCacheExtension}");

            advisoriesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Advisories}{ImdbCacheExtension}");
            awardsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Awards}{ImdbCacheExtension}");
            connectionsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Connections}{ImdbCacheExtension}");
            crazyCreditsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{CrazyCredits}{ImdbCacheExtension}");
            creditsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Credits}{ImdbCacheExtension}");
            goofsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Goofs}{ImdbCacheExtension}");
            keywordsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Keywords}{ImdbCacheExtension}");
            quotesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Quotes}{ImdbCacheExtension}");
            releasesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Releases}{ImdbCacheExtension}");
            soundtracksFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Soundtracks}{ImdbCacheExtension}");
            triviaFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Trivia}{ImdbCacheExtension}");
            versionsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Versions}{ImdbCacheExtension}");

            parentImdbFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{ImdbCacheExtension}");
            parentAdvisoriesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Advisories}{ImdbCacheExtension}");
            parentAwardsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Awards}{ImdbCacheExtension}");
            parentConnectionsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Connections}{ImdbCacheExtension}");
            parentCrazyCreditsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{CrazyCredits}{ImdbCacheExtension}");
            parentCreditsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Credits}{ImdbCacheExtension}");
            parentGoofsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Goofs}{ImdbCacheExtension}");
            parentKeywordsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Keywords}{ImdbCacheExtension}");
            parentQuotesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Quotes}{ImdbCacheExtension}");
            parentReleasesFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Releases}{ImdbCacheExtension}");
            parentSoundtracksFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Soundtracks}{ImdbCacheExtension}");
            parentTriviaFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Trivia}{ImdbCacheExtension}");
            parentVersionsFile = Path.Combine(cacheDirectory, $"{imdbMetadata.ImdbId}{Delimiter}{Parent}{Delimiter}{Versions}{ImdbCacheExtension}");
        }

        bool isDownloaded = false;
        (string Url, string File, string Html)[] files = new (string Url, string File, string Html)[]
            {
                (imdbUrl, imdbFile, imdbHtml),
                (advisoriesUrl, advisoriesFile, advisoriesHtml),
                (awardsUrl, awardsFile, awardsHtml),
                (connectionsUrl, connectionsFile, connectionsHtml),
                (crazyCreditsUrl, crazyCreditsFile, crazyCreditsHtml),
                (creditsUrl, creditsFile, creditsHtml),
                (goofsUrl, goofsFile, goofsHtml),
                (keywordsUrl, keywordsFile, keywordsHtml),
                (quotesUrl, quotesFile, quotesHtml),
                (releasesUrl, releasesFile, releasesHtml),
                (soundtracksUrl, soundtracksFile, soundtracksHtml),
                (triviaUrl, triviaFile, triviaHtml),
                (versionsUrl, versionsFile, versionsHtml),

                (parentImdbUrl, parentImdbFile, parentImdbHtml),
                (parentAdvisoriesUrl, parentAdvisoriesFile, parentAdvisoriesHtml),
                (parentAwardsUrl, parentAwardsFile, parentAwardsHtml),
                (parentConnectionsUrl, parentConnectionsFile, parentConnectionsHtml),
                (parentCrazyCreditsUrl, parentCrazyCreditsFile, parentCrazyCreditsHtml),
                (parentCreditsUrl, parentCreditsFile, parentCreditsHtml),
                (parentGoofsUrl, parentGoofsFile, parentGoofsHtml),
                (parentKeywordsUrl, parentKeywordsFile, parentKeywordsHtml),
                (parentQuotesUrl, parentQuotesFile, parentQuotesHtml),
                (parentReleasesUrl, parentReleasesFile, parentReleasesHtml),
                (parentSoundtracksUrl, parentSoundtracksFile, parentSoundtracksHtml),
                (parentTriviaUrl, parentTriviaFile, parentTriviaHtml),
                (parentVersionsUrl, parentVersionsFile, parentVersionsHtml)
            }
            .Where(data => data.Html.IsNotNullOrWhiteSpace()
                && ((!useCache && overwrite) || !cacheFiles.Contains(data.File)))
            .ToArray();

        if (@lock is not null)
        {
            lock (@lock)
            {
                files.ForEach(data =>
                {
                    isDownloaded = true;
                    log($"Downloaded {data.Url} to {data.File.EscapeMarkup()}.");
                    File.WriteAllText(data.File, data.Html);
                    log($"Saved to {data.File.EscapeMarkup()}.");
                });

                string newJsonFile = imdbMetadata.GetFilePath(metadataDirectory);
                if (jsonFile.IsNotNullOrWhiteSpace() && !jsonFile.EqualsIgnoreCase(newJsonFile))
                {
                    FileHelper.Recycle(jsonFile);
                }

                log($"Merged {imdbUrl}, {advisoriesUrl}, {connectionsUrl}, {crazyCreditsUrl}, {creditsUrl}, {goofsUrl}, {keywordsUrl}, {quotesUrl}, {releasesUrl}, {soundtracksUrl}, {triviaUrl}, {versionsUrl} to {newJsonFile.EscapeMarkup()}.");
                JsonHelper.SerializeToFile(imdbMetadata, newJsonFile);
                TimeSpan elapsed = Stopwatch.GetElapsedTime(startingTimestamp);
                log($"[green]Elapsed {elapsed} to Save {newJsonFile.EscapeMarkup()}.[/]");
            }
        }
        else
        {
            await files.ForEachAsync(
                async data =>
                {
                    isDownloaded = true;
                    log($"Downloaded {data.Url} to {data.File.EscapeMarkup()}.");
                    await File.WriteAllTextAsync(data.File, data.Html, cancellationToken);
                    log($"Saved to {data.File.EscapeMarkup()}.");
                },
                cancellationToken);

            string newJsonFile = imdbMetadata.GetFilePath(metadataDirectory);
            if (jsonFile.IsNotNullOrWhiteSpace() && !jsonFile.EqualsIgnoreCase(newJsonFile))
            {
                FileHelper.Recycle(jsonFile);
            }

            log($"Merged {imdbUrl}, {advisoriesUrl}, {connectionsUrl}, {crazyCreditsUrl}, {creditsUrl}, {goofsUrl}, {keywordsUrl}, {quotesUrl}, {releasesUrl}, {soundtracksUrl}, {triviaUrl}, {versionsUrl} to {newJsonFile.EscapeMarkup()}.");
            await JsonHelper.SerializeToFileAsync(imdbMetadata, newJsonFile, cancellationToken);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startingTimestamp);
            log($"[green]Elapsed {elapsed} to Save {newJsonFile.EscapeMarkup()}.[/]");
        }

        return isDownloaded;
    }

    internal static async Task DownloadImdbMetadataAsync(
        (string directory, int level)[] directories, Func<VideoDirectoryInfo, bool> predicate,
        bool overwrite = false, bool useCache = false, bool useBrowser = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        string[] movies = directories
            .SelectMany(directory => EnumerateDirectories(directory.directory, directory.level))
            .Where(directory => predicate(VideoDirectoryInfo.Parse(directory)))
            .ToArray();
        if (movies.Any())
        {
            await using PlayWrightWrapper playWrightWrapper = new("https://www.imdb.com/");
            Lock @lock = new();
            await movies.ForEachAsync(async movie => await DownloadImdbMetadataAsync(movie, playWrightWrapper, @lock, overwrite, useCache, log, cancellationToken), cancellationToken);
        }
    }

    internal static async Task WriteLibraryMovieMetadataAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params string[][] directoryDrives)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> existingMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);

        existingMetadata
            .Values
            .SelectMany(group => group.Keys, (group, video) => (group, video))
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .Where(video => !File.Exists(Path.IsPathRooted(video.video) ? video.video : Path.Combine(settings.LibraryDirectory, video.video)))
            .ForAll(video =>
            {
                log($"Delete {video.video}.");
                Debug.Assert(video.group.TryRemove(video.video, out _));
            });

        HashSet<string> existingVideos = existingMetadata.Values.SelectMany(group => group.Keys).ToHashSetOrdinalIgnoreCase();
        directoryDrives
            // Hard drives in parallel.
            .AsParallel()
            .ForAll(driveDirectories =>
            {
                ConcurrentDictionary<string, string> allMetadataFiles = new();
                ConcurrentDictionary<string, ConcurrentBag<string>> newVideos = new();

                driveDirectories
                    // IO per hard drive, sequential.
                    .SelectMany(directory => Directory.EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories))
                    // No IO involved.
                    .AsParallel()
                    .ForAll(file =>
                    {
                        string directory = PathHelper.GetDirectoryName(file);
                        if (file.IsImdbMetadata())
                        {
                            allMetadataFiles[directory] = file;
                            return;
                        }

                        if (file.IsVideo() && !file.IsDiskImage() && !existingVideos.Contains(Path.GetRelativePath(settings.LibraryDirectory, file)))
                        {

                            if (Path.GetFileName(directory).EqualsIgnoreCase(Featurettes))
                            {
                                return;
                            }

                            newVideos.AddOrUpdate(
                                directory,
                                key => [file],
                                (key, videos) =>
                                {
                                    videos.Add(file);
                                    return videos;
                                });
                        }
                    });

                Debug.Assert(newVideos.Values.All(group => group.Any()));
                Dictionary<string, Dictionary<string, VideoMetadata?>> newVideoMetadata = newVideos
                    .AsParallel()
                    .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
                    .Select(group =>
                    {
                        ImdbMetadata? imdbMetadata = null;
                        if (!allMetadataFiles.TryGetValue(group.Key, out string? metadataFile))
                        {
                            log($"!Metadata is missing for {group.Key}.");
                        }
                        else if (!ImdbMetadata.TryLoad(metadataFile, out imdbMetadata))
                        {
                            log($"!Metadata is unavailable for {group.Key}.");
                        }

                        return (Videos: group.Value, Metadata: imdbMetadata);
                    })
                    .SelectMany(group => group
                        .Videos
                        .Select(video =>
                        {
                            if (!TryReadVideoMetadata(video, out VideoMetadata? videoMetadata, group.Metadata, settings.LibraryDirectory))
                            {
                                log($"!Fail: {video}");
                            }

                            return (
                                ImdbId: group.Metadata?.ImdbId ?? string.Empty,
                                RelativePath: Path.GetRelativePath(settings.LibraryDirectory, video),
                                Metadata: videoMetadata);
                        }))
                    .ToLookup(video => video.ImdbId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToDictionary(video => video.RelativePath, video => video.Metadata));

                newVideoMetadata.ForEach(group => existingMetadata.AddOrUpdate(group.Key,
                    key =>
                    {
                        log($"Create {group.Key} with {string.Join("|", group.Value.Keys)}.");
                        return new ConcurrentDictionary<string, VideoMetadata?>(group.Value);
                    },
                    (key, videos) =>
                    {
                        group.Value.ForEach(video =>
                        {
                            log($"Add to {group.Key} with {video.Key}.");
                            Debug.Assert(videos.TryAdd(video.Key, video.Value));
                        });
                        return videos;
                    }));
            });

        existingMetadata
            .Where(group => group.Value.IsEmpty())
            .Do(group => log($"Delete {group.Key}."))
            .ToArray()
            .ForEach(group => Debug.Assert(existingMetadata.TryRemove(group)));

        await settings.WriteMovieLibraryMetadataAsync(existingMetadata, cancellationToken);
    }

    internal static async Task WriteExternalVideoMetadataAsync(ISettings settings, CancellationToken cancellationToken = default, params string[] directories)
    {
        Dictionary<string, VideoMetadata> allVideoMetadata = directories
            .SelectMany(directory => Directory.GetFiles(directory, VideoSearchPattern, SearchOption.AllDirectories))
            .Select(video =>
            {
                string metadata = PathHelper.ReplaceExtension(video, TmdbMetadata.NfoExtension);
                string imdbId = metadata.TryLoadNfoImdbId(out string? xmlImdbId) ? xmlImdbId : throw new InvalidOperationException(video);
                if (TryReadVideoMetadata(video, out VideoMetadata? videoMetadata))
                {
                    return (ImdbId: imdbId, Value: videoMetadata);
                }

                throw new InvalidOperationException(video);
            })
            .Distinct(metadata => metadata.ImdbId)
            .ToDictionary(metadata => metadata.ImdbId, metadata => metadata.Value);

        await settings.WriteMovieExternalMetadataAsync(allVideoMetadata, cancellationToken);
    }

    internal static async Task WriteMergedMovieMetadataAsync(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        ConcurrentDictionary<string, ConcurrentDictionary<string, VideoMetadata>> libraryMetadata = await settings.LoadMovieLibraryMetadataAsync(cancellationToken);
        ConcurrentDictionary<string, ImdbMetadata> mergedMetadata = await settings.LoadMovieMergedMetadataAsync(cancellationToken);
        ILookup<string, string> cacheFilesByImdbId = Directory
            .EnumerateFiles(settings.MovieMetadataCacheDirectory, ImdbCacheSearchPattern)
            .ToLookup(file => PathHelper.GetFileNameWithoutExtension(file).Split(Delimiter).First());
        Dictionary<string, string> metadataFilesByImdbId = Directory
            .EnumerateFiles(settings.MovieMetadataDirectory, ImdbMetadataSearchPattern)
            .ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        HashSet<string> topLibraryImdbIds = libraryMetadata
            .Where(group => group
                .Value
                .Select(video => PathHelper.GetFileNameWithoutExtension(video.Key))
                .Any(name => name.ContainsIgnoreCase(".1080p.") && (name.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopEnglishKeyword}") || name.EndsWithIgnoreCase($"{VersionSeparator}{settings.TopForeignKeyword}"))))
            .Select(group => group.Key)
            .ToHashSetOrdinalIgnoreCase();
        Action<string> recycleMetadata = settings.MovieMetadataBackupDirectory.IsNullOrWhiteSpace()
            ? FileHelper.Recycle
            : file => FileHelper.MoveToDirectory(file, settings.MovieMetadataBackupDirectory, true, true);
        Action<string> recycleCache = settings.MovieMetadataCacheBackupDirectory.IsNullOrWhiteSpace()
            ? FileHelper.Recycle
            : file => FileHelper.MoveToDirectory(file, settings.MovieMetadataCacheBackupDirectory, true, true);
        KeyValuePair<string, string>[] metadataToDelete = metadataFilesByImdbId
            .Where(metadataFile => topLibraryImdbIds.Contains(metadataFile.Key))
            .ToArray();
        log($"Delete {metadataToDelete.Length}");
        metadataToDelete
            .ForEach(metadataFile =>
            {
                log($"Delete {metadataFile.Value}");
                recycleMetadata(metadataFile.Value);
                metadataFilesByImdbId.Remove(metadataFile.Key);
            });

        cacheFilesByImdbId
            .Where(group => topLibraryImdbIds.Contains(group.Key))
            .ToArray()
            .ForEach(group => group.ForEach(file =>
            {
                log($"Delete {file}");
                recycleCache(file);
            }));

        mergedMetadata
            .Keys
            .Except(metadataFilesByImdbId.Keys, StringComparer.OrdinalIgnoreCase)
            .Do(imdbId => log($"Delete {imdbId}."))
            .ToArray()
            .ForEach(imdbId => Debug.Assert(mergedMetadata.TryRemove(imdbId, out _)));

        string[] newImdbIds = metadataFilesByImdbId
            .Keys
            .Except(mergedMetadata.Keys, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        log($"Add {newImdbIds.Length}.");
        newImdbIds
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForAll(imdbId =>
            {
                string file = metadataFilesByImdbId[imdbId];
                if (ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata))
                {
                    log($"Add {file}.");
                    mergedMetadata[imdbId] = imdbMetadata;
                }
                else
                {
                    Debug.Fail(file);
                }
            });

        await settings.WriteMovieMergedMetadataAsync(mergedMetadata, cancellationToken);
    }

    internal static async Task MergeMovieMetadataAsync(ISettings settings, string metadataDirectory, CancellationToken cancellationToken = default)
    {
        ConcurrentDictionary<string, ImdbMetadata> mergedMetadata = new();

        string[] movieMetadataFiles = Directory.GetFiles(metadataDirectory);
        Dictionary<string, string> metadataFilesByImdbId = movieMetadataFiles
            .ToDictionary(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        metadataFilesByImdbId
            .Keys
            .AsParallel()
            .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
            .ForEach(imdbId =>
            {
                string file = metadataFilesByImdbId[imdbId];
                if (ImdbMetadata.TryLoad(file, out ImdbMetadata? imdbMetadata))
                {
                    mergedMetadata[imdbId] = imdbMetadata;
                }
                else
                {
                    throw new InvalidOperationException(file);
                }
            });

        await settings.WriteMovieMergedMetadataAsync(mergedMetadata, cancellationToken);
    }

    internal static async Task DownloadMissingTitlesFromDoubanAsync(ISettings settings, string directory, int level = DefaultDirectoryLevel, bool skipFormatted = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        WebDriverHelper.DisposeAll();

        ConcurrentQueue<string> movies = new(skipFormatted
            ? EnumerateDirectories(directory, level).Where(movie => !VideoDirectoryInfo.TryParse(movie, out _))
            : EnumerateDirectories(directory, level));
        List<(string Title, string Year, string Directory)> noTranslation = [];
        await Enumerable
            .Range(0, Douban.MaxDegreeOfParallelism)
            .ParallelForEachAsync(
                async (webDriverIndex, _, token) =>
                {
                    using WebDriverWrapper webDriver = new(() => WebDriverHelper.Start(webDriverIndex, keepExisting: true));
                    while (movies.TryDequeue(out string? movie))
                    {
                        string[] metadataFiles = Directory
                            .EnumerateFiles(movie, TmdbMetadata.NfoSearchPattern)
                            .ToArray();
                        string backupMetadataFile = metadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase($"movie{Delimiter}{DefaultBackupFlag}{TmdbMetadata.NfoExtension}"));
                        string metadataFile = metadataFiles.Single(file => PathHelper.GetFileName(file).EqualsIgnoreCase($"movie{TmdbMetadata.NfoExtension}"));
                        XDocument metadataDocument = XDocument.Load(metadataFile);
                        string translatedTitle = metadataDocument.Root!.Element("title")!.Value;
                        if (translatedTitle.ContainsChineseCharacter() || Regex.IsMatch(translatedTitle, "^[0-9]+$"))
                        {
                            continue;
                        }

                        XDocument backupMetadataDocument = XDocument.Load(backupMetadataFile);
                        string backupOriginalTitle = backupMetadataDocument.Root!.Element("originaltitle")?.Value ?? string.Empty;
                        string backupEnglishTitle = backupMetadataDocument.TryGetTitle(out string? xmlTitle) ? xmlTitle : string.Empty;
                        string year = backupMetadataDocument.Root!.Element("year")?.Value ?? string.Empty;

                        if (!backupMetadataFile.TryLoadNfoImdbId(out string? imdbId)
                            || translatedTitle.IsNullOrEmpty()) // Already searched Douban, no result.
                        {
                            if (backupOriginalTitle.IsNullOrWhiteSpace() || !backupOriginalTitle.ContainsCjkCharacter())
                            {
                                noTranslation.Add((backupEnglishTitle, year, movie));
                            }

                            continue;
                        }

                        string doubanTitle = await Douban.GetTitleAsync(webDriver, imdbId, token);
                        int lastIndex = doubanTitle.LastIndexOfIgnoreCase(backupEnglishTitle);
                        if (lastIndex >= 0)
                        {
                            doubanTitle = doubanTitle[..lastIndex].Trim();
                        }

                        string originalTitle = backupMetadataDocument.Root!.Element("originaltitle")?.Value ?? string.Empty;
                        if (originalTitle.IsNotNullOrWhiteSpace())
                        {
                            lastIndex = doubanTitle.LastIndexOfIgnoreCase(originalTitle);
                            if (lastIndex >= 0)
                            {
                                doubanTitle = doubanTitle[..lastIndex].Trim();
                            }
                        }

                        log($"""
                            {movie}
                            {backupEnglishTitle}
                            {translatedTitle}
                            {doubanTitle}
                            
                            """);
                        if (!doubanTitle.EqualsIgnoreCase(translatedTitle))
                        {
                            metadataFiles
                                .Where(file => !file.EndsWithIgnoreCase($"{Delimiter}{DefaultBackupFlag}{TmdbMetadata.NfoExtension}"))
                                .ToArray()
                                .ForEach(metadataFile =>
                                {
                                    XDocument metadataDocument = XDocument.Load(metadataFile);
                                    metadataDocument.Root!.Element("title")!.Value = doubanTitle;
                                    metadataDocument.Save(metadataFile);
                                });
                        }

                        if (!doubanTitle.ContainsCjkCharacter() && !backupOriginalTitle.ContainsCjkCharacter())
                        {
                            noTranslation.Add((backupEnglishTitle, year, movie));
                        }

                        await Task.Delay(TimeSpan.FromSeconds(15), token);
                    }
                },
                Douban.MaxDegreeOfParallelism,
                cancellationToken);

        noTranslation.ForEach(movie => log($"{movie.Title} ({movie.Year})"));
        log(string.Empty);
        noTranslation.OrderBy(movie => movie.Directory).ForEach(movie => log($"{movie.Title} ({movie.Year}) - {movie.Directory}"));

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        string[] translatedTitles = await File.ReadAllLinesAsync(settings.TempFile, cancellationToken);
        Debug.Assert(noTranslation.Count == translatedTitles.Length);
        noTranslation.ForEach((movie, index) =>
        {
            string translatedTitle = translatedTitles[index];
            string year = translatedTitle[^5..^1];
            Debug.Assert(movie.Year.EqualsOrdinal(year));
            Directory.EnumerateFiles(movie.Directory, TmdbMetadata.NfoSearchPattern)
                .Where(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile))
                .ToArray()
                .ForEach(metadataFile =>
                {
                    XDocument metadataDocument = XDocument.Load(metadataFile);
                    metadataDocument.Root!.Element("title")!.Value = translatedTitle[..^7].Trim();
                    metadataDocument.Save(metadataFile);
                });
        });
    }

    internal static async Task WriteNikkatsuMetadataAsync(string cacheFile, string jsonFile)
    {
        const string WikipediaUri = "https://en.wikipedia.org";

        CQ mainCQ = await File.ReadAllTextAsync(cacheFile);
        WikipediaNikkatsu[] movies = mainCQ
            .Find("table.wikitable > tbody > tr")
            .Select(row => (row, cells: row.Cq().Find("td")))
            .Where(row => row.cells.Length == 5)
            .Select(row =>
            {
                CQ cells = row.cells;
                string releaseDate = cells[0].TextContent.Trim();

                string[] englishTitles = cells.Eq(1).Find("i > b").Select(dom => dom.TextContent).ToArray();
                Debug.Assert(englishTitles.Length is 0 or 1 or 2);
                string originalTitle = cells[1]
                    .ChildNodes
                    .Where(node => node.NodeType == NodeType.TEXT_NODE)
                    .Select(node => node.ToString() ?? string.Empty)
                    .FirstOrDefault(text => text.IsNotNullOrWhiteSpace(), string.Empty);
                CQ translatedTitleCQ = cells.Eq(1).Find("i").Last();
                string translatedTitle = translatedTitleCQ.Find("b").Any() ? string.Empty : translatedTitleCQ.Text();
                CQ linkCQ = cells.Eq(1).Find("a");
                Debug.Assert(linkCQ.Length is 0 or 1 or 2);
                string[] links = linkCQ.Select(dom => $"{WikipediaUri}{dom.GetAttribute("href")}").ToArray();

                string director = cells[2].TextContent.Trim();

                string[] cast = cells[3]
                    .ChildNodes
                    .Select(node => node.NodeType switch
                    {
                        NodeType.TEXT_NODE => node.ToString()?.Trim() ?? string.Empty,
                        _ => node.TextContent.Trim()
                    })
                    .Where(text => text.IsNotNullOrWhiteSpace())
                    .Order()
                    .ToArray();

                string note = cells[4].TextContent.Trim();

                return new WikipediaNikkatsu(releaseDate, originalTitle, translatedTitle, director, note)
                {
                    EnglishTitles = englishTitles,
                    Links = links,
                    Cast = cast,
                };
            })
            .DistinctBy(movie => (movie.ReleaseDate, movie.OriginalTitle))
            .ToArray();

        await JsonHelper.SerializeToFileAsync(movies, jsonFile);
    }

    internal static void CopyMetadata(string sourceDirectory, string destinationDirectory, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        ILookup<string, string> sourceMetadataFiles = Directory
            .EnumerateFiles(sourceDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .ToLookup(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);
        ILookup<string, string> destinationMetadataFiles = Directory
            .EnumerateFiles(destinationDirectory, ImdbMetadataSearchPattern, SearchOption.AllDirectories)
            .ToLookup(file => ImdbMetadata.TryGet(file, out string? imdbId) ? imdbId : string.Empty);

        destinationMetadataFiles
            .Where(destinationGroup => destinationGroup.Key.IsNotNullOrWhiteSpace() && sourceMetadataFiles.Contains(destinationGroup.Key))
            .ForEach(destinationGroup =>
            {
                string imdbId = destinationGroup.Key;
                Debug.Assert(imdbId.IsImdbId());
                string[] sourceGroup = sourceMetadataFiles[imdbId].ToArray();
                Debug.Assert(sourceGroup.Any());
                (string File, DateTime LastWriteUtc) sourceMetadataFile = sourceGroup
                    .Select(file => (File: file, LastWriteUtc: new FileInfo(file).LastWriteTimeUtc))
                    .MaxBy(file => file.LastWriteUtc);
                string sourceMetadataDirectory = PathHelper.GetDirectoryName(sourceMetadataFile.File);
                string[] sourceFiles = Directory
                    .EnumerateFiles(sourceMetadataDirectory, ImdbCacheSearchPattern, SearchOption.TopDirectoryOnly)
                    .Append(sourceMetadataFile.File)
                    .ToArray();
                destinationGroup
                    .Where(destinationMetadataFile => new FileInfo(destinationMetadataFile).LastWriteTimeUtc < sourceMetadataFile.LastWriteUtc)
                    .ForEach(destinationMetadataFile =>
                    {
                        string destinationMetadataDirectory = PathHelper.GetDirectoryName(destinationMetadataFile);
                        Directory
                            .EnumerateFiles(destinationMetadataDirectory, ImdbCacheSearchPattern, SearchOption.TopDirectoryOnly)
                            .Append(destinationMetadataFile)
                            .ToArray()
                            .ForEach(file =>
                            {
                                log($"Delete {file}.");
                                if (!isDryRun)
                                {
                                    FileHelper.Recycle(file);
                                }
                            });
                        sourceFiles.ForEach(file =>
                        {
                            log($"Copy {file} to {destinationMetadataDirectory}.");
                            if (!isDryRun)
                            {
                                FileHelper.CopyToDirectory(file, destinationMetadataDirectory);
                            }
                        });
                    });
            });
    }

    internal static void UpdateCertifications(ISettings settings, Action<string>? log = null, CancellationToken cancellationToken = default, params string[][] directoryDrives)
    {
        log ??= Logger.WriteLine;

        directoryDrives
            .AsParallel()
            .ForAll(driveDirectories =>
            {
                driveDirectories
                    .SelectMany(directory => Directory.EnumerateFiles(directory, ImdbMetadataSearchPattern, SearchOption.AllDirectories))
                    .Where(metadataFile => !PathHelper.GetFileNameWithoutExtension(metadataFile).EqualsIgnoreCase(NotExistingFlag))
                    .Select(metadataFile => (MetadataFile: metadataFile, AdvisoriesFile: PathHelper.ReplaceFileName(metadataFile, $"{metadataFile.GetImdbId()}.Advisories.log")))
                    .AsParallel()
                    .WithDegreeOfParallelism(IOMaxDegreeOfParallelism)
                    .ForAll(file =>
                    {
                        ImdbMetadata imdbMetadata = JsonHelper.DeserializeFromFile<ImdbMetadata>(file.MetadataFile);
                        if (imdbMetadata.Certifications is not null && imdbMetadata.Certifications.Any())
                        {
                            return;
                        }

                        CQ advisoriesCQ = File.ReadAllText(file.AdvisoriesFile);
                        Dictionary<string, string> certifications = advisoriesCQ
                            .Find("#certificates li.ipl-inline-list__item")
                            .Select(certificationDom => (
                                Certification: Regex.Replace(certificationDom.TextContent.Trim(), @"\s+", " "),
                                Link: certificationDom.Cq().Find("a").Attr("href").Trim()))
                            .DistinctBy(certification => certification.Certification)
                            .ToDictionary(certification => certification.Certification, certification => certification.Link);
                        if (certifications.IsEmpty())
                        {
                            return;
                        }

                        log(file.MetadataFile);
                        log(string.Join(Environment.NewLine, certifications.Keys));
                        imdbMetadata = imdbMetadata with { Certifications = certifications };
                        JsonHelper.SerializeToFile(imdbMetadata, file.MetadataFile);
                    });
            });
    }

    internal static void CopyMovieMetadata(string directory, int level = DefaultDirectoryLevel, bool overwrite = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        EnumerateDirectories(directory, level).ForEach(movie =>
        {
            string[] files = Directory.GetFiles(movie);
            string metadata = files.SingleOrDefault(file => PathHelper.GetFileName(file).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile), string.Empty);
            if (metadata.IsNullOrWhiteSpace())
            {
                metadata = files.FirstOrDefault(IsTmdbNfoMetadata, string.Empty);
                if (metadata.IsNullOrWhiteSpace())
                {
                    return;
                }
            }

            string[] videos = files
                .Where(file =>
                {
                    if (!file.IsVideo())
                    {
                        return false;
                    }

                    Match match = Regex.Match(PathHelper.GetFileNameWithoutExtension(file), @"\.cd([0-9]{1,2})$");
                    if (!match.Success)
                    {
                        return true;
                    }

                    int cd = int.Parse(match.Groups[1].Value);
                    return cd == 1;
                })
                .ToArray();
            videos.ForEach(video =>
            {
                string videoName = PathHelper.GetFileNameWithoutExtension(video);
                string videoMetadata = PathHelper.ReplaceFileNameWithoutExtension(metadata, videoName);
                if (!overwrite && File.Exists(videoMetadata))
                {
                    return;
                }

                FileHelper.Copy(metadata, videoMetadata, overwrite, true);
                log(videoMetadata);
            });
        });
    }

    internal static void SyncTmdbMetadata(string directory, int level = 2)
    {
        EnumerateDirectories(directory, level)
        .ForEach(movie =>
        {
            string[] movieMetadata = Directory.GetFiles(movie, TmdbMetadata.NfoSearchPattern);
            XDocument[] documents = movieMetadata.Select(XDocument.Load).ToArray();
            Debug.Assert(movieMetadata.Length >= 2);
            Debug.Assert(documents.Select(document => document.Root!.Element("imdbid")?.Value).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1);
            Debug.Assert(documents.Select(document => document.Root!.Element("tmdbid")?.Value).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1);
            Debug.Assert(documents.Select(document => document.Root!.Element("id")?.Value).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1);
            Debug.Assert(!movieMetadata.Select(PathHelper.GetFileNameWithoutExtension).Any(name => name.ContainsIgnoreCase(".cd2") || name.ContainsIgnoreCase(".cd02")));

            string[] years = documents.Select(document => document.Root?.Element("year")?.Value ?? string.Empty).Distinct().ToArray();
            string[][] countries = documents.Select(document => document.Root!.Elements("country").Select(element => element.Value).Order().ToArray()).ToArray();
            string[][] genres = documents.Select(document => document.Root!.Elements("genre").Select(element => element.Value).Order().ToArray()).ToArray();

            if (years.Length == 1
                && countries.Select(country => string.Join("|", country)).Distinct().Count() == 1
                && genres.Select(genre => string.Join("|", genre)).Distinct().Count() == 1)
            {
                return;
            }

            string latestMetadata = movieMetadata.OrderByDescending(metadata => new FileInfo(metadata).LastWriteTimeUtc).First();
            if (!PathHelper.GetFileName(latestMetadata).EqualsIgnoreCase(TmdbMetadata.MovieNfoFile))
            {
                Debugger.Break();
            }

            movieMetadata
                .Except([latestMetadata])
                .ToArray()
                .ForEach(metadata => FileHelper.Copy(latestMetadata, metadata, true, true));
        });
    }

    internal static void SyncImdbMetadata(DirectorySettings[] drives, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        (string movie, string imdbId, string json, DateTime LastWriteTimeUtc)[][] groups = drives
            .AsParallel()
            .SelectMany(drive => EnumerateDirectories(drive.Directory, drive.Level).Select(movie =>
            {
                string json = Directory.EnumerateFiles(movie, ImdbMetadataSearchPattern).Single();
                string imdbId = PathHelper.GetFileNameWithoutExtension(json).Split(ImdbMetadata.FileNameSeparator).First();
                return (movie, imdbId, json, new FileInfo(json).LastWriteTimeUtc);
            }))
            .Where(movie => !movie.imdbId.EqualsIgnoreCase(NotExistingFlag))
            .GroupBy(movie => movie.imdbId)
            .Select(group =>
            {
                Debug.Assert(group.Key.IsImdbId());
                return group.OrderByDescending(movie => movie.LastWriteTimeUtc).ToArray();
            })
            .Where(array => array.Length > 1)
            .OrderBy(array => array[0].movie)
            .ToArray();
        log($"{groups.Length} to sync.");
        groups.Select(group=>group.First().movie.EscapeMarkup()).Append(string.Empty).ForEach(log);

        groups.ForEach(array =>
        {
            (string movie, string imdbId, string json, DateTime LastWriteTimeUtc) latest = array[0];
            string[] latestFiles = Directory.GetFiles(latest.movie).Where(file => file.HasAnyExtension(ImdbMetadata.Extension, ImdbCacheExtension)).ToArray();
            array
                .Skip(1)
                .Where(movie => movie.LastWriteTimeUtc < latest.LastWriteTimeUtc)
                .ToArray()
                .ForEach(movie =>
                {
                    log($"Sync {latest.movie.EscapeMarkup()} to {movie.movie.EscapeMarkup()}");
                    string[] files = Directory.GetFiles(movie.movie).Where(file => file.HasAnyExtension(ImdbMetadata.Extension, ImdbCacheExtension)).ToArray();
                    files.ForEach(FileHelper.Recycle);
                    latestFiles.ForEach(file => FileHelper.CopyToDirectory(file, movie.movie, false, true));
                });
        });
    }
}

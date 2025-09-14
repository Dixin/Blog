namespace MediaManager.IO;

using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using Examples.Linq;
using MediaManager.Net;
using Xabe.FFmpeg;

public enum VideoCropMode
{
    NoCrop = 0,
    StrictCrop,
    AdaptiveCropWithLimit,
    AdaptiveCropWithoutLimit
}

public static class FfmpegHelper
{
    internal const string Executable = "ffmpeg";

    internal const string EncoderPostfix = $"{Video.Delimiter}{Executable}";

    private const int DefaultTimestampCount = 5;

    private const int DefaultCropDarknessLimit = 24;

    private static readonly int MaxDegreeOfParallelism = int.Min(3, Environment.ProcessorCount);

    internal static void MergeAllDubbed(
        string directory, string originalVideoSearchPattern = "", Func<string, string>? getDubbedVideo = null,
        Func<string, string>? getOutputVideo = null, Func<string, string>? renameAttachment = null,
        bool overwrite = false, bool? isTV = null, bool isDryRun = false, bool ignoreDurationDifference = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        string[] originalVideos = originalVideoSearchPattern.IsNotNullOrWhiteSpace()
            ? Directory
                .GetFiles(directory, originalVideoSearchPattern, SearchOption.AllDirectories)
            : Directory
                .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(file => file.IsVideo() && !PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase($"{Video.Delimiter}DUBBED{Video.Delimiter}"))
                .ToArray();

        originalVideos.ForEach(originalVideo =>
        {
            string output = getOutputVideo is not null ? getOutputVideo(originalVideo) : string.Empty;
            MergeDubbed(originalVideo, ref output, getDubbedVideo is not null ? getDubbedVideo(originalVideo) : string.Empty, overwrite, isTV, ignoreDurationDifference, isDryRun, log);

            // string originalVideoName = PathHelper.GetFileNameWithoutExtension(originalVideo);
            // Directory
            //     .GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            //     .ForEach(attachment =>
            //     {
            //         string renamedAttachment = renameAttachment is not null
            //             ? renameAttachment(attachment)
            //             : PathHelper.GetFileName(attachment).StartsWithIgnoreCase(originalVideoName)
            //                 ? PathHelper.ReplaceFileNameWithoutExtension(attachment, attachmentName => attachmentName.ReplaceIgnoreCase(originalVideoName, PathHelper.GetFileNameWithoutExtension(output)))
            //                 : attachment;
            //         if (attachment.EqualsIgnoreCase(renamedAttachment))
            //         {
            //             return;
            //         }

            //         log(attachment);
            //         if (!isDryRun)
            //         {
            //             FileHelper.Move(attachment, renamedAttachment);
            //         }
            //         log(renamedAttachment);
            //     });
        });
    }

    internal static async Task EncodeAsync(
        string input, string output = "", string? relativePath = null,
        bool overwrite = false, bool sample = false,
        VideoCropMode videoCropMode = VideoCropMode.NoCrop, int? cropTimestampCount = DefaultTimestampCount, int cropDarknessLimit = DefaultCropDarknessLimit,
        int retryCount = Video.IODefaultRetryCount, Action<string>? log = null, CancellationToken cancellationToken = default, params TimeSpan[] cropTimestamps)
    {
        log ??= Logger.WriteLine;

        if (output.IsNullOrWhiteSpace())
        {
            output = input;
            if (!PathHelper.GetFileNameWithoutExtension(output).ContainsIgnoreCase(EncoderPostfix))
            {
                output = PathHelper.AddFilePostfix(output, EncoderPostfix);
            }

            output = PathHelper.ReplaceExtension(output, Video.VideoExtension);
        }

        if (output.EqualsIgnoreCase(input))
        {
            throw new InvalidOperationException($"Input and output are the same: {input}.");
        }

        if (!overwrite && File.Exists(output))
        {
            log($"Output exists: {output}.");
            return;
        }

        TimeSpan? duration = null;
        if (videoCropMode is not VideoCropMode.NoCrop)
        {
            if (cropTimestamps.Any())
            {
                if (cropTimestampCount.HasValue && cropTimestampCount.Value != cropTimestamps.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(cropTimestampCount), cropTimestampCount, $"The value should be {cropTimestamps.Length}.");
                }
            }
            else
            {
                if (cropTimestampCount is null or <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(cropTimestampCount), cropTimestampCount, "The value should be positive.");
                }

                duration = (await FFmpeg.GetMediaInfo(input, cancellationToken)).Duration;
                cropTimestamps = GetTimestamps(duration.Value, cropTimestampCount.Value).ToArray();
            }
        }

        VideoMetadata videoMetadata = await Video.ReadVideoMetadataAsync(input, null, relativePath, retryCount, cancellationToken);

        string mapAudio = videoMetadata.AudioBitRates.Any() ? "-map 0:a " : string.Empty;

        int bitRate = videoMetadata.PhysicalDefinitionType is DefinitionType.P1080 ? 2048 : 1280;
        List<string> videoFilters = [];
        if (videoMetadata.PhysicalDefinitionType is DefinitionType.P480)
        {
            videoFilters.Add("bwdif=mode=send_field:parity=auto:deint=all");
        }

        if (videoCropMode is not VideoCropMode.NoCrop)
        {
            (int width, int height, int left, int top) = await GetVideoCropAsync(input, videoCropMode: videoCropMode, cropDarknessLimit: cropDarknessLimit, log: log, cancellationToken: cancellationToken, timestamps: cropTimestamps);
            if (width != videoMetadata.VideoWidth || height != videoMetadata.VideoHeight)
            {
                videoFilters.Add($"crop={width}:{height}:{left}:{top}");
            }
        }

        string sampleDuration;
        if (sample)
        {
            duration ??= (await FFmpeg.GetMediaInfo(input, cancellationToken)).Duration;
            TimeSpan sampleStart = duration.Value / 2;
            sampleDuration = $" -ss {sampleStart.Hours:00}:{sampleStart.Minutes:00}:{sampleStart.Seconds:00} -t 00:00:30";
        }
        else
        {
            sampleDuration = string.Empty;
        }

        if (!output.HasExtension(Video.VideoExtension))
        {
            throw new ArgumentOutOfRangeException(nameof(output), output, string.Empty);
        }

        string outputDirectory = PathHelper.GetDirectoryName(output);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string audio = videoMetadata.AudioBitRates.All(bitRate => bitRate > 260_000) ? "aac -ar 48000 -b:a 256k -ac 6" : "copy";
        string videoFilter = videoFilters.Any() ? $" -filter:v {string.Join(",", videoFilters)}" : string.Empty;
        string arguments = $"""
            -hwaccel auto{sampleDuration} -i "{input}" -loglevel verbose -c:v libx265 -profile:v main10 -pix_fmt yuv420p10le -preset slow -x265-params wpp=1:no-pmode=1:no-pme=1:no-psnr=1:no-ssim=1:log-level=info:input-csp=1:interlace=0:total-frames=0:level-idc=0:high-tier=1:uhd-bd=0:ref=4:no-allow-non-conformance=1:no-repeat-headers=1:annexb=1:no-aud=1:no-hrd=1:info=1:hash=0:no-temporal-layers=1:open-gop=1:min-keyint=23:keyint=250:gop-lookahead=0:bframes=4:b-adapt=2:b-pyramid=1:bframe-bias=0:rc-lookahead=25:lookahead-slices=4:scenecut=40:hist-scenecut=0:radl=0:no-splice=1:no-intra-refresh=1:ctu=64:min-cu-size=8:rect=1:no-amp=1:max-tu-size=32:tu-inter-depth=1:tu-intra-depth=1:limit-tu=0:rdoq-level=2:dynamic-rd=0.00:no-ssim-rd=1:signhide=1:no-tskip=1:nr-intra=0:nr-inter=0:no-constrained-intra=1:strong-intra-smoothing=1:max-merge=3:limit-refs=3:limit-modes=1:me=3:subme=3:merange=57:temporal-mvp=1:no-frame-dup=1:no-hme=1:weightp=1:no-weightb=1:no-analyze-src-pics=1:deblock=0\:0:no-sao=1:no-sao-non-deblock=1:rd=4:selective-sao=0:no-early-skip=1:rskip=1:no-fast-intra=1:no-tskip-fast=1:no-cu-lossless=1:no-b-intra=1:no-splitrd-skip=1:rdpenalty=0:psy-rd=2.00:psy-rdoq=1.00:no-rd-refine=1:no-lossless=1:cbqpoffs=0:crqpoffs=0:rc=abr:qcomp=0.60:qpstep=4:stats-write=0:stats-read=2:cplxblur=20.0:qblur=0.5:ipratio=1.40:pbratio=1.30:aq-mode=3:aq-strength=1.00:cutree=1:zone-count=0:no-strict-cbr=1:qg-size=32:no-rc-grain=1:qpmax=69:qpmin=0:no-const-vbv=1:sar=1:overscan=0:videoformat=5:range=0:colorprim=2:transfer=2:colormatrix=2:chromaloc=0:display-window=0:cll=0,0:min-luma=0:max-luma=1023:log2-max-poc-lsb=8:vui-timing-info=1:vui-hrd-info=1:slices=1:no-opt-qp-pps=1:no-opt-ref-list-length-pps=1:no-multi-pass-opt-rps=1:scenecut-bias=0.05:hist-threshold=0.01:no-opt-cu-delta-qp=1:no-aq-motion=1:no-hdr10=1:no-hdr10-opt=1:no-dhdr10-opt=1:no-idr-recovery-sei=1:analysis-reuse-level=0:analysis-save-reuse-level=0:analysis-load-reuse-level=0:scale-factor=0:refine-intra=0:refine-inter=0:refine-mv=1:refine-ctu-distortion=0:no-limit-sao=1:ctu-info=0:no-lowpass-dct=1:refine-analysis-type=0:copy-pic=1:max-ausize-factor=1.0:no-dynamic-refine=1:no-single-sei=1:no-hevc-aq=1:no-svt=1:no-field=1:qp-adaptation-range=1.00:no-scenecut-aware-qpconformance-window-offsets=1:bitrate={bitRate} -b:v {bitRate}k -map 0:v:0 {mapAudio}-map_metadata 0 -c:a {audio}{videoFilter} "{output}" -{(overwrite ? "y" : "n")}
            """;
        log(arguments);
        log(string.Empty);
        await ProcessHelper.StartAndWaitAsync(Executable, arguments, null, null, null, true, cancellationToken);
    }

    internal static bool MergeDubbed(string input, ref string output, string dubbed = "", bool overwrite = false, bool? isTV = null, bool ignoreDurationDifference = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        isTV ??= Video.SeasonEpisodeRegex.IsMatch(PathHelper.GetFileNameWithoutExtension(input));
        if (isTV.Value)
        {
            VideoEpisodeFileInfo inputVideo = VideoEpisodeFileInfo.Parse(input);
            if (dubbed.IsNullOrWhiteSpace())
            {
                dubbed = (inputVideo with { Edition = ".DUBBED" }).Name;
                dubbed = PathHelper.ReplaceFileName(input, dubbed);
            }

            if (output.IsNullOrWhiteSpace())
            {
                output = (inputVideo with { MultipleAudio = ".2Audio" }).Name;
                output = PathHelper.ReplaceFileName(input, output);
            }
        }
        else
        {
            VideoMovieFileInfo inputVideo = VideoMovieFileInfo.Parse(input);
            if (dubbed.IsNullOrWhiteSpace())
            {
                dubbed = (inputVideo with { Edition = ".DUBBED" }).Name;
                dubbed = PathHelper.ReplaceFileName(input, dubbed);
            }

            if (output.IsNullOrWhiteSpace())
            {
                output = (inputVideo with { MultipleAudio = ".2Audio" }).Name;
                output = PathHelper.ReplaceFileName(input, output);
            }
        }

        if (!ignoreDurationDifference)
        {
            TimeSpan difference = Video.ReadVideoMetadataAsync(input).Result.Duration - Video.ReadVideoMetadataAsync(dubbed).Result.Duration;
            if (difference > TimeSpan.FromSeconds(1) || difference < TimeSpan.FromSeconds(-1))
            {
                log($"The difference between the durations of {input} and {dubbed} is {difference}.");
                return false;
            }
        }

        log(input);
        log(dubbed);
        if (CompareDurationAsync(input, dubbed, log).Result != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dubbed), dubbed, string.Empty);
        }

        if (!output.HasExtension(Video.VideoExtension))
        {
            throw new ArgumentOutOfRangeException(nameof(output), output, string.Empty);
        }

        int result = 0;
        if (!isDryRun)
        {
            result = ProcessHelper.StartAndWait(
                Executable,
                $"""
                    -i "{input}" -i "{dubbed}" -c copy -map_metadata 0 -map 0 -map 1:a "{output}" -{(overwrite ? "y" : "n")}
                    """,
                window: true);
        }

        log(output);
        return result == 0;
    }

    private static IEnumerable<TimeSpan> GetTimestamps(TimeSpan duration, int timestampCount = DefaultTimestampCount)
    {
        TimeSpan firstTimestamp = duration / (timestampCount.ThrowIfLessThan(DefaultTimestampCount) + 1);
        return Enumerable.Range(1, timestampCount).Select(index => firstTimestamp * index);
    }

    internal static async Task<(int Width, int Height, int Left, int Top)> GetVideoCropAsync(
        string file, int timestampCount = DefaultTimestampCount, VideoCropMode videoCropMode = VideoCropMode.AdaptiveCropWithLimit, int frameCountPerTimestamp = 30, int cropDarknessLimit = DefaultCropDarknessLimit,
        Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        TimeSpan duration = (await FFmpeg.GetMediaInfo(file, cancellationToken)).Duration;
        return await GetVideoCropAsync(file, timestampCount, videoCropMode, frameCountPerTimestamp, cropDarknessLimit, log, cancellationToken, GetTimestamps(duration, timestampCount).ToArray());
    }

    private static async Task<(int Width, int Height, int Left, int Top)> GetVideoCropAsync(string file, int timestampCount = DefaultTimestampCount, VideoCropMode videoCropMode = VideoCropMode.AdaptiveCropWithLimit, int frameCountPerTimestamp = 30, int cropDarknessLimit = DefaultCropDarknessLimit, Action<string>? log = null, CancellationToken cancellationToken = default, params TimeSpan[] timestamps)
    {
        videoCropMode.ThrowIfEqual(VideoCropMode.NoCrop);
        frameCountPerTimestamp.ThrowIfNotPositive();
        cropDarknessLimit.ThrowIfNotPositive();
        log ??= Logger.WriteLine;

        if (timestamps.IsEmpty())
        {
            TimeSpan duration = (await FFmpeg.GetMediaInfo(file, cancellationToken)).Duration;
            timestamps = GetTimestamps(duration, timestampCount).ToArray();
        }

        (string Width, string Height, string X, string Y)[] crops = timestamps
            .SelectMany(timestamp =>
            {
                string arguments = $"""-ss {timestamp.Hours:00}:{timestamp.Minutes:00}:{timestamp.Seconds:00} -i "{file}" -filter:v cropdetect={cropDarknessLimit}:16:0 -vframes {frameCountPerTimestamp} -f null -max_muxing_queue_size 9999 NUL""";
                (int exitCode, List<string?> output, List<string?> errors) = ProcessHelper.Run(Executable, arguments);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException(file);
                }

                string?[] messages = output.Concat(errors).ToArray();
                (string, string, string, string)[] timestampCrops = messages
                    .Where(message => message.IsNotNullOrWhiteSpace())
                    .Select(message => Regex.Match(message!, " crop=([0-9]+):([0-9]+):([0-9]+):([0-9]+)$"))
                    .Where(match => match.Success)
                    .Select(match => (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value))
                    .ToArray();
                if (timestampCrops.Length <= 1 /* || timestampCrops.Length < frameCountPerTimestamp - 1*/)
                {
                    throw new InvalidOperationException($"""
                         {file}
                         {string.Join(Environment.NewLine, messages)}
                         """);
                }

                (string, string, string, string)[] distinctTimestampCrops = timestampCrops.Distinct().ToArray();
                if (distinctTimestampCrops.Length != 1)
                {
                    log($"""
                        {Executable} {arguments}
                        {string.Join(Environment.NewLine, timestampCrops)}
                        """);
                }

                return timestampCrops;
            })
            .ToArray();
        IGrouping<(int Width, int Height, int X, int Y), (string, string, string, string)>[] distinctCrops = crops
            .GroupBy(crop => (int.Parse(crop.Width), int.Parse(crop.Height), int.Parse(crop.X), int.Parse(crop.Y)))
            .OrderBy(group => group.Count())
            .ToArray();
        if (distinctCrops.Length == 1 || distinctCrops.First().Count() > frameCountPerTimestamp * (timestampCount / 2 + 1))
        {
            return distinctCrops.First().Key;
        }

        if (videoCropMode is VideoCropMode.StrictCrop)
        {
            throw new InvalidOperationException($"""
                 {file}
                 {string.Join(Environment.NewLine, crops)}
                 """);
        }

        (int Width, int Height, int Left, int Top) crop = (
            distinctCrops.Select(group => group.Key.Width).Max(),
            distinctCrops.Select(group => group.Key.Height).Max(),
            distinctCrops.Select(group => group.Key.X).Min(),
            distinctCrops.Select(group => group.Key.Y).Min()
        );

        if (videoCropMode is VideoCropMode.AdaptiveCropWithoutLimit)
        {
            return crop;
        }

        Debug.Assert(videoCropMode is VideoCropMode.AdaptiveCropWithLimit);
        (double Width, double Height, double X, double Y) average = (
            distinctCrops.Select(group => group.Key.Width).Average(),
            distinctCrops.Select(group => group.Key.Height).Average(),
            distinctCrops.Select(group => group.Key.X).Average(),
            distinctCrops.Select(group => group.Key.Y).Average()
        );
        const int MaxDelta = 10;
        if (distinctCrops.Any(group =>
                Math.Abs(average.Width - group.Key.Width) > MaxDelta
                || Math.Abs(average.Width - group.Key.Width) > MaxDelta
                || Math.Abs(average.X - group.Key.X) > MaxDelta
                || Math.Abs(average.Y - group.Key.Y) > MaxDelta))
        {
            throw new InvalidOperationException($"""
                 {file}
                 Average {average}
                 {string.Join(Environment.NewLine, crops)}
                 """);
        }

        return crop;
    }

    internal static async Task EncodeAllAsync(
        string inputDirectory, Func<string, VideoCropMode>? videoCropMode = null, string outputDirectory = "", bool overwrite = false, bool isTV = false,
        Func<string, bool>? inputPredicate = null, Func<string, string>? getOutput = null,
        int? maxDegreeOfParallelism = null, int? cropTimestampCount = DefaultTimestampCount, bool sample = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        maxDegreeOfParallelism ??= MaxDegreeOfParallelism;
        log ??= Logger.WriteLine;

        if (outputDirectory.IsNotNullOrWhiteSpace() && getOutput is not null)
        {
            throw new ArgumentOutOfRangeException(nameof(outputDirectory), outputDirectory, $"{nameof(outputDirectory)} conflicts with {nameof(getOutput)}.");
        }

        inputPredicate ??= file => PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase(EncoderPostfix);
        getOutput ??= outputDirectory.IsNullOrWhiteSpace()
            ? input => PathHelper.ReplaceExtension(input.AddEncoderIfMissing(isTV), Video.VideoExtension)
            : input => PathHelper.ReplaceExtension(input.AddEncoderIfMissing(isTV).ReplaceIgnoreCase(inputDirectory, outputDirectory), Video.VideoExtension);

        List<string> inputVideos = [];
        List<string> skippedInputVideos = [];
        ConcurrentBag<string> outputVideos = [];
        string[] videos = Directory
            .EnumerateFiles(inputDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file =>
            {
                if (!file.IsVideo())
                {
                    return false;
                }

                if (inputPredicate(file))
                {
                    inputVideos.Add(file);
                    return true;
                }

                log($"Skip {file}");
                skippedInputVideos.Add(file);
                return false;
            })
            .Order()
            .ToArray();
        await videos.ParallelForEachAsync(
                async (input, index, token) =>
                {
                    try
                    {
                        string output = getOutput(input);
                        outputVideos.Add(output);
                        await EncodeAsync(input, output, overwrite: overwrite, videoCropMode: videoCropMode?.Invoke(input) ?? VideoCropMode.NoCrop, cropTimestampCount: cropTimestampCount, sample: sample, log: log, cancellationToken: token);
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log(exception.ToString());
                    }
                },
                maxDegreeOfParallelism,
                cancellationToken);

        log($"Should encode {inputVideos.Count} input videos:");
        inputVideos.ForEach(log);

        log($"Skipped {skippedInputVideos.Count} input videos:");
        skippedInputVideos.ForEach(log);

        List<string> existingOutputVideos = [];
        List<string> missingOutputVideos = [];
        log($"Should output {outputVideos.Count} videos");
        outputVideos.ForEach(video =>
        {
            log(video);
            if (File.Exists(video))
            {
                existingOutputVideos.Add(video);
            }
            else
            {
                missingOutputVideos.Add(video);
            }
        });

        log($"Existing {existingOutputVideos.Count} output videos");
        existingOutputVideos.ForEach(log);

        log($"Existing {existingOutputVideos.Distinct(StringComparer.OrdinalIgnoreCase).Count()} distinct output videos");
        existingOutputVideos.GroupBy(video => video).Where(group => group.Count() > 1).ForEach(group => log(group.Key));

        log($"Missing {missingOutputVideos.Count} output videos");
        missingOutputVideos.ForEach(log);
    }

    private static string AddEncoderIfMissing(this string file, bool isTV = false) =>
        PathHelper.GetFileNameWithoutExtension(file).ContainsIgnoreCase(EncoderPostfix)
            ? file
            : isTV
                ? VideoEpisodeFileInfo.TryParse(file, out VideoEpisodeFileInfo? episode)
                    ? PathHelper.ReplaceFileNameWithoutExtension(file, (episode with { Encoder = EncoderPostfix }).Name)
                    : PathHelper.AddFilePostfix(file, EncoderPostfix)
                : VideoMovieFileInfo.TryParse(file, out VideoMovieFileInfo? video)
                    ? PathHelper.ReplaceFileNameWithoutExtension(file, (video with { Encoder = EncoderPostfix }).Name)
                    : PathHelper.AddFilePostfix(file, EncoderPostfix);

    internal static void MergeAllDubbedMovies(string directory, int level = Video.DefaultDirectoryLevel, string recycleDirectory = "", bool ignoreDurationDifference = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        (string OriginalVideo, string DubbedVideo, string MergedVideo)[] movies = Video
            .EnumerateDirectories(directory, level)
            .GroupBy(movie =>
            {
                if (ImdbMetadata.TryRead(movie, out string? imdbId, out _, out _, out _, out _))
                {
                    return imdbId;
                }

                throw new InvalidOperationException($"!IMDB id is missing in {movie}.");
            })
            .Select(group => group.ToArray())
            .Where(group => group.Length == 2)
            .Select(group => (
                Dubbed: group.FirstOrDefault(movie => PathHelper.GetFileName(movie).ContainsIgnoreCase(".DUBBED."), string.Empty),
                Original: group.FirstOrDefault(movie => !PathHelper.GetFileName(movie).ContainsIgnoreCase(".DUBBED."), string.Empty)))
            .Where(match => match.Original.IsNotNullOrWhiteSpace() && match.Dubbed.IsNotNullOrWhiteSpace())
            .Select(match => (
                OriginalVideo: Directory
                    .EnumerateFiles(match.Original, Video.VideoSearchPattern)
                    .SingleOrDefault(video => !PathHelper.GetFileNameWithoutExtension(video).ContainsIgnoreCase(".2Audio"), string.Empty),
                DubbedVideo: Directory
                    .EnumerateFiles(match.Dubbed, Video.VideoSearchPattern)
                    .Single()))
            .Where(match => match.OriginalVideo.IsNotNullOrWhiteSpace())
            .Select(match => (
                match.OriginalVideo,
                match.DubbedVideo,
                MergedVideo: PathHelper.AddFilePostfix(match.OriginalVideo, ".2Audio")))
            .ToArray();

        movies
            .Where(match => !File.Exists(match.MergedVideo))
            .Select(match =>
                (match, Result: MergeDubbed(match.OriginalVideo, ref match.MergedVideo, match.DubbedVideo, false, false, ignoreDurationDifference, isDryRun, log)))
            .Where(result => !result.Result)
            .ForEach(result => log(result.match.ToString()));

        if (isDryRun)
        {
            return;
        }

        Action<string> cleanDirectory = recycleDirectory.IsNullOrWhiteSpace()
            ? DirectoryHelper.Recycle
            : directory => DirectoryHelper.MoveToDirectory(directory, recycleDirectory);

        Action<string> cleanFile = recycleDirectory.IsNullOrWhiteSpace()
            ? FileHelper.Recycle
            : file => FileHelper.MoveToDirectory(file, recycleDirectory);

        movies
            .Where(match => File.Exists(match.MergedVideo))
            .ToArray()
            .ForEach(match =>
            {
                cleanFile(match.OriginalVideo);
                Directory.EnumerateFiles(PathHelper.GetDirectoryName(match.OriginalVideo))
                    .Where(file => !file.EqualsIgnoreCase(match.MergedVideo))
                    .ToArray()
                    .ForEach(file => FileHelper.ReplaceFileNameWithoutExtension(
                        file,
                        name => name.ReplaceIgnoreCase(PathHelper.GetFileNameWithoutExtension(match.OriginalVideo), PathHelper.GetFileNameWithoutExtension(match.MergedVideo))));

                cleanDirectory(PathHelper.GetDirectoryName(match.DubbedVideo));
            });

        movies
            .Where(match => !File.Exists(match.MergedVideo))
            .ToArray()
            .ForEach(match =>
            {
                Directory
                    .EnumerateFiles(PathHelper.GetDirectoryName(match.DubbedVideo))
                    .Where(file => PathHelper.GetFileNameWithoutExtension(file).StartsWithIgnoreCase(PathHelper.GetFileNameWithoutExtension(match.DubbedVideo)))
                    .ToArray()
                    .ForEach(file => FileHelper.MoveToDirectory(file, PathHelper.GetDirectoryName(match.OriginalVideo)));

                cleanDirectory(PathHelper.GetDirectoryName(match.DubbedVideo));
            });
    }

    internal static async Task<int> ExtractAndCompareAsync(ISettings settings, string inputVideo, string mergeAudio = "", string outputVideo = "", bool isSubtitleOnly = false, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;
        Debug.Assert(inputVideo.EndsWithIgnoreCase(".mkv"));

        if (outputVideo.IsNullOrWhiteSpace())
        {
            outputVideo = PathHelper.ReplaceExtension(inputVideo, ".mp4");
        }

        if (!outputVideo.HasExtension(Video.VideoExtension))
        {
            throw new ArgumentOutOfRangeException(nameof(outputVideo), outputVideo, string.Empty);
        }

        if (File.Exists(outputVideo))
        {
            return 0;
        }

        IMediaInfo inputMediaInfo = await FFmpeg.GetMediaInfo(inputVideo, cancellationToken);
        (int Index, int SubtitleIndex, string Title, string File)[] subtitles = inputMediaInfo
            .SubtitleStreams
            .GroupBy(subtitle => subtitle.Language)
            .Select(subtitleGroup => subtitleGroup.OrderBy(subtitle => subtitle.Index).ToArray())
            .Do(subtitleArray =>
            {
                if (inputVideo.Contains($"{Video.VersionSeparator}{settings.TopEnglishKeyword}"))
                {
                    Debug.Assert(subtitleArray.Count(subtitle => Regex.IsMatch(subtitle.Title, @"\bForced\b", RegexOptions.IgnoreCase)) <= 1);
                    Debug.Assert(subtitleArray.Count(subtitle => Regex.IsMatch(subtitle.Title, @"\bSDH\b", RegexOptions.IgnoreCase)) <= 1);
                    Debug.Assert(subtitleArray.Count(subtitle => !Regex.IsMatch(subtitle.Title, @"\bForced\b", RegexOptions.IgnoreCase) && !Regex.IsMatch(subtitle.Title, @"\bSDH\b", RegexOptions.IgnoreCase)) <= 1);
                }
            })
            .Concat()
            .ToArray()
            .Select((subtitle, index) => (
                subtitle.Index,
                SubtitleIndex: index,
                Title: GetSubtitleTitle(subtitle.Title),
                File: PathHelper.ReplaceExtension(PathHelper.AddFilePostfix(outputVideo, $".{subtitle.Language}{GetSubtitleTitle(subtitle.Title)}"), GetSubtitleExtension(subtitle.Codec))))
            .OrderBy(subtitle => subtitle.Index)
            .Do(subtitle => log(subtitle.File))
            .ToArray();

        IGrouping<string, (int Index, int SubtitleIndex, string Title, string File)>[] duplicateSubtitles = subtitles
            .GroupBy(subtitle => subtitle.File, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .ToArray();

        if (duplicateSubtitles.Any())
        {
            duplicateSubtitles.ForEach(group => group.Select(subtitle => $"{subtitle.Index} {subtitle.File}").Append(string.Empty).ForEach(log));
            HashSet<int> duplicateSubtitleIndexes = [.. duplicateSubtitles.Concat().Select(subtitle => subtitle.Index)];
            Enumerable
                .Range(0, subtitles.Length)
                .Where(index => duplicateSubtitleIndexes.Contains(subtitles[index].Index))
                .ForEach(index => subtitles[index] = subtitles[index] with
                {
                    File = PathHelper.AddFilePostfix(
                        subtitles[index].File,
                        $"{(subtitles[index].Title.IsNullOrWhiteSpace() ? "-" : "_")}{subtitles[index].SubtitleIndex}")
                });
            Debug.Assert(subtitles
                .GroupBy(subtitle => subtitle.File, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .IsEmpty());
        }

        if (mergeAudio.IsNotNullOrWhiteSpace())
        {
            IMediaInfo mergeMediaInfo = await FFmpeg.GetMediaInfo(mergeAudio, cancellationToken);
            TimeSpan inputDuration = inputMediaInfo.Duration;
            TimeSpan mergeDuration = mergeMediaInfo.Duration;
            if (CompareDuration(inputDuration, mergeDuration) != 0)
            {
                log($"{inputDuration} {inputVideo}");
                log($"{mergeDuration} {mergeAudio}");
                log(string.Empty);
                throw new ArgumentOutOfRangeException(nameof(mergeAudio), mergeAudio, string.Empty);
            }
        }

        string strict = inputMediaInfo.AudioStreams.Any(audio => audio.Codec.ContainsIgnoreCase("TrueHD")) ? "-strict -2" : "";
        string subtitleArguments = string.Join(
            " ",
            subtitles.Where(subtitle => !subtitle.File.HasExtension(".idx")).Select(subtitle => $"""
                -c copy -map 0:{subtitle.Index} "{subtitle.File}"
                """));
        string arguments = isSubtitleOnly
            ? $"""
            -i "{inputVideo}" {subtitleArguments} -n
            """
            : mergeAudio.IsNullOrWhiteSpace()
                ? $"""
                -i "{inputVideo}" -c copy -map_metadata 0 -map 0:v -map 0:a {strict} "{outputVideo}" {subtitleArguments} -n
                """
                : $"""
                -i "{inputVideo}" -i "{mergeAudio}" -c copy -map_metadata 0 -map 0:v -map 0:a -map 1:a {strict} "{outputVideo}" {subtitleArguments} -n
                """;
        log(arguments);
        log(string.Empty);

        int compare = 0;
        if (!isDryRun)
        {
            if (!isSubtitleOnly && File.Exists(outputVideo))
            {
                compare = await CompareDurationAsync(inputVideo, outputVideo);
                return compare;
            }

            string outputDirectory = PathHelper.GetDirectoryName(outputVideo);
            if (DriveHelper.TryGetAvailableFreeSpace(outputVideo, out long? availableFreeSpace) && new FileInfo(inputVideo).Length >= availableFreeSpace * 1.2)
            {
                log($"!!! {outputDirectory} does not has enough available free space for {inputVideo}.");
                compare = 1;
                return compare;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            int result = await ProcessHelper.StartAndWaitAsync(Executable, arguments, null, null, null, true, cancellationToken);
            if (result != 0)
            {
                compare = 1;
                return compare;
            }

            if (isSubtitleOnly)
            {
                compare = 0;
                return compare;
            }

            compare = await CompareDurationAsync(inputVideo, outputVideo);
            if (compare != 0)
            {
                log($"!!! ffmpeg failed {outputVideo}");
                return compare;
            }
        }

        (int Index, int SubtitleIndex, string Title, string File)[] dvdSubtitles = subtitles.Where(subtitle => subtitle.File.HasExtension(".idx")).ToArray();
        if (dvdSubtitles.Any())
        {
            subtitleArguments = string.Join(" ",
                dvdSubtitles.Select(subtitle => $"""
                    "{subtitle.Index}:{subtitle.File}"
                    """));
            arguments = $"""
                "{inputVideo}" tracks {subtitleArguments}
                """;
            log(arguments);
            if (!isDryRun)
            {
                int result = await ProcessHelper.StartAndWaitAsync("""
                    "C:\Program Files\MKVToolNix\mkvextract.exe"
                    """, arguments, null, null, null, true, cancellationToken);
                if (result != 0)
                {
                    log($"!!! mkvextract failed {outputVideo}");
                    compare = 1;
                }

                return compare;
            }
        }

        return compare;

        static string GetSubtitleExtension(string codec) => codec.ToUpperInvariant() switch
        {
            "SUBRIP" => ".srt",
            "HDMV_PGS_SUBTITLE" => ".sup",
            "ASS" => ".ass",
            "DVD_SUBTITLE" => ".idx",
            _ => throw new InvalidOperationException(codec)
        };

        static string GetSubtitleTitle(string? title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            string[] words = Regex.Matches(title, @"\b\w+\b").Select(match => match.Value.Trim()).ToArray();
            return words.ContainsIgnoreCase("Forced")
                ? "-forced"
                : words.ContainsIgnoreCase("SDH")
                    ? "-sdh"
                    : $"-{title.Replace(" ", "_").Replace(@"\", "_").Replace("/", "_").Replace("-", "_").Replace(".", "_")}";
        }
    }

    internal static async Task ExtractAllAsync(ISettings settings, string inputDirectory, Func<string, bool>? predicate = null, bool isTV = false, bool skipParsing = false, bool isDryRun = false, Action<string>? log = null, CancellationToken cancellationToken = default, params Func<string, string>[] outputVideos)
    {
        log ??= Logger.WriteLine;

        ConcurrentQueue<(string Input, string Dubbed)> tasks;
        if (skipParsing)
        {
            tasks = new(Directory
                .EnumerateFiles(inputDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(file => file.IsVideo() && (predicate is null || predicate(file)))
                .Select(video => (video, string.Empty)));
        }
        else
        {
            IEnumerable<(string Input, string Dubbed)> videoPairs = Directory
                .EnumerateFiles(inputDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(file => file.IsVideo() && (predicate is null || predicate(file)))
                .Select(video =>
                {
                    if (isTV)
                    {
                        VideoEpisodeFileInfo episode = VideoEpisodeFileInfo.Parse(video);
                        return (File: video, Title: episode.TVTitle, Year: string.Empty, Edition: episode.Edition);
                    }

                    VideoMovieFileInfo movie = VideoMovieFileInfo.Parse(video);
                    return (File: video, movie.Title, movie.Year, movie.Edition);
                })
                .GroupBy(video => $"{video.Title}{Video.Delimiter}{video.Year}", StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    if (group.Count() == 1)
                    {
                        return [(Input: group.Single().File, Dubbed: string.Empty)];
                    }

                    if (group.Count() == 2)
                    {
                        (string File, string Title, string Year, string Edition)[] input = group.Where(video => !video.Edition.ContainsIgnoreCase(".DUBBED")).ToArray();
                        (string File, string Title, string Year, string Edition)[] dubbed = group.Where(video => video.Edition.ContainsIgnoreCase(".DUBBED")).ToArray();
                        if (input.Any() && dubbed.Any())
                        {
                            return [(Input: input.Single().File, Dubbed: dubbed.Single().File)];
                        }
                    }

                    return group.Select(video => (Input: video.File, Dubbed: string.Empty));
                })
                .Concat();
            tasks = new(videoPairs);
        }

        Lock destinationCheckLock = new();
        await Parallel.ForEachAsync(
            outputVideos,
            cancellationToken,
            async (output, cancellation) =>
            {
                while (tasks.TryDequeue(out (string Input, string Dubbed) task))
                {
                    (string inputVideo, string dubbedVideo) = task;
                    string outputVideo = output(task.Input);
                    lock (destinationCheckLock)
                    {
                        string[] destinationVideos = outputVideos.Select(outputVideo => outputVideo(inputVideo)).Where(File.Exists).ToArray();
                        if (destinationVideos.Any())
                        {
                            destinationVideos.Append(string.Empty).ForEach(log);
                            continue;
                        }

                        if (DriveHelper.TryGetAvailableFreeSpace(inputVideo, out long? availableFreeSpace) && new FileInfo(inputVideo).Length >= availableFreeSpace * 1.2)
                        {
                            log($"!!! Output drive does not have enough available free space for {outputVideo}.");
                            tasks.Enqueue(task);
                            break;
                        }
                    }

                    int result = await ExtractAndCompareAsync(settings, inputVideo, dubbedVideo, outputVideo, false, isDryRun, log, cancellation);
                    log($"{result} {task}");
                    log("");
                }
            });
    }

    internal static async Task<int> CompareDurationAsync(string video1, string video2, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        TimeSpan duration1 = (await FFmpeg.GetMediaInfo(video1)).Duration;
        TimeSpan duration2 = (await FFmpeg.GetMediaInfo(video2)).Duration;
        int result = CompareDuration(duration1, duration2);
        if (result != 0)
        {
            log($"{duration1} {video1}");
            log($"{duration2} {video2}");
            log(string.Empty);
        }

        return result;
    }

    private static int CompareDuration(TimeSpan duration1, TimeSpan duration2)
    {
        TimeSpan difference = duration1 - duration2;
        return difference >= TimeSpan.FromSeconds(-1) && difference <= TimeSpan.FromSeconds(1) ? 0 : duration1.CompareTo(duration2);
    }
}

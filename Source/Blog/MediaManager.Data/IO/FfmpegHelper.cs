namespace MediaManager.IO;

using Examples.Common;
using Examples.Diagnostics;
using Examples.IO;
using Examples.Linq;
using MediaManager.Net;
using Xabe.FFmpeg;

public enum FfmpegVideoCrop
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

    private const int DefaultTimestampCount = 3;

    private static readonly int MaxDegreeOfParallelism = int.Min(3, Environment.ProcessorCount);

    internal static void MergeAllDubbed(string directory, string originalVideoSearchPattern = "", Func<string, string>? getDubbedVideo = null, Func<string, string>? getOutputVideo = null, Func<string, string>? renameAttachment = null, bool overwrite = false, bool? isTV = null, bool isDryRun = false, bool ignoreDurationDifference = false, Action<string>? log = null)
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

            string originalVideoName = PathHelper.GetFileNameWithoutExtension(originalVideo);
            Directory
                .GetFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .ForEach(attachment =>
                {
                    string renamedAttachment = renameAttachment is not null
                        ? renameAttachment(attachment)
                        : PathHelper.GetFileName(attachment).StartsWithIgnoreCase(originalVideoName)
                            ? PathHelper.ReplaceFileNameWithoutExtension(attachment, attachmentName => attachmentName.ReplaceIgnoreCase(originalVideoName, PathHelper.GetFileNameWithoutExtension(output)))
                            : attachment;
                    if (attachment.EqualsIgnoreCase(renamedAttachment))
                    {
                        return;
                    }

                    log(attachment);
                    if (!isDryRun)
                    {
                        FileHelper.Move(attachment, renamedAttachment);
                    }
                    log(renamedAttachment);
                });
        });
    }

    internal static async Task EncodeAsync(
        string input, string output = "",
        bool overwrite = false, FfmpegVideoCrop videoCrop = FfmpegVideoCrop.NoCrop, bool sample = false,
        string? relativePath = null, int retryCount = Video.IODefaultRetryCount, int? cropTimestampCount = DefaultTimestampCount, Action<string>? log = null, CancellationToken cancellationToken = default, params TimeSpan[] cropTimestamps)
    {
        log ??= Logger.WriteLine;

        TimeSpan? duration = null;
        if (videoCrop is not FfmpegVideoCrop.NoCrop)
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

        string mapAudio = videoMetadata.Audio > 0 ? "-map 0:a " : string.Empty;

        int bitRate = videoMetadata.DefinitionType is DefinitionType.P1080 ? 2048 : 1280;
        List<string> videoFilters = [];
        if (videoMetadata.DefinitionType is DefinitionType.P480)
        {
            videoFilters.Add("bwdif=mode=send_field:parity=auto:deint=all");
        }

        if (videoCrop is not FfmpegVideoCrop.NoCrop)
        {
            (int width, int height, int x, int y) = await GetVideoCropAsync(input, videoCrop, log: log, cancellationToken: cancellationToken, timestamps: cropTimestamps);
            if (width != videoMetadata.Width || height != videoMetadata.Height)
            {
                videoFilters.Add($"crop={width}:{height}:{x}:{y}");
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

        string outputDirectory = PathHelper.GetDirectoryName(output);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string audio = videoMetadata.AudioBitRates.All(audioBitRate => audioBitRate > 260_000) ? "aac -ar 48000 -b:a 256k -ac 6" : "copy";
        string videoFilter = videoFilters.Any() ? $" -filter:v {string.Join(",", videoFilters)}" : string.Empty;
        string arguments = $"""
            -hwaccel auto{sampleDuration} -i "{input}" -loglevel verbose -c:v libx265 -profile:v main10 -pix_fmt yuv420p10le -preset slow -x265-params wpp=1:no-pmode=1:no-pe=1:no-psnr=1:no-ssim=1:log-level=info:input-csp=1:interlace=0:total-frames=0:level-idc=0:high-tier=1:uhd-bd=0:ref=4:no-allow-non-conformance=1:no-repeat-headers=1:annexb=1:no-aud=1:no-hrd=1:info=1:hash=0:no-temporal-layers=1:open-gop=1:min-keyint=23:keyint=250:gop-lookahead=0:bframes=4:b-adapt=2:b-pyramid=1:bframe-bias=0:rc-lookahead=25:lookahead-slices=4:scenecut=40:hist-scenecut=0:radl=0:no-splice=1:no-intra-refresh=1:ctu=64:min-cu-size=8:rect=1:no-amp=1:max-tu-size=32:tu-inter-depth=1:tu-intra-depth=1:limit-tu=0:rdoq-level=2:dynamic-rd=0.00:no-ssim-rd=1:signhide=1:no-tskip=1:nr-intra=0:nr-inter=0:no-constrained-intra=1:strong-intra-smoothing=1:max-merge=3:limit-refs=3:limit-modes=1:me=3:subme=3:merange=57:temporal-mvp=1:no-frame-dup=1:no-hme=1:weightp=1:no-weightb=1:no-analyze-src-pics=1:deblock=0\:0:no-sao=1:no-sao-non-deblock=1:rd=4:selective-sao=0:no-early-skip=1:rskip=1:no-fast-intra=1:no-tskip-fast=1:no-cu-lossless=1:no-b-intra=1:no-splitrd-skip=1:rdpenalty=0:psy-rd=2.00:psy-rdoq=1.00:no-rd-refine=1:no-lossless=1:cbqpoffs=0:crqpoffs=0:rc=abr:qcomp=0.60:qpstep=4:stats-write=0:stats-read=2:cplxblur=20.0:qblur=0.5:ipratio=1.40:pbratio=1.30:aq-mode=3:aq-strength=1.00:cutree=1:zone-count=0:no-strict-cbr=1:qg-size=32:no-rc-grain=1:qpmax=69:qpmin=0:no-const-vbv=1:sar=1:overscan=0:videoformat=5:range=0:colorprim=2:transfer=2:colormatrix=2:chromaloc=0:display-window=0:cll=0,0:min-luma=0:max-luma=1023:log2-max-poc-lsb=8:vui-timing-info=1:vui-hrd-info=1:slices=1:no-opt-qp-pps=1:no-opt-ref-list-length-pps=1:no-multi-pass-opt-rps=1:scenecut-bias=0.05:hist-threshold=0.01:no-opt-cu-delta-qp=1:no-aq-motion=1:no-hdr10=1:no-hdr10-opt=1:no-dhdr10-opt=1:no-idr-recovery-sei=1:analysis-reuse-level=0:analysis-save-reuse-level=0:analysis-load-reuse-level=0:scale-factor=0:refine-intra=0:refine-inter=0:refine-mv=1:refine-ctu-distortion=0:no-limit-sao=1:ctu-info=0:no-lowpass-dct=1:refine-analysis-type=0:copy-pic=1:max-ausize-factor=1.0:no-dynamic-refine=1:no-single-sei=1:no-hevc-aq=1:no-svt=1:no-field=1:qp-adaptation-range=1.00:no-scenecut-aware-qpconformance-window-offsets=1:bitrate={bitRate} -b:v {bitRate}k -map 0:v:0 {mapAudio}-map_metadata 0 -c:a {audio}{videoFilter} "{output}" -{(overwrite ? "y" : "n")}
            """;
        log(arguments);
        log(string.Empty);
        await ProcessHelper.StartAndWaitAsync(Executable, arguments, null, null, null, true, cancellationToken);
    }

    internal static bool MergeDubbed(string input, ref string output, string dubbed = "", bool overwrite = false, bool? isTV = null, bool ignoreDurationDifference = false, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;
        isTV ??= Regex.IsMatch(PathHelper.GetFileNameWithoutExtension(input), @"(\.|\s)S[0-9]{2}E[0-9]{2}(\.|\s)", RegexOptions.IgnoreCase);
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

    internal static async Task<(int Width, int Height, int X, int Y)> GetVideoCropAsync(
        string file, FfmpegVideoCrop videoCrop = FfmpegVideoCrop.AdaptiveCropWithLimit, int timestampCount = DefaultTimestampCount, int frameCountPerTimestamp = 30, Action<string>? log = null, CancellationToken cancellationToken = default)
    {
        log ??= Logger.WriteLine;

        TimeSpan duration = (await FFmpeg.GetMediaInfo(file, cancellationToken)).Duration;
        return await GetVideoCropAsync(file, videoCrop, timestampCount, frameCountPerTimestamp, log, cancellationToken, GetTimestamps(duration, timestampCount).ToArray());
    }

    private static async Task<(int Width, int Height, int X, int Y)> GetVideoCropAsync(
        string file, FfmpegVideoCrop videoCrop = FfmpegVideoCrop.AdaptiveCropWithLimit, int timestampCount = DefaultTimestampCount, int frameCountPerTimestamp = 30, Action<string>? log = null, CancellationToken cancellationToken = default, params TimeSpan[] timestamps)
    {
        videoCrop.ThrowIfEqual(FfmpegVideoCrop.NoCrop);
        log ??= Logger.WriteLine;

        if (timestamps.IsEmpty())
        {
            TimeSpan duration = (await FFmpeg.GetMediaInfo(file, cancellationToken)).Duration;
            timestamps = GetTimestamps(duration, timestampCount).ToArray();
        }

        (string Width, string Height, string X, string Y)[] crops = timestamps
            .SelectMany(timestamp =>
            {
                string arguments = $"""-ss {timestamp.Hours:00}:{timestamp.Minutes:00}:{timestamp.Seconds:00} -i "{file}" -filter:v cropdetect -vframes {frameCountPerTimestamp} -f null -max_muxing_queue_size 9999 NUL""";
                (int exitCode, List<string?> output, List<string?> errors) = ProcessHelper.Run(Executable, arguments);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException(file);
                }

                (string, string, string, string)[] timestampCrops = output
                    .Concat(errors)
                    .Where(message => message.IsNotNullOrWhiteSpace())
                    .Select(message => Regex.Match(message!, @" crop=([0-9]+):([0-9]+):([0-9]+):([0-9]+)$"))
                    .Where(match => match.Success)
                    .Select(match => (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value))
                    .ToArray();
                Debug.Assert(timestampCrops.Length > 1/* && timestampCrops.Length >= frameCountPerTimestamp - 1*/);
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

        if (videoCrop is FfmpegVideoCrop.StrictCrop)
        {
            throw new InvalidOperationException($"""
                 {file}
                 {string.Join(Environment.NewLine, crops)}
                 """);
        }

        (int Width, int Height, int X, int Y) crop = (
            distinctCrops.Select(group => group.Key.Width).Max(),
            distinctCrops.Select(group => group.Key.Height).Max(),
            distinctCrops.Select(group => group.Key.X).Min(),
            distinctCrops.Select(group => group.Key.Y).Min()
        );

        if (videoCrop is FfmpegVideoCrop.AdaptiveCropWithoutLimit)
        {
            return crop;
        }

        Debug.Assert(videoCrop is FfmpegVideoCrop.AdaptiveCropWithLimit);
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
        string inputDirectory, string outputDirectory = "", FfmpegVideoCrop videoCrop = FfmpegVideoCrop.NoCrop, bool overwrite = false, bool isTV = false,
        Func<string, bool>? inputPredicate = null, Func<string, string>? getOutput = null,
        int? maxDegreeOfParallelism = null, Action<string>? log = null, CancellationToken cancellationToken = default)
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

        await Directory
            .EnumerateFiles(inputDirectory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
            .Where(file => file.IsVideo() && inputPredicate(file))
            .Order()
            .ToArray()
            .ParallelForEachAsync(
                async (input, index, token) =>
                {
                    try
                    {
                        await EncodeAsync(input, getOutput(input), overwrite, videoCrop, log: log, cancellationToken: token);
                    }
                    catch (Exception exception) when (exception.IsNotCritical())
                    {
                        log(exception.ToString());
                    }
                },
                maxDegreeOfParallelism,
                cancellationToken);
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

    internal static void MergeDubbedMovies(string directory, int level = Video.DefaultDirectoryLevel, bool isDryRun = false, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Video
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
                OriginalVideo: Directory.EnumerateFiles(match.Original, Video.VideoSearchPattern).Single(),
                DubbedVideo: Directory.EnumerateFiles(match.Dubbed, Video.VideoSearchPattern).Single()))
            .Select(match => (
                match.OriginalVideo,
                match.DubbedVideo,
                MergedVideo: PathHelper.AddFilePostfix(match.OriginalVideo, ".2Audio")))
            .ToArray()
            .Select(match =>
                (match, Result: MergeDubbed(match.OriginalVideo, ref match.MergedVideo, match.DubbedVideo, false, false, false, isDryRun, log)))
            .Where(result => !result.Result)
            .ForEach(result => log(result.match.ToString()));
    }
}
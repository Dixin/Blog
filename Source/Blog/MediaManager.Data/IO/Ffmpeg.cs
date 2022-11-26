namespace Examples.IO;

using Examples.Common;
using Examples.Diagnostics;
using Xabe.FFmpeg;

internal class FfmpegHelper
{
    internal static void MergeAllDubbed(string directory, string originalVideoSearchPattern = "", Func<string, string>? getDubbedVideo = null, Func<string, string>? renameSubtitle = null, bool overwrite = false, bool? isTV = null, bool isDryRun = false)
    {
        string[] originalVideos = originalVideoSearchPattern.IsNotNullOrWhiteSpace()
            ? Directory
                .GetFiles(directory, originalVideoSearchPattern, SearchOption.AllDirectories)
            : Directory
                .EnumerateFiles(directory, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                .Where(file => file.IsVideo() && !Path.GetFileNameWithoutExtension(file).ContainsIgnoreCase(".DUBBED."))
                .ToArray();
        originalVideos.ForEach(originalVideo =>
        {
            string output = string.Empty;
            MergeDubbed(originalVideo, ref output, getDubbedVideo is not null ? getDubbedVideo(originalVideo) : string.Empty, overwrite, isTV);
            Directory
                .EnumerateFiles(Path.GetDirectoryName(originalVideo) ?? throw new InvalidOperationException(originalVideo))
                .Where(file => file.IsSubtitle() && Path.GetFileName(file).StartsWithIgnoreCase(Path.GetFileNameWithoutExtension(originalVideo)))
                .ToArray()
                .ForEach(subtitle =>
                {
                    string renamedSubtitle = PathHelper.ReplaceFileNameWithoutExtension(subtitle, subtitleName => subtitleName.ReplaceIgnoreCase(Path.GetFileNameWithoutExtension(originalVideo), Path.GetFileNameWithoutExtension(output)));
                    FileHelper.Move(subtitle, renamedSubtitle);
                });
        });
    }

    internal static void Encode(string input, string output = "", bool overwrite = false, bool? estimateCrop = null, bool sample = false, string? relativePath = null, int retryCount = 10)
    {
        VideoMetadata videoMetadata = Video.GetVideoMetadataAsync(input, null, relativePath, retryCount).Result;

        int bitRate = videoMetadata.Is1080P ? 2048 : 1280;
        List<string> videoFilters = new();
        if (!videoMetadata.Is1080P && !videoMetadata.Is720P)
        {
            videoFilters.Add("bwdif=mode=send_field:parity=auto:deint=all");
        }

        TimeSpan? duration = null;
        if (estimateCrop.HasValue)
        {
            duration = FFmpeg.GetMediaInfo(input).Result.Duration;
            (int width, int height, int x, int y) = GetVideoCrop(input, duration.Value, estimate: estimateCrop.Value);
            if (width != videoMetadata.Width || height != videoMetadata.Height)
            {
                videoFilters.Add($"crop={width}:{height}:{x}:{y}");
            }
        }

        string sampleDuration;
        if (sample)
        {
            duration ??= FFmpeg.GetMediaInfo(input).Result.Duration;
            TimeSpan sampleStart = duration.Value / 2;
            sampleDuration = $" -ss {sampleStart.Hours:00}:{sampleStart.Minutes:00}:{sampleStart.Seconds:00} -t 00:00:30";
        }
        else
        {
            sampleDuration = string.Empty;
        }


        if (output.IsNullOrWhiteSpace())
        {
            output = PathHelper.AddFilePostfix(input, ".ffmpeg");
            output = PathHelper.ReplaceExtension(output, Video.VideoExtension);
        }

        string audio = videoMetadata.AudioBitRates.All(audioBitRate => audioBitRate > 260_000) ? "aac -ar 48000 -b:a 256k -ac" : "copy";
        string videoFilter = videoFilters.Any() ? $" -filter:v {string.Join(",", videoFilters)}" : string.Empty;

        ProcessHelper.StartAndWait(
            "ffmpeg",
            $"""
                -hwaccel auto{sampleDuration} -i "{input}" -loglevel verbose -c:v libx265 -profile:v main10 -pix_fmt yuv420p10le -preset slow -x265-params wpp=1:no-pmode=1:no-pe=1:no-psnr=1:no-ssim=1:log-level=info:input-csp=1:interlace=0:total-frames=0:level-idc=0:high-tier=1:uhd-bd=0:ref=4:no-allow-non-conformance=1:no-repeat-headers=1:annexb=1:no-aud=1:no-hrd=1:info=1:hash=0:no-temporal-layers=1:open-gop=1:min-keyint=23:keyint=250:gop-lookahead=0:bframes=4:b-adapt=2:b-pyramid=1:bframe-bias=0:rc-lookahead=25:lookahead-slices=4:scenecut=40:hist-scenecut=0:radl=0:no-splice=1:no-intra-refresh=1:ctu=64:min-cu-size=8:rect=1:no-amp=1:max-tu-size=32:tu-inter-depth=1:tu-intra-depth=1:limit-tu=0:rdoq-level=2:dynamic-rd=0.00:no-ssim-rd=1:signhide=1:no-tskip=1:nr-intra=0:nr-inter=0:no-constrained-intra=1:strong-intra-smoothing=1:max-merge=3:limit-refs=3:limit-modes=1:me=3:subme=3:merange=57:temporal-mvp=1:no-frame-dup=1:no-hme=1:weightp=1:no-weightb=1:no-analyze-src-pics=1:deblock=0\:0:no-sao=1:no-sao-non-deblock=1:rd=4:selective-sao=0:no-early-skip=1:rskip=1:no-fast-intra=1:no-tskip-fast=1:no-cu-lossless=1:no-b-intra=1:no-splitrd-skip=1:rdpenalty=0:psy-rd=2.00:psy-rdoq=1.00:no-rd-refine=1:no-lossless=1:cbqpoffs=0:crqpoffs=0:rc=abr:qcomp=0.60:qpstep=4:stats-write=0:stats-read=2:cplxblur=20.0:qblur=0.5:ipratio=1.40:pbratio=1.30:aq-mode=3:aq-strength=1.00:cutree=1:zone-count=0:no-strict-cbr=1:qg-size=32:no-rc-grain=1:qpmax=69:qpmin=0:no-const-vbv=1:sar=1:overscan=0:videoformat=5:range=0:colorprim=2:transfer=2:colormatrix=2:chromaloc=0:display-window=0:cll=0,0:min-luma=0:max-luma=1023:log2-max-poc-lsb=8:vui-timing-info=1:vui-hrd-info=1:slices=1:no-opt-qp-pps=1:no-opt-ref-list-length-pps=1:no-multi-pass-opt-rps=1:scenecut-bias=0.05:hist-threshold=0.01:no-opt-cu-delta-qp=1:no-aq-motion=1:no-hdr10=1:no-hdr10-opt=1:no-dhdr10-opt=1:no-idr-recovery-sei=1:analysis-reuse-level=0:analysis-save-reuse-level=0:analysis-load-reuse-level=0:scale-factor=0:refine-intra=0:refine-inter=0:refine-mv=1:refine-ctu-distortion=0:no-limit-sao=1:ctu-info=0:no-lowpass-dct=1:refine-analysis-type=0:copy-pic=1:max-ausize-factor=1.0:no-dynamic-refine=1:no-single-sei=1:no-hevc-aq=1:no-svt=1:no-field=1:qp-adaptation-range=1.00:no-scenecut-aware-qpconformance-window-offsets=1:bitrate={bitRate} -b:v {bitRate}k -map 0:v:0 -map 0:a -map_metadata 0 -c:a {audio}{videoFilter} "{output}" -{(overwrite ? "y" : "n")}
                """,
            window: true);
    }

    internal static void MergeDubbed(string input, ref string output, string dubbed = "", bool overwrite = false, bool? isTV = null)
    {
        isTV ??= Regex.IsMatch(Path.GetFileNameWithoutExtension(input), @"(\.|\s)S[0-9]{2}E[0-9]{2}(\.|\s)", RegexOptions.IgnoreCase);
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

        ProcessHelper.StartAndWait(
            "ffmpeg",
            $"""
                -i "{input}" -i "{dubbed}" -c copy -map_metadata 0 -map 0 -map 1:a "{output}" -{(overwrite ? "y" : "n")}
                """,
            window: true);
    }

    internal static (int Width, int Height, int X, int Y) GetVideoCrop(string file, int frameCount = 30, bool estimate = false, Action<string>? log = null) =>
        GetVideoCrop(file, FFmpeg.GetMediaInfo(file).Result.Duration, frameCount, estimate, log);

    internal static (int Width, int Height, int X, int Y) GetVideoCrop(string file, TimeSpan duration, int frameCount = 30, bool estimate = false, Action<string>? log = null)
    {
        TimeSpan[] timestamps = { duration / 4, duration / 2, duration - duration / 4 };
        (string Width, string Height, string X, string Y)[] crops = timestamps
            .Select(timestamp =>
            {
                string arguments = $"""-ss {timestamp.Hours:00}:{timestamp.Minutes:00}:{timestamp.Seconds:00} -i "{file}" -filter:v cropdetect -vframes {frameCount} -f null -max_muxing_queue_size 9999 NUL""";
                (int exitCode, List<string?> output, List<string?> errors) = ProcessHelper.Run("ffmpeg", arguments);
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
                Debug.Assert(timestampCrops.Length == frameCount - 1);
                (string, string, string, string)[] distinctTimestampCrops = timestampCrops.Distinct().ToArray();
                if (distinctTimestampCrops.Length != 1)
                {
                    log?.Invoke($"""
                        ffmpeg {arguments}
                        {string.Join(Environment.NewLine, timestampCrops)}
                        """);
                }

                return timestampCrops;
            })
            .Concat()
            .ToArray();
        IGrouping<(int Width, int Height, int X, int Y), (string, string, string, string)>[] distinctCrops = crops
            .GroupBy(crop => (int.Parse(crop.Width), int.Parse(crop.Height), int.Parse(crop.X), int.Parse(crop.Y)))
            .OrderBy(group => group.Count())
            .ToArray();
        if (distinctCrops.Length == 1 || distinctCrops.First().Count() >= frameCount * 2)
        {
            return distinctCrops.First().Key;
        }

        if (estimate)
        {
            return (
                distinctCrops.Select(group => group.Key.Width).Max(),
                distinctCrops.Select(group => group.Key.Height).Max(),
                distinctCrops.Select(group => group.Key.X).Min(),
                distinctCrops.Select(group => group.Key.Y).Min()
            );
        }

        throw new InvalidOperationException($"""
            {file}
            {string.Join(Environment.NewLine, crops)}
            """);
    }
}
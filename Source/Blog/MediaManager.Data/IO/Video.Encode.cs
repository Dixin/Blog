using Examples.Common;
using Examples.Diagnostics;

namespace Examples.IO;

internal static partial class Video
{
    internal static void Encode(string input, string output = "", bool overwrite = false, bool? estimateCrop = null, string? relativePath = null, int retryCount = 10, Action<string>? log = null)
    {
        log ??= TraceLog;
        VideoMetadata? videoMetadata = GetVideoMetadataAsync(input, null, relativePath, retryCount).Result;

        int bitRate = videoMetadata.Is1080P ? 2048 : 1280;
        List<string> videoFilters = new();
        if (!videoMetadata.Is1080P && !videoMetadata.Is720P)
        {
            videoFilters.Add("bwdif=mode=send_field:parity=auto:deint=all");
        }

        if (estimateCrop.HasValue)
        {
            (int width, int height, int x, int y) = GetVideoCrop(input, estimate: estimateCrop.Value);
            if (width != videoMetadata.Width || height != videoMetadata.Height)
            {
                videoFilters.Add($"crop={width}:{height}:{x}:{y}");
            }
        }

        if (output.IsNullOrWhiteSpace())
        {
            output = PathHelper.AddFilePostfix(input, ".ffmpeg");
            output = PathHelper.ReplaceExtension(output, VideoExtension);
        }

        string audio = videoMetadata.AudioBitRates.All(audioBitRate => audioBitRate > 260_000) ? "aac -ar 48000 -b:a 256k -ac" : "copy";
        string videoFilter = videoFilters.Any() ? $" -filter:v {string.Join(",", videoFilters)}" : string.Empty;

        ProcessHelper.StartAndWait(
            "ffmpeg",
            $"""
                -hwaccel auto -i "{input}" -loglevel verbose -c:v libx265 -profile:v main10 -pix_fmt yuv420p10le -preset slow -x265-params wpp=1:no-pmode=1:no-pe=1:no-psnr=1:no-ssim=1:log-level=info:input-csp=1:interlace=0:total-frames=0:level-idc=0:high-tier=1:uhd-bd=0:ref=4:no-allow-non-conformance=1:no-repeat-headers=1:annexb=1:no-aud=1:no-hrd=1:info=1:hash=0:no-temporal-layers=1:open-gop=1:min-keyint=23:keyint=250:gop-lookahead=0:bframes=4:b-adapt=2:b-pyramid=1:bframe-bias=0:rc-lookahead=25:lookahead-slices=4:scenecut=40:hist-scenecut=0:radl=0:no-splice=1:no-intra-refresh=1:ctu=64:min-cu-size=8:rect=1:no-amp=1:max-tu-size=32:tu-inter-depth=1:tu-intra-depth=1:limit-tu=0:rdoq-level=2:dynamic-rd=0.00:no-ssim-rd=1:signhide=1:no-tskip=1:nr-intra=0:nr-inter=0:no-constrained-intra=1:strong-intra-smoothing=1:max-merge=3:limit-refs=3:limit-modes=1:me=3:subme=3:merange=57:temporal-mvp=1:no-frame-dup=1:no-hme=1:weightp=1:no-weightb=1:no-analyze-src-pics=1:deblock=0\:0:no-sao=1:no-sao-non-deblock=1:rd=4:selective-sao=0:no-early-skip=1:rskip=1:no-fast-intra=1:no-tskip-fast=1:no-cu-lossless=1:no-b-intra=1:no-splitrd-skip=1:rdpenalty=0:psy-rd=2.00:psy-rdoq=1.00:no-rd-refine=1:no-lossless=1:cbqpoffs=0:crqpoffs=0:rc=abr:qcomp=0.60:qpstep=4:stats-write=0:stats-read=2:cplxblur=20.0:qblur=0.5:ipratio=1.40:pbratio=1.30:aq-mode=3:aq-strength=1.00:cutree=1:zone-count=0:no-strict-cbr=1:qg-size=32:no-rc-grain=1:qpmax=69:qpmin=0:no-const-vbv=1:sar=1:overscan=0:videoformat=5:range=0:colorprim=2:transfer=2:colormatrix=2:chromaloc=0:display-window=0:cll=0,0:min-luma=0:max-luma=1023:log2-max-poc-lsb=8:vui-timing-info=1:vui-hrd-info=1:slices=1:no-opt-qp-pps=1:no-opt-ref-list-length-pps=1:no-multi-pass-opt-rps=1:scenecut-bias=0.05:hist-threshold=0.01:no-opt-cu-delta-qp=1:no-aq-motion=1:no-hdr10=1:no-hdr10-opt=1:no-dhdr10-opt=1:no-idr-recovery-sei=1:analysis-reuse-level=0:analysis-save-reuse-level=0:analysis-load-reuse-level=0:scale-factor=0:refine-intra=0:refine-inter=0:refine-mv=1:refine-ctu-distortion=0:no-limit-sao=1:ctu-info=0:no-lowpass-dct=1:refine-analysis-type=0:copy-pic=1:max-ausize-factor=1.0:no-dynamic-refine=1:no-single-sei=1:no-hevc-aq=1:no-svt=1:no-field=1:qp-adaptation-range=1.00:no-scenecut-aware-qpconformance-window-offsets=1:bitrate={bitRate} -b:v {bitRate}k -map 0:v:0 -map 0:a -map_metadata 0 -c:a {audio}{videoFilter} "{output}" -{(overwrite ? "y" : "n")}
                """,
                window: true);
    }
}
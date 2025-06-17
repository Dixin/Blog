namespace MediaManager.IO;

using Examples.Common;

internal static class VideoFileInfoExtensions
{
    internal static EncoderType GetEncoderType<T>(this T video) where T : IVideoFileInfo, IDefaultSettings
    {
        string encoder = video.Encoder.TrimStart('.').ToLowerInvariant();
        string origin = video.Origin.TrimStart('.');
        if (origin.EqualsIgnoreCase("HDRip"))
        {
            return video.Version.EqualsIgnoreCase("KOOK") ? EncoderType.KoreanPremium : EncoderType.Korean;
        }

        bool isBluRay = origin.ContainsIgnoreCase("BluRay");
        switch (encoder)
        {
            case FfmpegHelper.Executable:
                return isBluRay ? EncoderType.FfmpegX265BluRay : EncoderType.FfmpegX265;
            case "nvenc":
                return isBluRay ? EncoderType.NvidiaX265BluRay : EncoderType.NvidiaX265;
            case "handbrake":
                return isBluRay ? EncoderType.HandbrakeH264BluRay : EncoderType.HandbrakeH264;
        }

        if (video.Version.EqualsOrdinal(T.Settings.TopEnglishKeyword) || video.Version.EqualsOrdinal(T.Settings.TopForeignKeyword))
        {
            return video.VideoCodec.ContainsIgnoreCase(".x265")
                ? isBluRay ? EncoderType.TopX265BluRay : EncoderType.TopX265
                : isBluRay ? EncoderType.TopH264BluRay : EncoderType.TopH264;
        }

        if (video.Version.EqualsIgnoreCase(T.Settings.PreferredOldKeyword) || video.Version.StartsWithIgnoreCase($"[{T.Settings.PreferredNewKeyword}."))
        {
            return video.VideoCodec.ContainsIgnoreCase(".x265")
                ? isBluRay ? EncoderType.PreferredX265BluRay : EncoderType.PreferredX265
                : isBluRay ? EncoderType.PreferredH264BluRay : EncoderType.PreferredH264;
        }

        if (video.Version.EqualsOrdinal(T.Settings.ContrastKeyword))
        {
            return isBluRay ? EncoderType.ContrastBluRay : EncoderType.Contrast;
        }

        return isBluRay ? EncoderType.HDBluRay : EncoderType.HD;
    }

    internal static DefinitionType GetDefinitionType(this IVideoFileInfo video) =>
        video.Edition.EndsWithIgnoreCase(Video.UpScaleDefinition)
            ? DefinitionType.P480
            : video.Definition switch
            {
                ".2160p" => DefinitionType.P2160,
                ".1080p" => DefinitionType.P1080,
                ".720p" => DefinitionType.P720,
                _ => DefinitionType.P480
            };

    internal static bool IsHD(this IVideoFileInfo video) =>
        video.Definition is ".2160p" or ".1080p" or ".720p" && !video.Edition.EndsWithIgnoreCase(Video.UpScaleDefinition);

    internal static bool IsHdr(this IVideoFileInfo video) =>
        video.VideoCodec.ContainsIgnoreCase(".HDR") || video.Edition.ContainsOrdinal(".HDR");

    internal static bool IsHdrDolbyVision(this IVideoFileInfo video) =>
        video.VideoCodec.ContainsIgnoreCase(".HDR.DV");

    internal static bool Is3D(this IVideoFileInfo video) =>
        video is VideoMovieFileInfo movie && movie.ThreeD.IsNotNullOrWhiteSpace() || video.Edition.ContainsOrdinal(".3D");

    internal static string FormatAudioCount(this IVideoFileInfo video) =>
        video.MultipleAudio.IsNullOrWhiteSpace() ? string.Empty : Regex.Match(video.MultipleAudio, @"\.([2-9])Audio").Groups[1].Value;
}
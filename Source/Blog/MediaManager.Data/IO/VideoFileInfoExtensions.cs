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
            return encoder.EqualsIgnoreCase(FfmpegHelper.Executable) ? EncoderType.KoreanFfmpegX265 : EncoderType.Korean;
        }

        bool isBluRay = origin.EqualsIgnoreCase("BluRay");
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
            return video.VideoCodec.EqualsOrdinal(".x265")
                ? isBluRay ? EncoderType.TopX265BluRay : EncoderType.TopX265
                : isBluRay ? EncoderType.TopH264BluRay : EncoderType.TopH264;
        }

        if (video.Version.EqualsIgnoreCase(T.Settings.PreferredOldKeyword) || video.Version.StartsWithIgnoreCase($"[{T.Settings.PreferredNewKeyword}."))
        {
            return video.VideoCodec.StartsWithOrdinal(".x265")
                ? isBluRay ? EncoderType.PreferredX265BluRay : EncoderType.PreferredX265
                : isBluRay ? EncoderType.PreferredH264BluRay : EncoderType.PreferredH264;
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

    internal static string FormatAudioCount(this IVideoFileInfo video) =>
        video.MultipleAudio.IsNullOrWhiteSpace() ? string.Empty : Regex.Match(video.MultipleAudio, @"\.([2-9])Audio").Groups[1].Value;
}
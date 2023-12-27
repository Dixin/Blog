namespace MediaManager.IO;

using Examples.Common;

internal static class VideoFileInfoExtensions
{
    internal static EncoderType GetEncoderType<T>(this T video) where T : IVideoFileInfo, IDefaultSettings
    {
        if (video.Version.EqualsOrdinal(T.Settings.TopEnglishKeyword) || video.Version.EqualsOrdinal(T.Settings.TopForeignKeyword))
        {
            return video.VideoCodec.EqualsOrdinal(".x265") ? EncoderType.X : EncoderType.H;
        }

        if (video.Version.EqualsIgnoreCase(T.Settings.PreferredOldKeyword) || video.Version.StartsWithIgnoreCase($"[{T.Settings.PreferredNewKeyword}."))
        {
            return video.VideoCodec.StartsWithOrdinal(".x265") ? EncoderType.XY : EncoderType.Y;
        }

        return video.Encoder.TrimStart('.') switch
        {
            "ffmpeg" => EncoderType.F,
            "nvenc" => EncoderType.N,
            "handbrake" => EncoderType.B,
            _ => EncoderType.P
        };
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
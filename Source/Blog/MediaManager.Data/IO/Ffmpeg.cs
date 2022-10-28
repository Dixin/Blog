namespace Examples.IO;

using Examples.Diagnostics;

internal class Ffmpeg
{
    internal static void MergeRarbgTV(string directory, string originalVideoSearchPattern, string originalSubtitleSearchPattern, Func<string, string> getDubbedVideo, Action<string?>? log = null)
    {
        Directory
            .GetFiles(directory, originalVideoSearchPattern, SearchOption.AllDirectories)
            .ForEach(originalVideo =>
            {
                string outputVideo = PathHelper.AddFilePostfix(originalVideo, ".2Audio");
                string dubbedVideo = getDubbedVideo(originalVideo);
                int exitCode = ProcessHelper.StartAndWait("ffmpeg", $@" -i ""{originalVideo}"" -i ""{dubbedVideo}"" -c copy -map_metadata 0 -map 0 -map 1:a ""{outputVideo}""", log, log);
                Debug.Assert(exitCode == 0);
            });
        Directory
            .GetFiles(directory, originalSubtitleSearchPattern, SearchOption.AllDirectories)
            .ForEach(subtitle =>
            {
                string newSubtitle = subtitle.Contains(".chs.") ? subtitle.Replace(".chs.", ".2Audio.chs.") : PathHelper.AddFilePrefix(subtitle, ".2Audio");
                File.Move(subtitle, newSubtitle);
            });
    }
}
using Examples.IO;

namespace MediaManager.IO;

internal static partial class Photo
{
    private static readonly string[] CommonImageExtensions = [".avif", ".bmp", ".dng", ".gif", ".heic", ".heif", ".jfif", ".jpeg", ".jpg", ".nef", ".png", ".psb", ".psd", ".tif", ".tiff", ".webp"];

    private static readonly string[] Image360Extensions = [".insp", ".insv", ".lrv"];

    private static readonly string[] AllImageExtensions = [.. CommonImageExtensions, .. Image360Extensions];

    [GeneratedRegex(@"[0-9]{2,}")]
    private static partial Regex NumbersRegex();

    internal static bool IsImage(this string file) => file.HasAnyExtension(AllImageExtensions);

    internal static bool IsCommonImage(this string file) => file.HasAnyExtension(CommonImageExtensions);

    internal static bool Is360Image(this string file) => file.HasAnyExtension(Image360Extensions);

    internal static void PrintDirectoriesWithErrors(string directory, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Directory.EnumerateDirectories(directory)
            .Order()
            .ForEach(album =>
            {
                string name = PathHelper.GetFileName(album);
                MatchCollection matches = NumbersRegex().Matches(name);
                if (matches.Any(match => match.Value.Length is not 8))
                {
                    log(album);
                }

                matches = ExifMetadata.Date360Regex().Matches(name);
                if (matches.Count == 0)
                {
                    return;
                }

                switch (matches.Count)
                {
                    case 1:
                        Debug.Assert(name[matches.Single().Index - 1] is '.');
                        break;
                    case 2:
                        DateOnly minDate = DateOnly.ParseExact(matches.First().Value, "yyyyMMdd");
                        Debug.Assert(name[matches.First().Index - 1] is '.');
                        DateOnly maxDate = DateOnly.ParseExact(matches.Last().Value, "yyyyMMdd");
                        Debug.Assert(name[matches.Last().Index - 1] is '~');
                        Debug.Assert(minDate < maxDate);
                        break;
                    default:
                        Debug.Fail(album);
                        break;
                }
            });
    }

    internal static void PrintImagesWithErrors(string directory, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        Lock logLock = new();

        Directory.EnumerateDirectories(directory)
            .Order()
            .AsParallel()
            .WithDegreeOfParallelism(2)
            .ForAll(album =>
            {
                string name = PathHelper.GetFileName(album);
                MatchCollection matches = ExifMetadata.Date360Regex().Matches(name);
                if (matches.Count == 0)
                {
                    return;
                }

                List<string> errors = [];
                switch (matches.Count)
                {
                    case 1:
                        DateOnly date = DateOnly.ParseExact(matches.Single().Value, "yyyyMMdd");
                        Directory.EnumerateFiles(album, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                            .Where(file => file.IsImage() || file.IsVideo())
                            .ForEach(file =>
                            {
                                if (ExifMetadata.TryGetTakenDate(file, out DateOnly? takenDate) && takenDate.Value != date)
                                {
                                    errors.Add($"{file} | {takenDate.Value.ToString("yyyyMMdd")}");
                                }
                            });
                        break;
                    case 2:
                        DateOnly minDate = DateOnly.ParseExact(matches.First().Value, "yyyyMMdd");
                        DateOnly maxDate = DateOnly.ParseExact(matches.Last().Value, "yyyyMMdd");
                        Debug.Assert(minDate < maxDate);
                        Directory.EnumerateFiles(album, PathHelper.AllSearchPattern, SearchOption.AllDirectories)
                            .Where(file => file.IsImage() || file.IsVideo())
                            .ForEach(file =>
                            {
                                if (ExifMetadata.TryGetTakenDate(file, out DateOnly? takenDate) && (takenDate.Value < minDate || takenDate.Value > maxDate))
                                {
                                    errors.Add($"{file} | {takenDate.Value.ToString("yyyyMMdd")}");
                                }
                            });
                        break;
                    default:
                        Debug.Fail(album);
                        break;
                }

                if (errors.IsEmpty())
                {
                    return;
                }

                lock (logLock)
                {
                    errors.ForEach(log);
                }
            });
    }
}
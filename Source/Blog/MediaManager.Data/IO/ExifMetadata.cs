using System.Drawing;
using System.Drawing.Imaging;
using Examples.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = MetadataExtractor.Directory;

namespace MediaManager.IO;

using Examples.Common;

internal static partial class ExifMetadata
{
    [GeneratedRegex("[0-9]{8}")]
    internal static partial Regex Date360Regex();

    [GeneratedRegex(":")]
    private static partial Regex SeparatorRegex();

    internal static bool TryGetTakenDate(string file, [NotNullWhen(true)] out DateOnly? takenDate, Action<string>? log = null)
    {
        log ??= Logger.WriteLine;

        if (file.Is360Image())
        {
            string date = Date360Regex().Match(PathHelper.GetFileNameWithoutExtension(file)).Value;
            takenDate = DateOnly.ParseExact(date, "yyyyMMdd");
            return true;
        }

        IReadOnlyList<Directory> directories;
        try
        {
            directories = ImageMetadataReader.ReadMetadata(file);
        }
        catch (ImageProcessingException imageProcessingException)
        {
            log($"! {file}");
            log(imageProcessingException.ToString());
            takenDate = null;
            return false;
        }

        DateTime takenDateTime = DateTime.MinValue;
        ExifSubIfdDirectory? result = directories
            .OfType<ExifSubIfdDirectory>()
            .FirstOrDefault(subIfdDirectory => subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out takenDateTime));
        Debug.Assert(result is null == (takenDateTime == DateTime.MinValue));
        if (result is not null)
        {
            takenDate = DateOnly.FromDateTime(takenDateTime);
            return true;
        }

        try
        {
            using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
            using Image image = Image.FromStream(fileStream, false, false);
            PropertyItem propertyItem = image.GetPropertyItem(36867);
            string dateTaken = Encoding.UTF8.GetString(propertyItem.Value);
            dateTaken = SeparatorRegex().Replace(dateTaken, "-", 2);
            DateTime dateTime = DateTime.Parse(dateTaken);
            takenDate = DateOnly.FromDateTime(dateTime);
            return true;
        }
        catch (Exception exception) when (exception.IsNotCritical())
        {
            log($"! {file}");
            log(exception.ToString());
            takenDate = null;
            return false;
        }
    }
}
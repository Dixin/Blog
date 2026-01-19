using Examples.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace MediaManager.IO;

using Directory = MetadataExtractor.Directory;

internal static partial class ExifMetadata
{
    [GeneratedRegex("[0-9]{8}")]
    internal static partial Regex Date360Regex();

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
        if (takenDateTime == DateTime.MinValue)
        {
            takenDate = null;
            return false;
        }

        takenDate = DateOnly.FromDateTime(takenDateTime);
        return true;

        //using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        //using Image myImage = Image.FromStream(fs, false, false);
        //PropertyItem propItem = myImage.GetPropertyItem(36867);
        //string value = Encoding.UTF8.GetString(propItem.Value);
        //string dateTaken = new Regex(":").Replace(value, "-", 2);
        //DateTime dateTime = DateTime.Parse(dateTaken);
        //return DateOnly.FromDateTime(dateTime);
    }
}
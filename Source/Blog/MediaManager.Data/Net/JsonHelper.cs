namespace Examples.Net;

using Examples.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;

internal class JsonHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static TValue Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json) =>
        DeserializeNullable<TValue>(json) ?? throw new InvalidOperationException($"{json} should not be null.");

    public static TValue? DeserializeNullable<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json) =>
        JsonSerializer.Deserialize<TValue>(json, DeserializerOptions);

    public static TValue DeserializeFromFile<TValue>(string file)
    {
        string jsonContent = File.ReadAllText(file);
        return Deserialize<TValue>(jsonContent);
    }

    public static async Task<TValue> DeserializeFromFileAsync<TValue>(string file)
    {
        string jsonContent = await File.ReadAllTextAsync(file);
        return Deserialize<TValue>(jsonContent);
    }

    public static string Serialize<TValue>(TValue value) =>
        JsonSerializer.Serialize(value, SerializerOptions);

    public static void SerializeToFile<TValue>(TValue value, string file, object? @lock = null)
    {
        string jsonContent = Serialize(value);
        FileHelper.WriteText(file, jsonContent, @lock: @lock);
    }

    public static async Task SerializeToFileAsync<TValue>(TValue value, string file)
    {
        string jsonContent = Serialize(value);
        await FileHelper.WriteTextAsync(file, jsonContent);
    }
}

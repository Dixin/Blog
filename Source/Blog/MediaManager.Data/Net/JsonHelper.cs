namespace MediaManager.Net;

using System.Text.Encodings.Web;
using System.Text.Unicode;
using Examples.IO;
using Examples.Text.Json;

internal static class JsonHelper
{
    internal static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        TypeInfoResolver = new AlphabeticalPropertyJsonTypeInfoResolver()
    };

    internal static JsonSerializerOptions DeserializerOptions => new()
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

    public static async Task<TValue> DeserializeFromFileAsync<TValue>(string file, CancellationToken cancellationToken = default)
    {
        string jsonContent = await File.ReadAllTextAsync(file, cancellationToken);
        return Deserialize<TValue>(jsonContent);
    }

    public static TValue DeserializeFromFile<TValue>(string file, TValue @default) =>
        File.Exists(file)
            ? DeserializeNullable<TValue>(File.ReadAllText(file)) ?? @default
            : @default;

    public static async Task<TValue> DeserializeFromFileAsync<TValue>(string file, TValue @default, CancellationToken cancellationToken = default) =>
        File.Exists(file)
            ? DeserializeNullable<TValue>(await File.ReadAllTextAsync(file, cancellationToken)) ?? @default
            : @default;

    public static string Serialize<TValue>(TValue value) =>
        JsonSerializer.Serialize(value, SerializerOptions);

    public static void SerializeToFile<TValue>(TValue value, string file)
    {
        string jsonContent = Serialize(value);
        FileHelper.WriteText(file, jsonContent);
    }

    public static void SerializeToFile<TValue>(TValue value, string file, ref readonly Lock @lock)
    {
        string jsonContent = Serialize(value);
        FileHelper.WriteText(file, jsonContent, in @lock);
    }

    public static async Task SerializeToFileAsync<TValue>(TValue value, string file, CancellationToken cancellationToken = default)
    {
        string jsonContent = Serialize(value);
        await FileHelper.WriteTextAsync(file, jsonContent, cancellationToken: cancellationToken);
    }
}

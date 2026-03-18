namespace MediaManager.Net;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
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

    internal static JsonSerializerOptions DeserializerOptions
    {
        get
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }
    }

    public static TValue Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json) =>
        DeserializeNullable<TValue>(json) ?? throw new InvalidOperationException($"{json} should not be null.");

    public static TValue? DeserializeNullable<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json) =>
        JsonSerializer.Deserialize<TValue>(json, DeserializerOptions);

    public static TValue DeserializeFromFile<TValue>(string file)
    {
        using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
        return JsonSerializer.Deserialize<TValue>(fileStream, DeserializerOptions)
            ?? throw new InvalidOperationException($"{file} content should not be null.");
    }

    public static async Task<TValue> DeserializeFromFileAsync<TValue>(string file, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
        return await JsonSerializer.DeserializeAsync<TValue>(fileStream, DeserializerOptions, cancellationToken)
            ?? throw new InvalidOperationException($"{file} content should not be null.");
    }

    public static TValue DeserializeFromFile<TValue>(string file, TValue @default)
    {
        if (File.Exists(file))
        {
            using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<TValue>(fileStream, DeserializerOptions)
                ?? @default;
        }

        return @default;
    }

    public static async Task<TValue> DeserializeFromFileAsync<TValue>(string file, TValue @default, CancellationToken cancellationToken = default)
    {
        if (File.Exists(file))
        {
            await using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
            return await JsonSerializer.DeserializeAsync<TValue>(fileStream, DeserializerOptions, cancellationToken)
                ?? @default;
        }

        return @default;
    }

    public static string Serialize<TValue>(TValue value) =>
        JsonSerializer.Serialize(value, SerializerOptions);

    public static void SerializeToFile<TValue>(TValue value, string file, bool @finally = false)
    {
        using FileStream fileStream = new(file, FileMode.OpenOrCreate, FileAccess.Write);
        if (@finally)
        {
            try { }
            finally
            {
                JsonSerializer.Serialize(fileStream, value, SerializerOptions);
            }
        }
        else
        {
            JsonSerializer.Serialize(fileStream, value, SerializerOptions);
        }
    }

    public static void SerializeToFile<TValue>(TValue value, string file, ref readonly Lock @lock, bool @finally = false)
    {
        using FileStream fileStream = new(file, FileMode.OpenOrCreate, FileAccess.Write);
        lock (@lock)
        {
            if (@finally)
            {
                try { }
                finally
                {
                    JsonSerializer.Serialize(fileStream, value, SerializerOptions);
                }
            }
            else
            {
                JsonSerializer.Serialize(fileStream, value, SerializerOptions);
            }
        }
    }

    public static async Task SerializeToFileAsync<TValue>(TValue value, string file, bool @finally = false, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = new(file, FileMode.OpenOrCreate, FileAccess.Write);
        if (@finally)
        {
            try { }
            finally
            {
                await JsonSerializer.SerializeAsync(fileStream, value, SerializerOptions, cancellationToken);
            }
        }
        else
        {
            await JsonSerializer.SerializeAsync(fileStream, value, SerializerOptions, cancellationToken);
        }
    }
}

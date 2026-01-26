namespace Examples.Text.Json;

using System.Text.Json.Serialization.Metadata;

public class AlphabeticalPropertyJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            JsonPropertyInfo[] orderedProperties = typeInfo.Properties.OrderBy(property => property.Name, StringComparer.Ordinal).ToArray();
            typeInfo.Properties.Clear();
            foreach (JsonPropertyInfo property in orderedProperties)
            {
                typeInfo.Properties.Add(property);
            }
        }

        return typeInfo;
    }
}
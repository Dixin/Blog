#if !NET8_0_OR_GREATER
namespace Examples.Runtime.Serialization;

using System.Runtime.Serialization;

using Examples.Common;

public class Serializer
{
    private readonly IFormatter formatter;

    public Serializer(IFormatter formatter)
    {
        this.formatter = formatter;
    }

    public string Serialize(ISerializable serializable)
    {
        serializable.NotNull();

        using MemoryStream stream = new();
#pragma warning disable SYSLIB0011
        this.formatter.Serialize(stream, serializable);
#pragma warning restore SYSLIB0011
        return Convert.ToBase64String(stream.ToArray());
    }

    public T Deserialize<T>(string base64) where T : class, ISerializable =>
#pragma warning disable SYSLIB0011
        this.formatter.Deserialize(new MemoryStream(Convert.FromBase64String(base64.NotNullOrEmpty()))) is T result
#pragma warning restore SYSLIB0011
            ? result
            : throw new SerializationException();
}
#endif
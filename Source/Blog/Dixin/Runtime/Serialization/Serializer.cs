namespace Dixin.Runtime.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    using Dixin.Common;

    public class Serializer
    {
        private readonly IFormatter formatter;

        public Serializer(IFormatter formatter)
        {
            this.formatter = formatter;
        }

        public string Serialize(ISerializable serializable)
        {
            serializable.NotNull(nameof(serializable));

            using (MemoryStream stream = new MemoryStream())
            {
                this.formatter.Serialize(stream, serializable);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public T Deserialize<T>(string base64) where T : class, ISerializable
        {
            base64.NotNullOrEmpty(nameof(base64));

            T result = this.formatter.Deserialize(new MemoryStream(Convert.FromBase64String(base64))) as T;
            if (result != null)
            {
                return result;
            }

            throw new SerializationException();
        }
    }
}

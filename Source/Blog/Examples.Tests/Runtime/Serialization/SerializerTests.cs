﻿#if !NET8_0_OR_GREATER
namespace Examples.Tests.Runtime.Serialization;

using System.Runtime.Serialization.Formatters.Binary;

using Examples.Runtime.Serialization;

[TestClass]
public class SerializerTests
{
    [TestMethod]
    public void SerializeDeserializeTest()
    {
        Exception? exceptionToSerialize = null;
        int int32 = 1;
        try
        {
            int32 /= int32 - int32;
        }
        catch (Exception exception)
        {
            exceptionToSerialize = exception;
        }

        Serializer serializer = new(new BinaryFormatter());
        string base64 = serializer.Serialize(exceptionToSerialize ?? throw new AssertFailedException());
        Trace.WriteLine(base64);
        Assert.IsFalse(string.IsNullOrWhiteSpace(base64));

        Exception serializedException = serializer.Deserialize<Exception>(base64);
        Trace.WriteLine(serializedException);
        Assert.IsNotNull(serializedException);
        Assert.IsNotNull(serializedException.GetType());
        Assert.IsFalse(string.IsNullOrWhiteSpace(serializedException.Message));
    }
}
#endif
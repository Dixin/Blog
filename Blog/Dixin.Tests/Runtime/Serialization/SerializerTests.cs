﻿namespace Dixin.Tests.Runtime.Serialization
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization.Formatters.Soap;

    using Dixin.Runtime.Serialization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SerializerTests
    {
        [TestMethod]
        public void SerializeDeserializeTest()
        {
            Exception exceptionToSerialize = null;
            int int32 = 1;
            try
            {
                int32 = int32 / (int32 - int32);
            }
            catch (Exception exception)
            {
                exceptionToSerialize = exception;
            }

            Serializer serializer = new Serializer(new SoapFormatter());
            string base64 = serializer.Serialize(exceptionToSerialize);
            Trace.WriteLine(base64);
            Assert.IsFalse(string.IsNullOrWhiteSpace(base64));

            Exception serializedException = serializer.Deserialize<Exception>(base64);
            Trace.WriteLine(serializedException);
            Assert.IsNotNull(serializedException);
            Assert.IsNotNull(serializedException.GetType());
            Assert.IsFalse(string.IsNullOrWhiteSpace(serializedException.Message));
        }
    }
}

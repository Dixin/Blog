namespace Dixin.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    public static class EncodingHelper
    {
        public static string Convert(string value, Encoding from, Encoding to)
        {
            Contract.Requires<ArgumentNullException>(from != null);
            Contract.Requires<ArgumentNullException>(to != null);

            byte[] fromBytes = from.GetBytes(value);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);

            return to.GetString(toBytes);
        }

        public static string GB2312ToUtf8
            (this string gb2312Value) => Convert(gb2312Value, Encoding.GetEncoding("gb2312"), Encoding.UTF8);

        public static string Utf8ToGB2312
            (this string utf8Value) => Convert(utf8Value, Encoding.UTF8, Encoding.GetEncoding("gb2312"));

        public static void Convert(Encoding from, Encoding to, string fromPath, string toPath = null)
        {
            Contract.Requires<ArgumentNullException>(from != null);
            Contract.Requires<ArgumentNullException>(to != null);

            byte[] fromBytes = File.ReadAllBytes(fromPath);
            byte[] toBytes = Encoding.Convert(from, to, fromBytes);
            File.WriteAllBytes(toPath ?? fromPath, toBytes);
        }
    }
}
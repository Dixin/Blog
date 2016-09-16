namespace Dixin.Linq
{
    public static class TraceHelper
    {
        public static string TraceLine(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
            return message;
        }

        public static T TraceLine<T>(T value)
        {
            System.Diagnostics.Trace.WriteLine(value);
            return value;
        }

        public static string Trace(string message)
        {
            System.Diagnostics.Trace.Write(message);
            return message;
        }

        public static T Trace<T>(T value)
        {
            System.Diagnostics.Trace.Write(value);
            return value;
        }
    }
}

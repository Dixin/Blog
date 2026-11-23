namespace Examples.Common;

using Examples.Threading.Tasks;
using static Result;

using Ok = Examples.Common.Unit;

public class ResultException : Exception
{
    public ResultException() { }

    public ResultException(string message) : base(message) { }

    public ResultException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class ResultException<TError> : ResultException
{
    public TError Error { get; }

    public ResultException(TError error) => this.Error = error;

    public ResultException(string message, TError error)
        : base(message) =>
        this.Error = error;

    public ResultException(string message, TError error, Exception innerException)
        : base(message, innerException) =>
        this.Error = error;
}

public static partial class Result
{
    public static Ok Ok() => Common.Unit.Value;

    public static Unit Unit() => Common.Unit.Value;
}

internal static class ResultTest
{
    internal static Result<int, DivideByZeroException> Divide(int a, int b) =>
        b == 0
            ? new DivideByZeroException()
            : a / b;

    internal static Result<int, OverflowException> Abs(int value)
    {
        if (value >= 0)
        {
            return value;
        }

        value = -value;
        return value < 0 ? new OverflowException() : value;
    }

    internal static void PrintDivide(int a, int b, int c)
    {
        string message =
            (from result1 in Divide(a, b)
             from result2 in Divide(result1, c)
             select result2) switch
            {
                int result => $"Ok: {result}",
                DivideByZeroException error => $"Fail: {error.Message}"
            };

        Console.WriteLine(message);
    }

    internal static Result<Ok> Print(int a, int b, int c)
    {
        Result<string> result =
            from result1 in ~Divide(a, b)
            let d = result1 + c
            from result2 in ~Divide(d, c)
            from result3 in ~Abs(result2)
            select Math.Abs(result2).ToString();

        return result switch
        {
            string message => new Func<Ok>(() =>
            {
                Console.WriteLine($"Ok: {message}");
                return Ok();
            })(),
            Exception error => error,
        };
    }
}

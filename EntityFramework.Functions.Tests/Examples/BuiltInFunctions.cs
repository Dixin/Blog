namespace EntityFramework.Functions.Tests.Examples
{
    public static class BuiltInFunctions
    {
        [Function("LEFT", FunctionType.BuiltInFunction)]
        public static string Left(this string value, int count) => Function.CallNotSupported<string>(nameof(Left));
    }
}

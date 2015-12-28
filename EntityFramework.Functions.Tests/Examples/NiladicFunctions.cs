namespace EntityFramework.Functions.Tests.Examples
{
    using System;

    // https://technet.microsoft.com/en-us/library/ms174979.aspx
    public static class NiladicFunctions
    {
        [Function(FunctionType.NiladicFunction, "CURRENT_TIMESTAMP")]
        public static DateTime? CurrentTimestamp() => Function.CallNotSupported<DateTime?>();

        [Function(FunctionType.NiladicFunction, "CURRENT_USER")]
        public static string CurrentUser() => Function.CallNotSupported<string>();

        [Function(FunctionType.NiladicFunction, "SESSION_USER")]
        public static string SessionUser() => Function.CallNotSupported<string>();

        [Function(FunctionType.NiladicFunction, "SYSTEM_USER")]
        public static string SystemUser() => Function.CallNotSupported<string>();

        [Function(FunctionType.NiladicFunction, "USER")]
        public static string User() => Function.CallNotSupported<string>();
    }
}
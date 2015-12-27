namespace EntityFramework.Functions.Tests.Examples
{
    using System;

    public static class NiladicFunctions
    {

        [Function("CURRENT_TIMESTAMP", FunctionType.NiladicFunction)]
        public static DateTime? CurrentTimestamp() => Function.CallNotSupported<DateTime?>(nameof(CurrentTimestamp));

        [Function("CURRENT_USER", FunctionType.NiladicFunction)]
        public static string CurrentUser() => Function.CallNotSupported<string>(nameof(CurrentUser));

        [Function("SESSION_USER", FunctionType.NiladicFunction)]
        public static string SessionUser() => Function.CallNotSupported<string>(nameof(SessionUser));

        [Function("SYSTEM_USER", FunctionType.NiladicFunction)]
        public static string SystemUser() => Function.CallNotSupported<string>(nameof(SystemUser));

        [Function("USER", FunctionType.NiladicFunction)]
        public static string User() => Function.CallNotSupported<string>(nameof(User));
    }
}
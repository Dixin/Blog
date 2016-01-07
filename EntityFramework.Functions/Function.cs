namespace EntityFramework.Functions
{
    using System;
#if NET45
    using System.Runtime.CompilerServices;
#endif

    public static partial class Function
    {
        public static T CallNotSupported<T>(
#if NET45
            [CallerMemberName]
#endif
            string methodName = null)
        {
            // System.Data.Entity.Strings.ELinq_EdmFunctionDirectCall.
            throw new NotSupportedException(
                $"Direct call to method {methodName} is not supported. This function can only be invoked from LINQ to Entities.");
        }
    }
}

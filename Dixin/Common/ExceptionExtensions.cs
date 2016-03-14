namespace Dixin.Common
{
    using System;
    using System.Threading;

    public static class ExceptionExtensions
    {
        public static bool IsCritical(this Exception exception) => 
            exception is AccessViolationException
            || exception is AppDomainUnloadedException
            || exception is BadImageFormatException
            || exception is CannotUnloadAppDomainException
            || exception is InvalidProgramException
            || exception is OutOfMemoryException
            || exception is ThreadAbortException;

        public static bool IsNotCritical(this Exception exception) =>
            !(exception is AccessViolationException
            || exception is AppDomainUnloadedException
            || exception is BadImageFormatException
            || exception is CannotUnloadAppDomainException
            || exception is InvalidProgramException
            || exception is OutOfMemoryException
            || exception is ThreadAbortException);

        public static bool Trace(this Exception exception, bool result = false)
        {
            System.Diagnostics.Trace.WriteLine(exception);
            return result;
        }
    }
}

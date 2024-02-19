namespace Examples.Common;

public static class ExceptionExtensions
{
    public static bool IsCritical(this Exception exception) =>
        exception is AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or CannotUnloadAppDomainException
            or InvalidProgramException
            or OutOfMemoryException
            or ThreadAbortException;

    public static bool IsNotCritical(this Exception exception) =>
        exception is not (AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or CannotUnloadAppDomainException
            or InvalidProgramException
            or OutOfMemoryException
            or ThreadAbortException);

    public static bool Trace(this Exception exception, bool result = false)
    {
        System.Diagnostics.Trace.WriteLine(exception);
        return result;
    }
}
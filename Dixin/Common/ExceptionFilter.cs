namespace Dixin.Common
{
    using System;
    using System.Diagnostics;

    internal static partial class ExceptionFilter
    {
        private static void A() => B();

        private static void B() => C();

        private static void C() => D();

        private static void D()
        {
            int localVariable1 = 1;
            int localVariable2 = 2;
            int localVariable3 = 3;
            int localVariable4 = 4;
            int localVariable5 = 5;
            throw new OperationCanceledException(nameof(ExceptionFilter));
        }

        private static bool Log(this object message, bool result = false)
        {
            Trace.WriteLine(message);
            return result;
        }
    }

    internal static partial class ExceptionFilter
    {
        private static void Filter()
        {
            try
            {
                A();
            }
            catch (OperationCanceledException exception) when (string.Equals(nameof(ExceptionFilter), exception.Message, StringComparison.Ordinal))
            {
            }
        }

#if ERROR
        private static void Filter()
        {
            try
            {
                A();
            }
            catch (OperationCanceledException exception) 
         // {
                if (string.Equals(nameof(ExceptionFilter), exception.Message, StringComparison.Ordinal))
                {
                }
         // }
        }
#endif
    }

    internal static partial class ExceptionFilter
    {
        private static void Catch()
        {
            try
            {
                A();
            }
            catch (Exception exception)
            {
                exception.Log();
                throw;
            }
        }

        private static void When()
        {
            try
            {
                A();
            }
            catch (Exception exception) when (exception.Log())
            {
            }
        }

        internal static void Log()
        {
            try
            {
                A();
            }
            catch (Exception exception) when (exception.Log(true))
            {
                exception.Log();
                throw;
            }
        }
    }
}

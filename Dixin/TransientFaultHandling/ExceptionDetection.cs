namespace Dixin.TransientFaultHandling
{
    using System;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    public class ExceptionDetection : ITransientErrorDetectionStrategy
    {
        private readonly Func<Exception, bool> isTransient;

        public ExceptionDetection(Func<Exception, bool> isTransient = null)
        {
            this.isTransient = isTransient ?? (_ => true);
        }

        public bool IsTransient(Exception exception) => this.isTransient(exception);
    }

    public class TransientDetection<TException> : ExceptionDetection
        where TException : Exception
    {
        public TransientDetection() : base(exception => exception is TException)
        {
        }
    }
}
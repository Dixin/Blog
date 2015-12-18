namespace Dixin.Threading
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;

    public sealed class SingleInstance : IDisposable
    {
        private readonly Mutex mutex;

        public SingleInstance(string mutexName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(mutexName));

            this.mutex = new Mutex(true, mutexName);
            this.IsSingle = this.mutex.WaitOne(TimeSpan.Zero, true);
        }

        public bool IsSingle { get; }

        public void Dispose()
        {
            if (this.IsSingle)
            {
                this.mutex.ReleaseMutex();
            }

            this.mutex.Dispose();
        }

        public static bool Detect(string mutexName, Action single, Action multiple = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(mutexName));
            Contract.Requires<ArgumentNullException>(single != null);

            using (Mutex mutex = new Mutex(true, mutexName))
            {
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    try
                    {
                        single();
                        return true;
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }

                multiple?.Invoke();
                return false;
            }
        }
    }
}

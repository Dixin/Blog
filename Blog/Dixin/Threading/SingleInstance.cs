﻿namespace Dixin.Threading
{
    using System;
    using System.Threading;

    using Dixin.Common;

    public sealed class SingleInstance : IDisposable
    {
        private readonly Mutex mutex;

        public SingleInstance(string mutexName)
        {
            mutexName.NotNullOrEmpty(nameof(mutexName));

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
            mutexName.NotNullOrEmpty(nameof(mutexName));
            single.NotNull(nameof(single));

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

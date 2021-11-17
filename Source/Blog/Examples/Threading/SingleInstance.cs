namespace Examples.Threading;

using Examples.Common;

public sealed class SingleInstance : IDisposable
{
    private readonly Mutex mutex;

    public SingleInstance(string mutexName)
    {
        this.mutex = new(true, mutexName.NotNullOrEmpty());
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

    public static bool Detect(string mutexName, Action single, Action? multiple = null)
    {
        single.NotNull();

        using Mutex mutex = new(true, mutexName.NotNullOrEmpty());
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
namespace Dixin.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public interface IAwaitable
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter : INotifyCompletion // or ICriticalNotifyCompletion
    {
        // INotifyCompletion has one method: void OnCompleted(Action continuation);

        // ICriticalNotifyCompletion implements INotifyCompletion,
        // with one more method: void UnsafeOnCompleted(Action continuation);

        bool IsCompleted { get; }

        void GetResult();
    }

    public interface IAwaitable<out TResult>
    {
        IAwaiter<TResult> GetAwaiter();
    }

    public interface IAwaiter<out TResult> : INotifyCompletion // or ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }

        TResult GetResult();
    }

    internal class FuncAwaitable<TResult> : IAwaitable<TResult>
    {
        private readonly Func<TResult> function;

        public FuncAwaitable(Func<TResult> function)
        {
            this.function = function;
        }

        public IAwaiter<TResult> GetAwaiter() => new FuncAwaiter<TResult>(this.function);
    }

    public sealed class FuncAwaiter<TResult> : IAwaiter<TResult>, IDisposable
    {
        private readonly Task<TResult> task;

        public FuncAwaiter(Func<TResult> function)
        {
            this.task = new Task<TResult>(function);
            this.task.Start();
        }

        bool IAwaiter<TResult>.IsCompleted => this.task.IsCompleted;

        TResult IAwaiter<TResult>.GetResult() => this.task.Result;

        void INotifyCompletion.OnCompleted(Action continuation) => new Task(continuation).Start();

        public void Dispose() => this.task.Dispose();
    }

    public static partial class FuncExtensions
    {
        public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> function)
        {
            Task<TResult> task = new Task<TResult>(function);
            task.Start();
            return task.GetAwaiter();
        }
    }

    public static partial class ActionExtensions
    {
        public static TaskAwaiter GetAwaiter(this Action action)
        {
            Task task = new Task(action);
            task.Start();
            return task.GetAwaiter();
        }
    }
}

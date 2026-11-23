namespace Examples.Threading.Tasks;

public interface IAwaitable
{
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    IAwaiter GetAwaiter();
}

public interface IAwaiter : INotifyCompletion // or ICriticalNotifyCompletion
{
    bool IsCompleted { get; }

    void GetResult();
}

public interface ICriticalAwaiter : IAwaiter, ICriticalNotifyCompletion;

public interface IAwaitable<out TResult, out TAwaiter> where TAwaiter : IAwaiter<TResult>
{
    TAwaiter GetAwaiter();
}

public interface IAwaiter<out TResult> : INotifyCompletion // or ICriticalNotifyCompletion
{
    bool IsCompleted { get; }

    TResult GetResult();
}

public interface ICriticalAwaiter<out TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion;

public interface IAsyncMethodBuilder<in TResult, out TSelf, out TAwaitable>
    where TSelf : IAsyncMethodBuilder<TResult, TSelf, TAwaitable>
{
    static abstract TSelf Create();

    TAwaitable Task { get; }

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine;

    public void SetStateMachine(IAsyncStateMachine stateMachine);

    public void SetResult(TResult result);

    public void SetException(Exception exception);

    public void AwaitOnCompleted<TNotifyCompletionAwaiter, TStateMachine>(
        ref TNotifyCompletionAwaiter awaiter, ref TStateMachine stateMachine)
        where TNotifyCompletionAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine;

    public void AwaitUnsafeOnCompleted<TCriticalNotifyCompletionAwaiter, TStateMachine>(
        ref TCriticalNotifyCompletionAwaiter awaiter, ref TStateMachine stateMachine)
        where TCriticalNotifyCompletionAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine;
}

[AsyncMethodBuilder(typeof(AsyncFunMethodBuilder<>))]
public delegate TResult Fun<out TResult>();

public sealed class FunAwaiter<TResult>(TaskAwaiter<TResult> taskAwaiter) : IAwaiter<TResult>
{

    public bool IsCompleted => taskAwaiter.IsCompleted;

    public TResult GetResult() => taskAwaiter.GetResult();

    public void OnCompleted(Action continuation) => taskAwaiter.OnCompleted(continuation);
}

public static partial class FunExtensions
{
    public static FunAwaiter<TResult> GetAwaiter<TResult>(this Fun<TResult> fun) => 
        new(Task.Run(() => fun()).GetAwaiter());
}
// Not needed
// public class FunAwaitable<TResult>(Task<TResult> task) : IAwaitable<TResult, FunAwaiter<TResult>>
// {
//    public FunAwaiter<TResult> GetAwaiter() => new(task.GetAwaiter());
// }

public class AsyncFunMethodBuilder<TResult> : IAsyncMethodBuilder<TResult, AsyncFunMethodBuilder<TResult>, Fun<TResult>>
{
    private AsyncTaskMethodBuilder<TResult> taskBuilder;

    public static AsyncFunMethodBuilder<TResult> Create() =>
        new() { taskBuilder = AsyncTaskMethodBuilder<TResult>.Create() };

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        this.taskBuilder.Start(ref stateMachine);

    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        this.taskBuilder.SetStateMachine(stateMachine);

    public void SetResult(TResult result) => this.taskBuilder.SetResult(result);

    public void SetException(Exception exception) =>
        this.taskBuilder.SetException(exception);

    public Fun<TResult> Task => new(() => this.taskBuilder.Task.Result);

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine =>
        this.taskBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
        this.taskBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
}
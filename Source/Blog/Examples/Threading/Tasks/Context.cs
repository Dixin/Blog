﻿namespace Examples.Threading.Tasks;

using Examples.Common;

public static partial class ActionExtensions
{
    public static Task InvokeWith(this Action action, SynchronizationContext synchronizationContext, ExecutionContext executionContext)
    {
        action.ThrowIfNull();

        return new Func<object?>(() =>
        {
            action();
            return null;
        }).InvokeWith(synchronizationContext, executionContext);
    }
}

public static partial class FuncExtensions
{
    public static TResult InvokeWith<TResult>(this Func<TResult> function, ExecutionContext? executionContext)
    {
        function.ThrowIfNull();

        if (executionContext is null)
        {
            return function();
        }

        TResult? result = default;

        // See: System.Runtime.CompilerServices.AsyncMethodBuilderCore.MoveNextRunner.Run()
        ExecutionContext.Run(executionContext, _ => result = function(), null);
        return result!;
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static Task<TResult> InvokeWith<TResult>(this Func<TResult> function, SynchronizationContext? synchronizationContext, ExecutionContext? executionContext)
    {
        function.ThrowIfNull();

        TaskCompletionSource<TResult> taskCompletionSource = new();
        try
        {
            if (synchronizationContext is null)
            {
                TResult result = function.InvokeWith(executionContext);
                taskCompletionSource.SetResult(result);
            }
            else
            {
                // See: System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create()
                synchronizationContext.OperationStarted();

                // See: System.Threading.Tasks.SynchronizationContextAwaitTaskContinuation.PostAction()
                synchronizationContext.Post(
                    _ =>
                    {
                        try
                        {
                            TResult result = function.InvokeWith(executionContext);

                            // See: System.Runtime.CompilerServices.AsyncVoidMethodBuilder.NotifySynchronizationContextOfCompletion()
                            synchronizationContext.OperationCompleted();
                            taskCompletionSource.SetResult(result);
                        }
                        catch (Exception exception)
                        {
                            taskCompletionSource.SetException(exception);
                        }
                    }, 
                    null);
            }
        }
        catch (Exception exception)
        {
            taskCompletionSource.SetException(exception);
        }

        return taskCompletionSource.Task;
    }
}

public static class TaskExtensions
{
    public static Task<TNewResult> ContinueWithContext<TResult, TNewResult>(this Task<TResult> task, Func<Task<TResult>, TNewResult> continuation)
    {
        task.ThrowIfNull();
        continuation.ThrowIfNull();

        // See: System.Runtime.CompilerServices.AsyncMethodBuilderCore.GetCompletionAction()
        ExecutionContext? executionContext = ExecutionContext.Capture();

        // See: System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create()
        // See: System.Runtime.CompilerServices.AsyncMethodBuilderCore.MoveNextRunner.Run()
        SynchronizationContext? synchronizationContext = SynchronizationContext.Current;
        return task
            .ContinueWith(t =>
                new Func<TNewResult>(() => continuation(t)).InvokeWith(synchronizationContext, executionContext))
            .Unwrap();
    }

    public static Task<TNewResult> ContinueWithContext<TNewResult>(this Task task, Func<Task, TNewResult> continuation)
    {
        task.ThrowIfNull();
        continuation.ThrowIfNull();

        // See: System.Runtime.CompilerServices.AsyncMethodBuilderCore.GetCompletionAction()
        ExecutionContext? executionContext = ExecutionContext.Capture();

        // See: System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Create()
        // See: System.Runtime.CompilerServices.AsyncMethodBuilderCore.MoveNextRunner.Run()
        SynchronizationContext? synchronizationContext = SynchronizationContext.Current;
        return task
            .ContinueWith(t =>
                new Func<TNewResult>(() => continuation(t)).InvokeWith(synchronizationContext, executionContext))
            .Unwrap();
    }

    public static Task ContinueWithContext<TResult>(this Task<TResult> task, Action<Task<TResult>> continuation)
    {
        continuation.ThrowIfNull();

        return task.ThrowIfNull().ContinueWithContext(new Func<Task<TResult>, object?>(t =>
        {
            continuation(t);
            return null;
        }));
    }

    public static Task ContinueWithContext(this Task task, Action<Task> continuation)
    {
        continuation.ThrowIfNull();

        return task.ThrowIfNull().ContinueWithContext(new Func<Task, object?>(t =>
        {
            continuation(t);
            return null;
        }));
    }
}
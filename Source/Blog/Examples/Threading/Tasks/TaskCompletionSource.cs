namespace Examples.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    public class TaskCompletionSource
    {
        private readonly TaskCompletionSource<Unit?> taskCompletionSource;

        public TaskCompletionSource()
        {
            this.taskCompletionSource = new();
        }

        public TaskCompletionSource(object state)
        {
            this.taskCompletionSource = new(state);
        }

        public TaskCompletionSource(TaskCreationOptions creationOptions)
        {
            this.taskCompletionSource = new(creationOptions);
        }

        public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
        {
            this.taskCompletionSource = new(state, creationOptions);
        }

        public Task Task => this.taskCompletionSource.Task;

        public void SetCanceled() => this.taskCompletionSource.SetCanceled();

        public void SetException(Exception exception) => this.taskCompletionSource.SetException(exception);

        public void SetException(IEnumerable<Exception> exceptions) => this.taskCompletionSource.SetException(exceptions);

        public void SetComplete() => this.taskCompletionSource.SetResult(null);

        public bool TrySetCanceled() => this.taskCompletionSource.TrySetCanceled();

        public bool TrySetException(Exception exception) => this.taskCompletionSource.TrySetException(exception);

        public bool TrySetException(IEnumerable<Exception> exceptions) => this.taskCompletionSource.TrySetException(exceptions);

        public bool TrySetComplete() => this.taskCompletionSource.TrySetResult(null);
    }
}

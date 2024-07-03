using System;
using System.Threading.Tasks;

namespace LSCore.Async
{
    public struct LSTask
    {
        public static Action<LSTask<object>> WrapAction(Action<LSTask> action) => task => action(task);
        public static Action<LSTask<object>> WrapNullAction(Action<LSTask> action) => task => action?.Invoke(task);
        
        private TaskCompletionSource<object> source;
        private Task Task { get; set; }

#if DEBUG
        private string stackTrace;
#endif
        
        public bool IsSuccess => Task.IsCompletedSuccessfully;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsError => Task.IsFaulted;
        public bool IsCompleted => Task.IsCompleted;
        public Exception Exception => Task.Exception;

        private static LSTask completed;
        
        public static LSTask Completed
        {
            get
            {
                if (completed.source == null)
                {
                    completed = Create();
                    completed.Success();
                }

                return completed;
            }
        }
        
        public static LSTask Create()
        {
            var task = new LSTask();
            var source = new TaskCompletionSource<object>();
            task.source = source;
            task.Task = source.Task;
            task.success = source.TrySetResult;
            task.error = source.TrySetException;
            task.cancel = source.TrySetCanceled;
/*#if DEBUG
            task.stackTrace = UniTrace.Create();   
#endif*/
            return task;
        }
        
        public static LSTask Create(Task task)
        {
            var lsTask = new LSTask();
            lsTask.Task = task;
/*#if DEBUG
            lsTask.stackTrace = UniTrace.Create();   
#endif*/
            return lsTask;
        }

        internal Func<object, bool> success;
        internal Func<Exception, bool> error;
        internal Func<bool> cancel;
        
        public void Success() => success(default);
        public void Error(Exception exception) => error(exception);
        public void Cancel() => cancel();
        
        public void Error(string message = "") => Error(new Exception(message));
        public static implicit operator Task(LSTask task) => task.Task;

        public static implicit operator LSTask(Task task) => Create(task);
        
        public LSTask OnComplete(Action<LSTask> onComplete)
        {
            var copy = this;
            Task.OnComplete(_ => onComplete(copy));
            return this;
        }
        
        public LSTask SetupOnComplete(LSTask target)
        {
            return Task.SetupOnComplete(target);
        }
    }

    public struct LSTask<T>
    {
        private TaskCompletionSource<T> source;
        private Task<T> Task { get; set; }
        public T Result => Task.Result;
        public bool IsSuccess => Task.IsCompletedSuccessfully;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsError => Task.IsFaulted;
        public bool IsCompleted => Task.IsCompleted;
        public Exception Exception => Task.Exception;

#if DEBUG
        private string stackTrace;
#endif
        
        public static LSTask<T> GetCompleted(T data)
        {
            return System.Threading.Tasks.Task.FromResult(data);
        }
        
        public static LSTask<T> Create()
        {
            var task = new LSTask<T>();
            var source = new TaskCompletionSource<T>();
            task.source = source;
            task.Task = source.Task;
/*#if DEBUG
            task.stackTrace = UniTrace.Create();   
#endif*/
            return task;
        }
        
        public static LSTask<T> Create(Task<T> task)
        {
            var lsTask = new LSTask<T>();
            lsTask.Task = task;
/*#if DEBUG
            lsTask.stackTrace = UniTrace.Create();   
#endif*/
            return lsTask;
        }

        public void Success(T result) => source.TrySetResult(result);

        public void Error(string message = "") => source.TrySetException(new Exception(message));
        public void Error(Exception exception) => source.TrySetException(exception);

        public void Cancel() => source.TrySetCanceled();

        public static implicit operator Task<T>(LSTask<T> task) => task.Task;
        public static implicit operator LSTask<T>(Task<T> task) => Create(task);
        public static implicit operator LSTask(LSTask<T> task)
        {
            var lstask = LSTask.Create(task.Task);
            var source = task.source;
            lstask.success = _ => source.TrySetResult(default);
            lstask.error = source.TrySetException;
            lstask.cancel = source.TrySetCanceled;
            return lstask;
        }

        public LSTask<T> OnComplete(Action<LSTask<T>> onComplete)
        {
            var copy = this;
            Task.OnComplete(_ => onComplete(copy));
            return this;
        }

        public LSTask<T> SetupOnComplete(LSTask<T> target)
        {
            return Task.SetupOnComplete(target);
        }
        
        public LSTask SetupOnComplete(LSTask target)
        {
            return Task.SetupOnComplete(target);
        }
    }
    
    public static partial class Wait
    {
        public static LSTask OnComplete(this Task task, Action<LSTask> onComplete)
        {
            LSTask lsTask = task;
            if (task.IsCompleted)
            {
                onComplete(lsTask);
                return lsTask;
            }
            
            task.GetAwaiter().OnCompleted(() => onComplete(lsTask));
            return lsTask;
        }
        
        public static LSTask<T> OnComplete<T>(this Task<T> task, Action<LSTask<T>> onComplete)
        {
            LSTask<T> lsTask = task;
            if (task.IsCompleted)
            {
                onComplete(lsTask);
                return lsTask;
            }
            
            task.GetAwaiter().OnCompleted(() => onComplete(lsTask));
            return lsTask;
        }
        
        public static LSTask SetupOnComplete(this Task task, LSTask target)
        {
            task.OnComplete(result =>
            {
                if (result.IsSuccess) target.Success();
                else if (result.IsError) target.Error(result.Exception);
                else if (result.IsCanceled) target.Cancel();
            });
            
            return target;
        }
        
        public static LSTask SetupOnComplete<T>(this Task task, LSTask<T> target, T data)
        {
            task.SetupOnComplete(target, () => data);
            return target;
        }
        
        public static LSTask SetupOnComplete<T>(this Task task, LSTask<T> target, Func<T> data)
        {
            task.OnComplete(result =>
            {
                if (result.IsSuccess) target.Success(data());
                else if (result.IsError) target.Error(result.Exception);
                else if (result.IsCanceled) target.Cancel();
            });
            
            return target;
        }
        
        public static LSTask<T> SetupOnComplete<T>(this Task<T> task, LSTask<T> target)
        {
            task.SetupOnComplete(target, () => task.Result);
            return target;
        }
    }
}
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LSCore
{
    public static partial class LSAddressables
    {
        public static AsyncOperationHandle<T> OnComplete<T>(this in AsyncOperationHandle<T> task, Action<T> onComplete)
        {
            task.Completed += result => onComplete(result.Result);
            return task;
        }
        
        public static AsyncOperationHandle OnComplete(this in AsyncOperationHandle task, Action onComplete)
        {
            task.Completed += _ => onComplete();
            return task;
        }
        
        public static AsyncOperationHandle<T> OnComplete<T>(this in AsyncOperationHandle<T> task, Action onComplete)
        {
            task.Completed += _ => onComplete();
            return task;
        }
        
        public static AsyncOperationHandle<T> OnSuccess<T>(this in AsyncOperationHandle<T> task, Action onSuccess)
        {
            task.Completed += result  =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess();
                }
            };
            return task;
        }
        
        public static AsyncOperationHandle<T> OnSuccess<T>(this in AsyncOperationHandle<T> task, Action<T> onSuccess)
        {
            task.Completed += result  =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess(result.Result);
                }
            };
            return task;
        }

        public static AsyncOperationHandle<T> OnError<T>(this in AsyncOperationHandle<T> task, Action onError)
        {
            task.Completed += result =>
            {
                if (result.Status == AsyncOperationStatus.Failed)
                {
                    onError();
                }
            };
            return task;
        }
    }
}
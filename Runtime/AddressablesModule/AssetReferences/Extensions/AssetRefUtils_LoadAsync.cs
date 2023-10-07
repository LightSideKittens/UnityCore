using System;
using LightGamesCore.Runtime.AddressablesModule;
using LSCore.Async;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LSCore.AddressablesModule.AssetReferences
{
    public static partial class LGAddressables
    {
        public static AsyncOperationHandle<T> LoadAsync<T>(this AssetRef<T> reference) where T : Object
        {
            var isValid = reference.IsValid();
            var task = isValid
                ? reference.Task
                : reference.LoadAssetAsync().OnError(reference.ReleaseAsset);
#if DEBUG
            if (!isValid)
            {
                task.OnSuccess(asset => Burger.Log($"[Addressables] {asset.name} loaded"));
            }
#endif
            return task;
        }
        
        public static void Cache(this AssetReference reference, float reloadDelay = 1f, Action onSuccess = null, Action onError = null)
        {
            Addressables.DownloadDependenciesAsync(reference, true).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    onError?.Invoke();
                    Wait.Delay(reloadDelay, () => Cache(reference, reloadDelay, onSuccess, onError));
                }
                else
                {
                    onSuccess?.Invoke();
                }
            };
        }
        
        public static void CacheDependencies(this IDependecyContainer container, float reloadDelay = 1f,
                Action onComplete = null,
                Action onSuccess = null,
                Action onError = null)
        {
            var count = 0;
            var cachedCount = 0;
            
            foreach (var dependency in container.Dependencies)
            {
                count++;
                dependency.Cache(reloadDelay, () =>
                {
                    onSuccess?.Invoke();
                    cachedCount++;
                    if (count == cachedCount) onComplete?.Invoke();
                }, onError);
            }
        }

        public static void LoadLoop<T>(this AssetRef<T> reference, float delay = 0.2f, Action<T> onSuccess = null, Action onError = null) where T : Object
        {
            reference.LoadLoop(delay, onSuccess, () =>
            {
                onError?.Invoke();
                return true;
            });
        }
        
        public static void LoadLoop<T>(this AssetRef<T> reference, float delay, Action<T> onSuccess, Func<bool> onError) where T : Object
        {
#if DEBUG
            CustomLogHandler.Enable = true;
            CustomLogHandler.LogHandler
                .AddFilter("Cannot release")
                .AddFilter("System.Exception")
                .AddFilter("OperationException")
                .AddFilter("RemoteProviderException");
#endif
            
            reference.LoadAsync().OnSuccess(OnSuccess).OnError(Reload);

            void OnSuccess(T asset)
            {
#if DEBUG
                CustomLogHandler.LogHandler.ClearFilters();
                CustomLogHandler.Enable = false;
#endif
                onSuccess?.Invoke(asset);
            }
            
            void Reload()
            {
                if (onError())
                {
                    Wait.Delay(delay, () => reference.LoadLoop(delay, onSuccess, onError));
                }
            }
        }
        
        public static void TryLoad<T>(this AssetRef<T> reference, float attemptDelay = 0.2f, int maxAttepts = 3, Action<T> onSuccess = null, Action onError = null) where T : Object
        {
            var attempts = 0;
            reference.LoadLoop(attemptDelay, onSuccess, () =>
            {
                attempts++;
                var canContinue = attempts != maxAttepts;
                if(canContinue) onError?.Invoke();
                return canContinue;
            });
        }
    }
}
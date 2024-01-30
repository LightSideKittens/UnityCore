using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class LoadWithDependencies<T> : AsyncOperationBase<T> where T : Object
    {
        private int dependenciesCount;
        private int resolvedDependenciesCount;
        private bool hasError;
        private AssetRef<T> reference;
        private T asset;
        private Action<LoadWithDependencies<T>, T> onLoad;

        public AsyncOperationHandle<T> Task => Addressables.ResourceManager.StartOperation(this, default);

        private LoadWithDependencies() { }

        public static LoadWithDependencies<T> Create(AssetRef<T> reference, Action<LoadWithDependencies<T>, T> onLoad)
        {
            var task = new LoadWithDependencies<T>();
            task.reference = reference;
            task.onLoad = onLoad;
            
            Addressables.LoadAssetAsync<T>(reference.RuntimeKey).OnSuccess(task.OnLoad).OnError(task.Fail);
            return task;
        }

        private void OnLoad(T asset)
        {
            reference.SetAsset(asset);
            this.asset = asset;
            onLoad(this, asset);
        }

        public LoadWithDependencies<T> Add<T1>(AssetRef<T1> reference) where T1 : Object
        {
            dependenciesCount++;
            reference.LoadAsync().OnComplete(OnComplete).OnError(OnError);
            return this;
            void OnComplete(T1 _) => resolvedDependenciesCount++;
        }

        protected override void Execute()
        {
            World.Updated += Update;
        }

        private void Update()
        {
            if (dependenciesCount > 0 && dependenciesCount == resolvedDependenciesCount)
            {
                World.Updated -= Update;
                Complete(asset, !hasError, string.Empty);
            }
        }
        
        private void Fail() => Complete(asset, false, string.Empty);

        private void OnError()
        {
            hasError = true;
        }
    }
}
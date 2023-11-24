using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace LSCore.AddressablesModule.AssetReferences
{
    public class AssetRef<T> : AssetReferenceT<T> where T : Object
    {
        private Func<T> getter;
        private T asset;
        public AsyncOperationHandle<T> Task { get; private set; }
        protected AssetRef(string guid) : base(guid) { }
        
        public static implicit operator T(AssetRef<T> assetRef) => assetRef.getter();

        [Preserve]
        public override AsyncOperationHandle<T> LoadAssetAsync()
        { 
            getter = FirstGetter;
            Task = base.LoadAssetAsync();
            return Task;
        }

        public void SetAsset(T asset)
        {
            this.asset = asset;
            getter = DefaultGetter;
        }

        private T FirstGetter()
        {
            asset = Asset as T; 
            getter = DefaultGetter;
            return asset;
        }

        private T DefaultGetter() => asset;
    }
}
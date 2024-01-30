using System;
using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class AssetRef<T> : AssetReferenceT<T> where T : Object
    {
        private Func<T> getter;
        private T asset;
        public AsyncOperationHandle<T> Task { get; private set; }
        public AssetRef(string guid) : base(guid) { }
        public AssetRef() : base("") {}
        
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
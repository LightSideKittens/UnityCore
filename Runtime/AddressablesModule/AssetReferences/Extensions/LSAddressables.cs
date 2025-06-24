using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LSCore
{
    public static partial class LSAddressables
    {
        public static AsyncOperationHandle InitializationTask => Addressables.InitializeAsync();
        public static T Load<T>(object key) where T : Object => Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
        
        public static T Load<T>(this AssetReference reference) => reference.LoadAsync<T>().WaitForCompletion();
        public static T Load<T>(this AssetReferenceT<T> reference) where T : Object => reference.LoadAsync<T>().WaitForCompletion();
        public static T Load<T>(this AssRef reference) => reference.LoadAsync<T>().WaitForCompletion();
        public static T Load<T>(this AssRef<T> reference) where T : Object => reference.LoadAsync<T>().WaitForCompletion();
        
        
        public static AsyncOperationHandle<T> LoadAsync<T>(this AssetReference reference) => reference.LoadAssetAsync<T>();
        public static AsyncOperationHandle<T> LoadAsync<T>(this AssetReferenceT<T> reference) where T : Object => reference.LoadAssetAsync<T>();
        public static AsyncOperationHandle<T> LoadAsync<T>(this AssRef reference) => reference.LoadAssetAsync<T>();
        public static AsyncOperationHandle<T> LoadAsync<T>(this AssRef<T> reference) where T : Object => reference.LoadAssetAsync<T>();
        public static AsyncOperationHandle<T> LoadAsync<T>(this ComponentAssRef<T> reference) where T : Component => reference.LoadAssetAsync<T>();
    }
}
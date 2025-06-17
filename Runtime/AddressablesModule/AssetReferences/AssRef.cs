using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    public class AssRef : AssetReference, IEquatable<AssRef>
    {
        public override Object Asset
        {
            get
            {
                var handle = OperationHandle;
                if (handle.IsDone)
                {
                    return handle.Result as Object;
                }

                return handle.WaitForCompletion() as Object;
            }
        }
        
        public AssRef() { }
        public AssRef(string guid) : base(guid) { }
        
        public override int GetHashCode() => RuntimeKey == null ? 0 : RuntimeKey.GetHashCode();
        public static bool operator ==(AssRef left, AssRef right) => Equals(left, right);
        public static bool operator !=(AssRef left, AssRef right) => !Equals(left, right);

        public bool Equals(AssRef other)
        {
            if(other is null) return false;
            var key = other.RuntimeKey;
            return key != null && key.Equals(RuntimeKey);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return RuntimeKey == null || string.IsNullOrEmpty(RuntimeKey.ToString());
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AssRef)obj);
        }
    }
    
    [Serializable]
    public class AssRef<T> : AssetReferenceT<T>, IEquatable<AssRef<T>> where T : Object
    {
        public static implicit operator T(AssRef<T> reference)
        {
            var handle = reference.OperationHandle;
            if (handle.IsDone)
            {
                return handle.Result as T;
            }

            return handle.WaitForCompletion() as T;
        }

        public AssRef(string guid) : base(guid) { }
        
        public override int GetHashCode() => RuntimeKey == null ? 0 : RuntimeKey.GetHashCode();
        public static bool operator ==(AssRef<T> left, AssRef<T> right) => Equals(left, right);
        public static bool operator !=(AssRef<T> left, AssRef<T> right) => !Equals(left, right);

        public bool Equals(AssRef<T> other)
        {
            if(other is null) return false;
            var key = other.RuntimeKey;
            return key != null && key.Equals(RuntimeKey);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return RuntimeKey == null || string.IsNullOrEmpty(RuntimeKey.ToString());
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AssRef<T>)obj);
        }
        
        public override AsyncOperationHandle<T> LoadAssetAsync()
        {
            return base.LoadAssetAsync<T>();
        }
    }
    
    [Serializable]
    public class ComponentAssRef<T> : AssRef<GameObject> where T : Component
    {
        public static implicit operator T(ComponentAssRef<T> reference)
        {
            GameObject go = reference;
            return go.GetComponent<T>();
        }
        
        public ComponentAssRef(string guid) : base(guid) { }


        public override AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
        {
            var handle = Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync(), x =>
            {
                var comp = x.Result.GetComponent<TObject>();
                return Addressables.ResourceManager.CreateCompletedOperation(comp, string.Empty);
            });
            
            return handle;
        }
    }
}
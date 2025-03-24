using System;
using UnityEngine.AddressableAssets;

namespace LSCore
{
    [Serializable]
    public class LSAssetReference : AssetReference, IEquatable<LSAssetReference>
    {
        public LSAssetReference() { }
        public LSAssetReference(string guid) : base(guid) { }
        
        public override int GetHashCode() => RuntimeKey == null ? 0 : RuntimeKey.GetHashCode();
        public static bool operator ==(LSAssetReference left, LSAssetReference right) => Equals(left, right);
        public static bool operator !=(LSAssetReference left, LSAssetReference right) => !Equals(left, right);

        public bool Equals(LSAssetReference other)
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
            return Equals((LSAssetReference)obj);
        }
    }
}
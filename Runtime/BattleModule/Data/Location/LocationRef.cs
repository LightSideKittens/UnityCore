using System;
using LSCore;
using UnityEngine.AddressableAssets;

namespace LSCore.BattleModule
{
    [Serializable]
    public class LocationRef : AssetReferenceT<Location>
    {
        public LocationRef(string guid) : base(guid)
        {
        }
    }
}
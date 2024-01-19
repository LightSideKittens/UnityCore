using System;
using LSCore.AddressablesModule.AssetReferences;

namespace Battle.Data
{
    [Serializable]
    public class LocationRef : AssetRef<Location>
    {
        public LocationRef(string guid) : base(guid)
        {
            
        }
    }
}
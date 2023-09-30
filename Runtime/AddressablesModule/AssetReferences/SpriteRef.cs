using System;
using UnityEngine;

namespace LSCore.AddressablesModule.AssetReferences
{
    [Serializable]
    public class SpriteRef : AssetRef<Sprite>
    {
        public SpriteRef(string guid) : base(guid) { }
    }
}
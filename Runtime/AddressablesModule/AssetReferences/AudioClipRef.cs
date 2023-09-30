using System;
using UnityEngine;

namespace LSCore.AddressablesModule.AssetReferences
{
    [Serializable]
    public class AudioClipRef : AssetRef<AudioClip>
    {
        public AudioClipRef(string guid) : base(guid) { }
    }
}
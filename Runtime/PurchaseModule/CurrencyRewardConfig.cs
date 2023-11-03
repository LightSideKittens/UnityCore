using System;
using LSCore.AddressablesModule.AssetReferences;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore
{
    public class CurrencyRewardConfig : ScriptableObject, IReward
    {
        public string title;    // TODO: Localization
        public SpriteRef preview;
        public string description;

        [OdinSerialize] 
        [HideReferenceObjectPicker]
        public Fund Fund { get; set; } = new();
        
        public bool Claim(out Action claim)
        {
            claim = Fund.Earn;
            return true;
        }
    }
}
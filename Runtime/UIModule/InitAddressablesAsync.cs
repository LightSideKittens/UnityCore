using System;
using UnityEngine;

namespace LSCore.AddressablesModule
{
    [Serializable]
    public class InitAddressablesAsync : DoIt
    {
        [SerializeReference] public DoIt[] onComplete;
        
        public override void Do()
        {
            LSAddressables.InitializationTask.Completed += onComplete.Do;
        }
    }
}
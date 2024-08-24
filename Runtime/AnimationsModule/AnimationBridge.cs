using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class AnimationBridge : MonoBehaviour
    {
        [Serializable]
        private struct Data
        {
            
        }
        
        [SerializeField] private Animation animation;
        
        [SerializeReference] public List<LSAction> actions;

        private void OnDidApplyAnimationProperties()
        {
            
        }
    }
}
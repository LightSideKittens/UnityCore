using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class PlayAnimationWrapper : LSAction
    {
        public AnimationWrapper wrapper;
        
        [ValueDropdown("Animations")]
        public string animation;
        public PlayMode playMode = PlayMode.StopSameLayer;
        
        public IEnumerable<string> Animations
        {
            get
            {
                if (wrapper == null)
                {
                    yield break;
                }
                
                foreach (AnimationState state in wrapper.Animation)
                {
                    yield return state.name;
                }
            }
        }
        
        public override void Invoke()
        {
            wrapper.Play(animation, playMode);
        }
    }
}
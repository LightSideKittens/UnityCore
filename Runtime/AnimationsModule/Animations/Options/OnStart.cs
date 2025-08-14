using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct OnStart : IOption
    {
        [SerializeReference] private List<DoIt> actions;
        
        public void ApplyTo(Tween tween)
        {
            tween.OnStart(actions.Do);
        }
    }
}
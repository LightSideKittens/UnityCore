using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct SetUpdate : IOption
    {
        [SerializeField] private UpdateType updateType;
        
        public void ApplyTo(Tween tween)
        {
            tween.SetUpdate(updateType);
        }
    }
}
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct EaseOption : IOptions
    {
        [SerializeField] private bool useCustom;

        [ShowIf(nameof(useCustom))]
        [SerializeField]
        private AnimationCurve curve;
        
        [HideIf(nameof(useCustom))]
        [SerializeField]
        private Ease ease;
        
        public void ApplyTo(Tween tween)
        {
            if (useCustom)
            {
                tween.SetEase(curve);
            }
            else
            {
                tween.SetEase(ease);
            }
        }
    }
}
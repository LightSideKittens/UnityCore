using System;
using System.Collections.Generic;
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
        [ValueDropdown(nameof(CurveNames))]
        private string curveName;
        
        [HideIf(nameof(useCustom))]
        [SerializeField]
        private Ease ease;

        private static IEnumerable<string> CurveNames => AnimationCurves.Names;

        public void ApplyTo(Tween tween)
        {
            if (useCustom)
            {
                tween.SetEase(AnimationCurves.Get(curveName));
            }
            else
            {
                tween.SetEase(ease);
            }
        }
    }
}
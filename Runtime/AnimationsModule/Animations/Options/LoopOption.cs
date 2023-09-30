using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct LoopOption : IOptions
    {
        [SerializeField] private int loopsCount;
        [SerializeField] private LoopType loopType;

        public void ApplyTo(Tween tween)
        {
            tween.SetLoops(loopsCount, loopType);
        }
    }
}
using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct SetLoops : IOption
    {
        [SerializeField] private int loopsCount;
        [SerializeField] private LoopType loopType;

        public void ApplyTo(Tween tween)
        {
            tween.SetLoops(loopsCount, loopType);
        }
    }
}
using System;
using DG.Tweening;
using LSCore.Attributes;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class DefaultUIViewAnimation : ShowHideAnim
    {
        [SerializeField, GetComponent] private CanvasGroup canvasGroup;
        [SerializeField] private float duration = 0.2f;
        public override void Init() { }

        public override Tween Show => canvasGroup.DOFade(1, duration);
        public override Tween Hide => canvasGroup.DOFade(0, duration);
    }
}
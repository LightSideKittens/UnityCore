using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class StandaloneSubmittableAnim : BaseSubmittableAnim
    {
        private Transform transform;
        private Tween current;
        private Vector3 defaultScale;
        private Vector3 targetScale;
        private Vector3 scaleModification;
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            defaultScale = transform.localScale;
            targetScale = defaultScale;
            Submittable.States.HoverChanged += OnHover;
            Submittable.States.PressChanged += OnPress;
            Submittable.States.SelectChanged += OnSelect;
            Submittable.Submitted += OnSubmit;
        }

        private void OnSelect(bool isSelected)
        {
            float duration = 0.3f;
            scaleModification += defaultScale * (0.1f * (isSelected ? 1 : -1));
            AnimScale(duration);
        }

        private void OnPress(bool isPressing)
        {
            float duration;
            if (isPressing)
            {
                duration = 0.3f;
                targetScale = defaultScale * 0.8f;
            }
            else
            {
                duration = 0.15f;
                targetScale = defaultScale;
            }

            AnimScale(duration);
        }
        
        private void OnHover(bool isHovering)
        {
            float duration = 0.15f;
            scaleModification += defaultScale * (0.1f * (isHovering ? 1 : -1));
            AnimScale(duration);
        }
        
        private void OnSubmit()
        {
            targetScale = defaultScale;
            AnimScale(0.5f).SetEase(Ease.OutElastic);
        }

        public override void OnDisable()
        {
            current.Kill();
            if (current == null) return;
            transform.localScale = defaultScale;
            current = null;
        }
        
        private Tween AnimScale(float duration)
        {
            current.Kill();
            current = transform.DOScale(targetScale + scaleModification, duration);
            return current;
        }
    }
}
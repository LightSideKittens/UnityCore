using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    internal struct ClickAnim
    {
        private Transform transform;
        private Tween current;
        private Vector3 defaultScale;

        public ClickAnim(Transform transform) : this()
        {
            this.transform = transform;
        }
        
        public void OnPointerDown()
        {
            current.Complete();
            defaultScale = transform.localScale;
            current = transform.DOScale(defaultScale * 0.8f, 0.3f);
        }
        
        public void OnPointerClick()
        {
            current.Kill();
            current = transform.DOScale(defaultScale, 0.5f).SetEase(Ease.OutElastic);
        }
        
        public void OnPointerUp()
        {
            current.Kill();
            current = transform.DOScale(defaultScale, 0.15f);
        }

        public void OnDisable()
        {
            current.Kill();
        }
    }
}
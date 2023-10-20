using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSCore
{
    public class LSButton : Button
    {
        private Tween current;
        private Vector3 lastScale;
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            current?.Complete();
            lastScale = transform.localScale;
            current = transform.DOScale(-0.2f * Mathf.Abs(lastScale.x), 0.3f).SetRelative();
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            current?.Complete();
            current = transform.DOScale(0.2f * Mathf.Abs(lastScale.x), 0.5f).SetEase(Ease.OutElastic).SetRelative();
        }
    }
}
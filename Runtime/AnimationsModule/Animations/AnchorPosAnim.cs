using System;
using DG.Tweening;
using LSCore.ReferenceFrom.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AnchorPosAnim : BaseAnim<Vector2, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            target.anchoredPosition = startValue;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOAnchorPos(endValue, Duration);
        }
    }
    
    [Serializable]
    public class LocalPosAnim : BaseAnim<Vector3, Transform>
    {
        protected override void InitAction(Transform target)
        {
            target.localPosition = startValue;
        }

        protected override Tween AnimAction(Transform target)
        {
            return target.DOLocalMove(endValue, Duration);
        }
    }
    
    [Serializable]
    public class LocalPosByTransformAnim : BaseAnim<Transform, Transform>
    {
        public bool useValuePath;
        [HideIf("ShowStartValue")]public string startValuePath;
        [HideIf("ShowEndValue")]public string endValuePath;

        protected override bool ShowStartValue => !useValuePath;
        protected override bool ShowEndValue => base.ShowEndValue && !useValuePath;

        protected override void InitAction(Transform target)
        {
            target.localPosition = startValue.localPosition;
        }

        protected override Tween AnimAction(Transform target)
        {
            return target.DOLocalMove(endValue.localPosition, Duration);
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            if(!World.IsPlaying) return;
            if (useValuePath)
            {
                startValue = root.FindComponent<Transform>(startValuePath);
                endValue = root.FindComponent<Transform>(endValuePath);
            }
        }
    }
}
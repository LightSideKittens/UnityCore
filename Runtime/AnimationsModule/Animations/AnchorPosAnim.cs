using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AnchorPosAnim : BaseAnim<Vector2, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            //SetSatrtValue(() => target.anchoredPosition, x => target.anchoredPosition = x, (a, b) => a + b);
            target.anchoredPosition = startValue;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOAnchorPos(endValue, duration);
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
            return target.DOLocalMove(endValue, duration);
        }
    }
    
        
    [Serializable]
    public class LocalCurveAnim : LocalPosAnim
    {
        [Serializable]
        public class CurveData
        {
            public MoveItCurve curve;
            public Vector2 randomRange = new(1, 1);
            public MoveItCurve.EvaluateMode mode;
            
            public float Factor => Random.Range(randomRange.x, randomRange.y);
            public float Evaluate(float x) => curve.Evaluate(x, mode);
        }
        
        public CurveData xCurve;
        public CurveData yCurve;
        public CurveData zCurve;
        
        protected override void InitAction(Transform target)
        {
            target.localPosition = startValue;
        }

        protected override Tween AnimAction(Transform target)
        {
            var startPos = target.localPosition;
            var dir = endValue - startPos;
            var dirMagnitude = dir.magnitude;
            var dirNorm = dirMagnitude > 0.0001f ? dir / dirMagnitude : Vector3.forward;
            var xFactor = xCurve.Factor;
            var yFactor = yCurve.Factor;
            var zFactor = zCurve.Factor;
            
            return DOTween.To(() => 0f, t =>
            {
                var basePos = Vector3.Lerp(startPos, endValue, t);

                var offset = new Vector3(
                    xCurve.Evaluate(t),
                    yCurve.Evaluate(t),
                    zCurve.Evaluate(t)
                );
                
                offset.x *= xFactor;
                offset.y *= yFactor;
                offset.z *= zFactor;
                
                var projection = Vector3.Dot(offset, dirNorm);
                var offsetPerp = offset - dirNorm * projection;
                offsetPerp += offset.y * dirNorm;
                target.localPosition = basePos + offsetPerp;
            }, 1f, duration).SetTarget(target);
        }
    }
}
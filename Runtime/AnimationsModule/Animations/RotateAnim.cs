using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class RotateAnim : BaseAnim<Vector3, Transform>
    {
        public bool useLocalSpace;

        protected override void InitAction(Transform target)
        {
            if (useLocalSpace)
            {
                target.localEulerAngles = startValue;
                return;
            }
            
            target.eulerAngles = startValue;
        }
        
        protected override Tween AnimAction(Transform target)
        {
            return useLocalSpace ? target.DOLocalRotate(endValue,Duration) : target.DORotate(endValue,Duration);
        }
    }
}
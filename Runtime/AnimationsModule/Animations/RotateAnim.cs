using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class RotateAnim : BaseAnim<Vector3>
    {
        public Transform target;
        public bool useLocalSpace;

        protected override void Internal_Init()
        {
            if (useLocalSpace)
            {
                target.localEulerAngles = startValue;
                return;
            }
            
            target.eulerAngles = startValue;
        }
        
        protected override Tween Internal_Animate()
        {
            return useLocalSpace ? target.DOLocalRotate(endValue, duration) : target.DORotate(endValue, duration);
        }
    }
}
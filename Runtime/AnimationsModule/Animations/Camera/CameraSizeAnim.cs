using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class CameraSizeAnim : BaseAnim<float>
    {
        public Camera target;

        protected override void Internal_Init()
        {
            target.orthographicSize = startValue;
        }

        protected override Tween Internal_Animate()
        {
            return target.DOOrthoSize(endValue, duration);
        }
    }
}
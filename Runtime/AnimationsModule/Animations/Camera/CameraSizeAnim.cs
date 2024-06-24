using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class CameraSizeAnim : BaseAnim<float, Camera>
    {
        protected override void InitAction(Camera target)
        {
            target.orthographicSize = startValue;
        }

        protected override Tween AnimAction(Camera target)
        {
            return target.DOOrthoSize(endValue,Duration);
        }
    }
}
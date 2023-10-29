using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public static Tween Run(in float time, TweenCallback<float> update) => DOVirtual.Float(0, 1, time, update);
        public static Tween Run(in float time, TweenCallback update) =>  DOTween.Sequence().AppendInterval(time).OnUpdate(update);
        public static Tween Delay(in float time, TweenCallback onComplete) => DOTween.Sequence().AppendInterval(time).OnComplete(onComplete);
        public static Tween InfinityLoop(in float delay, TweenCallback onLoop) => DOTween.Sequence().AppendInterval(delay).SetLoops(-1).OnStepComplete(onLoop);
        public static Tween Frames(int count, Action onComplete)
        {
            var current = 0;
            var tween = DOTween.Sequence().AppendInterval(float.MaxValue);
            tween.OnUpdate(() =>
            {
                if (current >= count)
                {
                    tween.Kill();
                    onComplete();
                }
                
                current++;
            });
            
            return tween;
        }
    }
}
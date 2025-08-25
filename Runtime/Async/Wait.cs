using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public static Tween Run(in float time, TweenCallback<float> update) => DOVirtual.Float(0, 1, time, update).SetEase(Ease.Linear);
        public static Tween InverseRun(in float time, TweenCallback<float> update) => DOVirtual.Float(1, 0, time, update).SetEase(Ease.Linear);
        public static Tween Run(in float time, TweenCallback update) =>  DOTween.Sequence().AppendInterval(time).OnUpdate(update).SetEase(Ease.Linear);
        public static Tween FromTo(float from, float to, float time, TweenCallback<float> update) =>  DOVirtual.Float(from, to, time, update).SetEase(Ease.Linear);
        public static Tween TimerForward(in float time, TweenCallback<float> update) => DOVirtual.Float(0, time, time, update).SetEase(Ease.Linear);
        public static Tween TimerBack(in float time, TweenCallback<float> update) => DOVirtual.Float(time, 0, time, update).SetEase(Ease.Linear);
        public static Tween Delay(in float time, TweenCallback onComplete) => Delay(time).OnComplete(onComplete);
        public static Tween Delay(in float time) => DOTween.To(plugFrom, plugTo, 1, time);
        private static float plugFrom() => 0f;
        private static void plugTo(float t){}
        public static Tween Cycles(in float delay, int cycles, TweenCallback onLoop) => DOTween.Sequence().AppendInterval(delay).SetLoops(cycles).OnStepComplete(onLoop);
        public static Tween InfinityLoop(in float delay, TweenCallback onLoop) => Cycles(delay, -1, onLoop);
        public static Tween Seconder(this TimeSpan time, Action<TimeSpan> onLoop, bool callImmediately = true)
        {
            if (callImmediately)
            {
                onLoop(time);
            }
            
            return Cycles(1, -1, () =>
            {
                time -= TimeSpan.FromSeconds(1);
                onLoop(time);
            });
        }

        public static Tween Frames(uint count, Action onComplete)
        {
            var current = 0;
            return While(() => current++ < count, onComplete);
        }

        public static Tween Coroutine(CustomYieldInstruction instruction, Action onComplete) => While(() => instruction.keepWaiting, onComplete);

        public static Tween While(Func<bool> predicate, Action onComplete)
        {
            var tween = DOTween.Sequence().AppendInterval(float.MaxValue);
            tween.OnUpdate(Update);
            
            return tween;

            void Update()
            {
                if (!predicate())
                {
                    tween.Kill();
                    onComplete();
                }
            }
        }
    }
}
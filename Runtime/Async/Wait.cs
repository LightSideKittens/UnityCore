using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public static Task OnComplete(this Task task, Action<Task> onComplete)
        {
            World.Updated += Update;
            return task;

            void Update()
            {
                if (!task.IsCompleted) return;
                World.Updated -= Update;
                onComplete(task);
            }
        }
        
        public static Tween Run(in float time, TweenCallback<float> update) => DOVirtual.Float(0, 1, time, update);
        public static Tween Run(in float time, TweenCallback update) =>  DOTween.Sequence().AppendInterval(time).OnUpdate(update);
        public static Tween Delay(in float time, TweenCallback onComplete) => DOTween.Sequence().AppendInterval(time).OnComplete(onComplete);
        public static Tween InfinityLoop(in float delay, TweenCallback onLoop) => DOTween.Sequence().AppendInterval(delay).SetLoops(-1).OnStepComplete(onLoop);
        public static Coroutine Frames(in int count, Action onComplete) => World.RunCoroutine(WaitFrames(count, onComplete));
        public static WaitTime Time(in float time) => new (time);

        private static IEnumerator WaitFrames(int count, Action onComplete)
        {
            for (int i = 0; i < count; i++)
            {
                yield return null;
            }

            onComplete();
        }
    }

    public class WaitTime : CustomYieldInstruction
    {
        public float time;
        public float current;
        public float Remain => time - current;
        public bool isRunning;
        
        internal WaitTime(float time)
        {
            this.time = time;
        }

        public override bool keepWaiting => current >= time;

        public void Run()
        {
            World.RunCoroutine(Routine());
        }
        
        private IEnumerator Routine()
        {
            if (isRunning) yield break;
            isRunning = true;
            
            yield return this;
            
            time = 0;
            current = 0;
            isRunning = false;
        }
    }
}
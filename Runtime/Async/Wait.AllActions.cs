using System;
using UnityEngine;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public static WhenAllActions AllActions(bool startManual = false) => new(startManual);

        public static WhenAllActions AllActions(Action onComplete, bool startManual = false) => new(onComplete, startManual);
        
        public partial class WhenAllActions
        {
            public event Action Completed;
            private int dependenciesCount;
            private int resolvedDependenciesCount;
            private bool started;

            internal WhenAllActions(bool startManual)
            {
                SetCreatePlace();
                TryStart(startManual);
            }

            internal WhenAllActions(Action onComplete, bool startManual)
            {
                SetCreatePlace();
                Completed += onComplete;
                TryStart(startManual);
            }

            partial void SetCreatePlace();
            partial void RegisterAction(string place);
            partial void UnRegisterAction(string place);

            private void TryStart(bool startManual)
            {
                if (startManual) return;
                Start();
            }

            public void Start()
            {
                if (started)
                {
                    Burger.Warning($"[{nameof(WhenAllActions)}] Is already started");
                    return;
                }

                started = true;
                World.Updated += Update;
            }

            public void OnComplete(Action onComlete) => Completed += onComlete;

            private void Update()
            {
                if (resolvedDependenciesCount >= dependenciesCount)
                {
                    World.Updated -= Update;
                    Completed();
                }
            }

            public Action WaitAction()
            {
                string trace = UniTrace.Create(1);
                dependenciesCount++;
                RegisterAction(trace);

                return OnResolved;
                void OnResolved()
                {
                    UnRegisterAction(trace);
                    resolvedDependenciesCount++;
                }
            }
        }
    }
}
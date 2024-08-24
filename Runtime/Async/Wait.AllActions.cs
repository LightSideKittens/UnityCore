using System;
using UnityEngine;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public static WhenAllActions AllActions() => new();

        public static WhenAllActions AllActions(Action onComplete) => new(onComplete);
        
        public partial class WhenAllActions
        {
            public event Action Completed;
            private int dependenciesCount;
            private int resolvedDependenciesCount;

            internal WhenAllActions()
            {
                SetCreatePlace();
            }

            internal WhenAllActions(Action onComplete)
            {
                SetCreatePlace();
                Completed += onComplete;
            }

            partial void SetCreatePlace();
            partial void RegisterAction(string place);
            partial void UnRegisterAction(string place);
            
            public void OnComplete(Action onComlete) => Completed = onComlete;

            public Action WaitAction()
            {
                dependenciesCount++;
#if UNITY_EDITOR
                string trace = UniTrace.Create(1);
                RegisterAction(trace);
#endif


                return OnResolved;
                void OnResolved()
                {
#if UNITY_EDITOR
                    UnRegisterAction(trace);
#endif
                    resolvedDependenciesCount++;

                    if (resolvedDependenciesCount == dependenciesCount)
                    {
                        Completed?.Invoke();
                    }
                }
            }
        }
    }
}
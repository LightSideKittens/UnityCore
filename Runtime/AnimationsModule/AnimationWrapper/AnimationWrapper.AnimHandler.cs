using System;
using Sirenix.OdinInspector;
using UnityEditor;

namespace LSCore.AnimationsModule
{
    public partial class AnimationWrapper
    {
        [Serializable]
        public abstract class  Handler
        {
            protected bool forceHandle;
            private bool isStarted;
            
            protected virtual bool CanUse => true;
            
            public void Start()
            {
#if UNITY_EDITOR
                if (World.IsEditMode || !CanUse)
                {
                    return;
                }
#endif
                if (!isStarted)
                {
                    isStarted = true;
                    forceHandle = true;
                    OnStart();
                }
            }
            
            public void Stop()
            {
#if UNITY_EDITOR
                if (World.IsEditMode || !CanUse)
                {
                    return;
                }
#endif
                
                if (isStarted)
                {
                    isStarted = false;
                    OnStop();
                }
            }

            public abstract void Handle();
            protected virtual void OnStart(){}
            protected virtual void OnStop(){}

#if UNITY_EDITOR
            [ShowInInspector] private bool gizmos;
            
            public void DrawGizmos()
            {
                if (gizmos)
                {
                    OnDrawGizmos();
                }
            }
            
            protected virtual void OnDrawGizmos(){}
            public virtual void OnSceneGUI(){}
#endif
        }
        
        [Serializable]
        public abstract class Handler<T> : Handler
        {
            [LabelText("$Label")]
            public T value;
            protected T lastValue;

            protected abstract string Label { get; }
            protected virtual bool IsEquals => value.Equals(lastValue);

            public sealed override void Handle()
            {
#if UNITY_EDITOR
                if (World.IsEditMode || !CanUse)
                {
                    return;
                }
#endif
                if (forceHandle)
                {
                    forceHandle = false;
                    goto handle;
                }
                
                if (IsEquals) return;
                
                handle:
                OnHandle();
                lastValue = value;
            }
            
            protected abstract void OnHandle();
        }
    }
}
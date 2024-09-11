using System;
using Sirenix.OdinInspector;

namespace LSCore.AnimationsModule
{
    public partial class AnimationWrapper
    {
        [Serializable]
        public abstract class  Handler
        {
            protected bool forceHandle;
            private bool isStarted;

            public void Start()
            {
                if (!isStarted)
                {
                    isStarted = true;
                    forceHandle = true;
                    OnStart();
                }
            }
            
            public void Stop()
            {
                if (isStarted)
                {
                    isStarted = false;
                    OnStop();
                }
            }

            public abstract void Handle();
            protected virtual void OnStart(){}
            protected virtual void OnStop(){}
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
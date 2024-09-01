using System;
using Sirenix.OdinInspector;

namespace LSCore.AnimationsModule
{
    public partial class AnimationBridge
    {
        [Serializable]
        public abstract class Handler
        {
            public virtual void Start(){}
            public abstract void Handle();
            public virtual void Stop(){}
        }
        
        [Serializable]
        public abstract class Handler<T> : Handler
        {
            [LabelText("$Lable")]
            public T value;
            private T lastValue;

            protected abstract string Lable { get; }
            protected virtual bool IsEquals => value.Equals(lastValue);

            public sealed override void Handle()
            {
                if (IsEquals) return;
                OnHandle();
                lastValue = value;
            }
            
            protected abstract void OnHandle();
        }
    }
}
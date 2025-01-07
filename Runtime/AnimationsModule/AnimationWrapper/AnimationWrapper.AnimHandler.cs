using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public partial class BadassAnimation
{
    [Serializable]
    public abstract class Handler
    {
        [GenerateGuid(Hide = true)]
        public string guid;
        [NonSerialized] public Dictionary<string, EvaluateData> evaluators = new();
        
        protected bool forceHandle;
        private bool isStarted;
#if UNITY_EDITOR    
        protected virtual bool CanUse => true;
#endif
        
        public void Start()
        {
#if UNITY_EDITOR
            if (!CanUse)
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
            if (!CanUse)
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
        public abstract Type ValueType { get; }
        public abstract string HandlerName { get; }
        
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
        
#if UNITY_EDITOR
        public override Type ValueType => typeof(T);
        public override string HandlerName => GetType().Name;
#endif
        
        public sealed override void Handle()
        {
#if UNITY_EDITOR
            if (!CanUse)
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

using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseComp
    {
        protected CompData data;
        protected Transform transform;
        
        protected bool useUpdate;
        protected bool useFixedUpdate;
#if UNITY_EDITOR
        private int lastWorldInstanceId;
#endif
        private bool isRegistered;
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (value == isRunning) return;
                
                isRunning = value;
                if (value)
                {
                    if(useUpdate) data.update += Update;
                    if (useFixedUpdate) data.fixedUpdate += FixedUpdate;
                }
                else
                {
                    data.update -= Update;
                    data.fixedUpdate -= FixedUpdate;
                }
            }
        }

        public void SetIsRunning(bool value) => IsRunning = value;
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }

        public void Init(CompData data)
        {
            Register(data);
            Init();
        }

        public void Register(CompData data)
        {
#if UNITY_EDITOR
            var instanceId = World.InstanceId;
            if (lastWorldInstanceId != instanceId)
            {
                lastWorldInstanceId = instanceId;
                isRegistered = false;
            }
#endif
            if(isRegistered) return;

            this.data = data;
            transform = data.transform;
            OnRegister();
            isRegistered = true;
        }

        protected void Reg<T>(T obj) where T : BaseComp
        {
            TransformDict<T>.Add(transform, obj);
            data.destroy += data.Remove<T>;
        }
        
        protected virtual void OnRegister() { }
        
        protected virtual void Init() {}
    }
}
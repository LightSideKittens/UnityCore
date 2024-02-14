using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseComp
    {
        protected CompData data;
        protected Transform transform;
        
        public void Init(Transform transform, CompData data)
        {
            this.data = data;
            Register(transform);
            data.destroy += UnRegister;
            Init();
        }

        public void Register(Transform transform)
        {
            this.transform = transform;
            OnRegister();
        }

        public abstract void UnRegister();
        protected abstract void OnRegister();
        protected abstract void Init();
    }
}
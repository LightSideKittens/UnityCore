using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseComp
    {
        protected CompData data;
        protected Transform transform;
        
        public void Init(CompData data)
        {
            Register(data);
            Init();
        }

        public void Register(CompData data)
        {
            this.data = data;
            transform = data.transform;
            OnRegister();
        }

        protected void Reg<T>(T obj) where T : BaseComp
        {
            TransformDict<T>.Add(transform, obj);
            data.destroy += data.Remove<T>;
        }
        
        protected virtual void OnRegister() { }
        
        protected abstract void Init();
    }
}
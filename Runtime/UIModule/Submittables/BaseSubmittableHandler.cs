using System;

namespace LSCore
{
    [Serializable]
    public abstract class BaseSubmittableHandler
    {
        public ISubmittable Submittable { get; private set; }

        public void Init(ISubmittable submittable)
        {
            Submittable = submittable;
            Init();
        }
        
        protected virtual void Init(){}
        public virtual void OnEnable(){}
        public virtual void OnDisable(){}
    }
}
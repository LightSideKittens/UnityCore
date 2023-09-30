using System;

namespace LSCore.ConditionModule
{
    [Serializable]
    public abstract class BaseCondition
    {
        private Func<bool> checker;
        protected abstract bool IsCompleted { get; set; }

        protected BaseCondition()
        {
            checker = FirstCheck;
        }

        public static implicit operator bool(BaseCondition conditions) => conditions.checker();

        protected virtual void Init(){}

        private bool FirstCheck()
        {
            Init();
            checker = Check;
            return IsCompleted;
        }
        
        private bool Check() => IsCompleted;
        
        public virtual void Reset()
        {
            
        }
    }
}
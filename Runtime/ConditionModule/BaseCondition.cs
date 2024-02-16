using System;

namespace LSCore.ConditionModule
{
    public abstract class BaseCondition
    {
        public enum ConditionType
        {
            If,
            And,
            Or,
        }
        
        private Func<bool> checker;

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
            return checker();
        }

        protected internal abstract bool Check();
        
        public virtual void Reset()
        {
            
        }
    }
}
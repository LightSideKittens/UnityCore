using System;

namespace LSCore.ConditionModule
{
    public abstract class BaseCondition
    {
        public enum ConditionType
        {
            And,
            Or,
        }

        public bool not;
        private Func<bool> checker;

        protected BaseCondition()
        {
            checker = FirstCheck;
        }

        public static implicit operator bool(BaseCondition conditions) => conditions.checker();

        private bool FirstCheck()
        {
            checker = FullCheck;
            return checker();
        }

        private bool FullCheck() => Check() ^ not;
        protected internal abstract bool Check();
    }
}
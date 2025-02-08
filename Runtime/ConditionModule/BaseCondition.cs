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

        public static implicit operator bool(BaseCondition conditions) => conditions.FullCheck();
        private bool FullCheck() => Check() ^ not;
        protected internal abstract bool Check();
    }
}
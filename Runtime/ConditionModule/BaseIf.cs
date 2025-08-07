namespace LSCore.ConditionModule
{
    public abstract class BaseIf
    {
        public enum ConditionType
        {
            And,
            Or,
        }

        public bool not;

        public static implicit operator bool(BaseIf ifs) => ifs.FullCheck();
        private bool FullCheck() => Check() ^ not;
        protected internal abstract bool Check();
    }
}
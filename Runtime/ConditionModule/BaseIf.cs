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

        public static implicit operator bool(BaseIf ifs)
        {
            if(ifs == null) return true;
            return ifs.Check() ^ ifs.not;
        }
        
        protected internal abstract bool Check();
    }
}
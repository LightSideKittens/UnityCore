namespace LSCore.ConditionModule
{
    public static class ConditionsBuilder
    {
        public static ConditionsContainer If<T>() where T : BaseCondition, new()
        {
            return new ConditionsContainer(new T());
        }

        public static ConditionsContainer AndIf<T>(this ConditionsContainer conditions) where T : BaseCondition, new()
        {
            var newContainer = new ConditionsContainer(conditions);
            conditions.And<T>();
            
            return newContainer;
        }

        public static ConditionsContainer OrIf<T>(this ConditionsContainer conditions) where T : BaseCondition, new()
        {
            var newContainer = new ConditionsContainer(conditions);
            conditions.Or<T>();
            
            return newContainer;
        }
        
        public static ConditionsContainer If(BaseCondition condition)
        {
            return new ConditionsContainer(condition);
        }
        
        public static ConditionsContainer AndIf(this ConditionsContainer conditions, BaseCondition condition)
        {
            var newContainer = new ConditionsContainer(conditions);
            conditions.And(condition);
            
            return newContainer;
        }

        public static ConditionsContainer OrIf(this ConditionsContainer conditions, BaseCondition condition)
        {
            var newContainer = new ConditionsContainer(conditions);
            conditions.Or(condition);
            
            return newContainer;
        }
    }
}
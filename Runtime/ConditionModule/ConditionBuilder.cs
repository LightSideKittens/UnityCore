using System;
using System.Collections.Generic;

namespace LSCore.ConditionModule
{
    public class ConditionBuilder : BaseCondition
    {
        public static ConditionBuilder Default { get; } = If(() => false);
        private readonly List<Data> data = new();
        
        public struct Data
        {
            public ConditionType type;
            public Func<bool> condition;
        }
        
        private ConditionBuilder(){}
        
        public static ConditionBuilder If(Func<bool> condition)
        {
            var builder = new ConditionBuilder();
            builder.data.Add(new Data {type = default, condition = condition});
            return builder;
        }

        public ConditionBuilder And(Func<bool> condition)
        {
            data.Add(new Data {type = ConditionType.And, condition = condition});
            return this;
        }

        public ConditionBuilder Or(Func<bool> condition)
        {
            data.Add(new Data {type = ConditionType.Or, condition = condition});
            return this;
        }
        
        public static ConditionBuilder If(BaseCondition condition)
        {
            var builder = new ConditionBuilder();
            builder.data.Add(new Data {type = default, condition = condition.Check});
            return builder;
        }

        public ConditionBuilder And(BaseCondition condition)
        {
            data.Add(new Data {type = ConditionType.And, condition = condition.Check});
            return this;
        }

        public ConditionBuilder Or(BaseCondition condition)
        {
            data.Add(new Data {type = ConditionType.Or, condition = condition.Check});
            return this;
        }
        
        public ConditionBuilder Add<T>(T condition) where T : Condition
        {
            data.Add(new Data {type = condition.type, condition = condition.Check});
            return this;
        }

        public ConditionBuilder Clear()
        {
            data.Clear();
            return this;
        }

        protected internal override bool Check()
        {
            bool value = data[0].condition();

            for (int i = 1; i < data.Count; i++)
            {
                var item = data[i];

                if (item.type == ConditionType.And)
                {
                    value &= item.condition();
                }
                else
                {
                    value |= item.condition();
                }
            }
                
            return value;
        }
    }
}
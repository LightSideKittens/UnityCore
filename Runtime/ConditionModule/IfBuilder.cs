using System;
using System.Collections.Generic;

namespace LSCore.ConditionModule
{
    public class IfBuilder : BaseIf
    {
        public static IfBuilder Default { get; } = If(() => false);
        private readonly List<Data> data = new();
        
        public struct Data
        {
            public ConditionType type;
            public Func<bool> condition;
        }
        
        private IfBuilder(){}
        
        public static IfBuilder If(Func<bool> condition)
        {
            var builder = new IfBuilder();
            builder.data.Add(new Data {type = default, condition = condition});
            return builder;
        }

        public IfBuilder And(Func<bool> condition)
        {
            data.Add(new Data {type = ConditionType.And, condition = condition});
            return this;
        }

        public IfBuilder Or(Func<bool> condition)
        {
            data.Add(new Data {type = ConditionType.Or, condition = condition});
            return this;
        }
        
        public static IfBuilder If(BaseIf @if)
        {
            var builder = new IfBuilder();
            builder.data.Add(new Data {type = default, condition = @if.Check});
            return builder;
        }

        public IfBuilder And(BaseIf @if)
        {
            data.Add(new Data {type = ConditionType.And, condition = @if.Check});
            return this;
        }

        public IfBuilder Or(BaseIf @if)
        {
            data.Add(new Data {type = ConditionType.Or, condition = @if.Check});
            return this;
        }
        
        public IfBuilder Add<T>(T condition) where T : If
        {
            data.Add(new Data {type = condition.type, condition = condition.Check});
            return this;
        }

        public IfBuilder Clear()
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
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace LSCore.ConditionModule
{
    [Serializable]
    public class ConditionsContainer : BaseCondition
    {
        private static class ConditionType
        {
            public const string Start = nameof(Start);
            public const string And = nameof(And);
            public const string Or = nameof(Or);
        }
        
        [Serializable]
        private struct ConditionData
        {
            public string conditionType;
            public BaseCondition condition;

            public ConditionData(BaseCondition condition, string conditionType)
            {
                this.condition = condition;
                this.conditionType = conditionType;
            }

            public void Reset()
            {
                condition.Reset();
            }
        }
        
        [JsonProperty] 
        [SerializeField]
        private List<ConditionData> conditions = new List<ConditionData>();

        
        public ConditionsContainer(BaseCondition condition)
        {
            conditions.Add(new ConditionData(condition, ConditionType.Start));
        }
        
        public ConditionsContainer And<T>() where T : BaseCondition, new()
        {
            conditions.Add(new ConditionData(new T(), ConditionType.And));
            
            return this;
        }
        
        public ConditionsContainer Or<T>() where T : BaseCondition, new()
        {
            conditions.Add(new ConditionData(new T(), ConditionType.Or));
            
            return this;
        }
        
        public ConditionsContainer And(BaseCondition condition)
        {
            conditions.Add(new ConditionData(condition, ConditionType.And));
            
            return this;
        }
        
        public ConditionsContainer Or(BaseCondition condition)
        {
            conditions.Add(new ConditionData(condition, ConditionType.Or));
            
            return this;
        }

        protected override bool IsCompleted 
        {
            get
            {
                bool value = conditions[0].condition;

                for (int i = 1; i < conditions.Count; i++)
                {
                    var data = conditions[i];

                    if (data.conditionType == ConditionType.And)
                    {
                        value &= data.condition;
                    }
                    else
                    {
                        value |= data.condition;
                    }
                }
                
                return value;
            }
            set{} 
        }

        public override void Reset()
        {
            foreach (var condition in conditions)
            {
                condition.Reset();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace LSCore.ConditionModule
{
    [Serializable]
    public class ConditionsContainer : BaseCondition
    {
        private enum ConditionType
        {
            Start,
            And,
            Or,
        }
        
        [Serializable]
        private struct ConditionData
        {
            public ConditionType type;
            [SerializeReference] 
            public BaseCondition condition;

            public ConditionData(BaseCondition condition, ConditionType type)
            {
                this.condition = condition;
                this.type = type;
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

                    if (data.type == ConditionType.And)
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
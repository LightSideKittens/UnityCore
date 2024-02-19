using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.ConditionModule
{
    [Serializable]
    public class Conditions<T> : Condition, ISerializationCallbackReceiver where T : Condition
    {
        [SerializeReference] public List<T> conditions;
        [NonSerialized] public ConditionBuilder conditionBuilder;

        protected internal override bool Check() => conditionBuilder;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if(conditions == null) return;
            if (conditions.Count == 0) return;
            
            conditionBuilder = ConditionBuilder.If(conditions[0]);
            
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i] != null)
                {
                    conditionBuilder.Add(conditions[i]);
                }
            }
        }
    }
}
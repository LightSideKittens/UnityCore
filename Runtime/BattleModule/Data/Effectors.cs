using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class Effectors : SerializedScriptableObject
    {
        [Serializable] 
        public class EffectorByName
        {
            [SerializeField, Id("Effectors")] public Id effectorName;
            [OdinSerialize] public BaseEffector effector;
        }
        
        [OdinSerialize, TableList] private List<EffectorByName> byName = new();
        public static Dictionary<Id, BaseEffector> ByName { get; } = new();

        public void Init()
        {
            for (int i = 0; i < byName.Count; i++)
            {
                var pair = byName[i];
                pair.effector.id = pair.effectorName;
                ByName.TryAdd(pair.effectorName, pair.effector);
            }
        }
    }
}
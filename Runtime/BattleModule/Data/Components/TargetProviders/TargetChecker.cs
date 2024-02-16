using System;
using LSCore.ConditionModule;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class TargetChecker : Condition
    {
        
    }
    
    [Serializable]
    public class TargetCheckerConditions : TargetChecker
    {
        [SerializeField] private Conditions conditions;
        protected override bool Check() => conditions;
    }
}
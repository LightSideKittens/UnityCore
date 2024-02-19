using System;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;

namespace LSCore.BattleModule
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class TargetChecker : Condition { }

    [Serializable]
    public class TargetCheckers : TargetChecker
    {
        [InlineProperty] [HideLabel] public Conditions<TargetChecker> conditions;
        protected override bool Check() => conditions;
    }
}
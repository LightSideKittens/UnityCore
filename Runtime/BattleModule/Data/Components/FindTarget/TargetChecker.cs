using System;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;

namespace LSCore.BattleModule
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class TargetChecker : If { }

    [Serializable]
    public class TargetCheckers : TargetChecker
    {
        [InlineProperty] [HideLabel] public Ifs<TargetChecker> ifs;
        protected override bool Check() => ifs;
    }
}
using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;

namespace LSCore.ConditionModule
{
    [Serializable]
    public abstract class Condition : BaseCondition
    {
        [HideLabel] [HideCondition] public ConditionType type;
    }
}
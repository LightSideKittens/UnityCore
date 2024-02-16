using System;
using Sirenix.OdinInspector;

namespace LSCore.ConditionModule
{
    [Serializable]
    public abstract class Condition : BaseCondition
    {
        [HideLabel] public ConditionType type;
    }
}
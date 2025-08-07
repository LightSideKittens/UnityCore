using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;

namespace LSCore.ConditionModule
{
    [Serializable]
    public abstract class If : BaseIf
    {
        [HideLabel] [HideCondition] public ConditionType type;
    }
    
    [Serializable]
    public class BoolIf : If
    {
        public bool value;
        
        protected internal override bool Check()
        {
            return value;
        }
    }
}